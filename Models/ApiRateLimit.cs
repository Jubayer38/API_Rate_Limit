namespace AdvancedRateLimitAPI.Models
{
    public class ApiRateLimit
    {
        public int Id { get; set; }
        public string? ApiName { get; set; }
        public string? UserId { get; set; }
        public int BaseLimit { get; set; }
        public int MaxLimit { get; set; }
        public string? TimeUnit { get; set; } // "minute" or "hour"
        public bool IsGlobal { get; set; }
        public bool IsPeak { get; set; }
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
    }
}
