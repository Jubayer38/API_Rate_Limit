using AdvancedRateLimitAPI.Interfaces;
using AdvancedRateLimitAPI.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdvancedRateLimitAPI.Middleware
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _scopeFactory;

        public RateLimitingMiddleware(RequestDelegate next, IMemoryCache cache, IConfiguration configuration, IServiceScopeFactory scopeFactory)
        {
            _next = next;
            _cache = cache;
            _configuration = configuration;
            _scopeFactory = scopeFactory;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var apiName = context.Request.Path.Value?.Split('/').Last();
            if (string.IsNullOrEmpty(apiName))
            {
                await _next(context);
                return;
            }

            // Simulate user from header if identity is not set
            var userId = context.User.Identity?.Name ?? context.Request.Headers["Authorization"].FirstOrDefault() ?? "global";

            using var scope = _scopeFactory.CreateScope();
            var rateLimitService = scope.ServiceProvider.GetRequiredService<IApiRateLimitService>();
            var limits = await rateLimitService.GetAllLimitsAsync();

            var apiLimits = limits.Where(x => x.ApiName.Equals(apiName, StringComparison.OrdinalIgnoreCase)).ToList();
            if (!apiLimits.Any())
            {
                await _next(context);
                return;
            }

            var isPeakTime = IsPeakTime(apiLimits);
            var applicableLimit = GetApplicableLimit(apiLimits, isPeakTime, userId);
            if (applicableLimit == null)
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("No applicable rate limit found!");
                return;
            }

            int maxLimit = (int)(applicableLimit.BaseLimit * 1.5);
            var cacheKey = $"{apiName}_{userId}_{isPeakTime}";
            var currentRequestCount = _cache.Get<int>(cacheKey);

            if (currentRequestCount >= maxLimit)
            {
                context.Response.StatusCode = 429;
                await context.Response.WriteAsync(
                    $@"Rate limit exceeded!
                    User: {(userId == "global" ? "Unauthenticated (Global)" : userId)}
                    Type: {(applicableLimit.IsGlobal ? "Global" : "User-specific")}
                    Time Slot: {(isPeakTime ? "Peak" : "Off-peak")}
                    Base Limit: {applicableLimit.BaseLimit}
                    Effective Max Limit: {maxLimit} requests per {applicableLimit.TimeUnit.ToLower()}
                    Try again later."
                );
                return;
            }

            _cache.Set(cacheKey, currentRequestCount + 1, GetCacheExpiration(applicableLimit));
            await _next(context);
        }

        private bool IsPeakTime(List<ApiRateLimit> apiLimits)
        {
            var now = DateTime.UtcNow;
            return apiLimits.Any(limit => limit.IsPeak && limit.EffectiveFrom <= now && limit.EffectiveTo >= now);
        }

        private ApiRateLimit? GetApplicableLimit(List<ApiRateLimit> apiLimits, bool isPeak, string userId)
        {
            return apiLimits.FirstOrDefault(x =>
                (x.IsGlobal || x.UserId?.Equals(userId, StringComparison.OrdinalIgnoreCase) == true)
                && x.IsPeak == isPeak);
        }

        private TimeSpan GetCacheExpiration(ApiRateLimit apiLimit)
        {
            return apiLimit.TimeUnit.ToLower() == "minute"
                ? TimeSpan.FromMinutes(1)
                : TimeSpan.FromHours(1);
        }
    }
}
