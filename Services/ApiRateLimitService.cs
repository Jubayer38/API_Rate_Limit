using AdvancedRateLimitAPI.Models;
using AdvancedRateLimitAPI.Interfaces;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace AdvancedRateLimitAPI.Services
{
    public class ApiRateLimitService : IApiRateLimitService
    {
        private readonly string _connectionString;

        public ApiRateLimitService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("OracleDBConnection")
                ?? throw new ArgumentNullException("Missing OracleDBConnection string.");
        }

        public async Task<List<ApiRateLimit>> GetAllLimitsAsync()
        {
            var limits = new List<ApiRateLimit>();

            using var connection = new OracleConnection(_connectionString);
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM ApiRateLimits";

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                limits.Add(new ApiRateLimit
                {
                    Id = reader.GetInt32(0),
                    ApiName = reader.GetString(1),
                    UserId = reader.IsDBNull(2) ? null : reader.GetString(2),
                    BaseLimit = reader.GetInt32(3),
                    TimeUnit = reader.GetString(4),
                    IsGlobal = reader.GetInt32(5) == 1,
                    IsPeak = reader.GetInt32(6) == 1,
                    EffectiveFrom = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                    EffectiveTo = reader.IsDBNull(8) ? null : reader.GetDateTime(8)
                });
            }

            return limits;
        }
    }
}
