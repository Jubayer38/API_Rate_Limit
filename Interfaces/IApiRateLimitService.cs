using AdvancedRateLimitAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AdvancedRateLimitAPI.Interfaces
{
    public interface IApiRateLimitService
    {
        Task<List<ApiRateLimit>> GetAllLimitsAsync();
    }
}
