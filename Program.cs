using AdvancedRateLimitAPI.Interfaces;
using AdvancedRateLimitAPI.Middleware;
using AdvancedRateLimitAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddScoped<IApiRateLimitService, ApiRateLimitService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();

var app = builder.Build();

// Use Middleware (Middleware should not be added as a service)
app.UseMiddleware<RateLimitingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
