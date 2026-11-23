using System.ComponentModel.DataAnnotations;

namespace InterCase.Common
{
    public class JwtOptions
    {
        public const string SectionName = "Jwt";

        [Required]
        public string Issuer { get; set; } = string.Empty;

        [Required]
        public string Audience { get; set; } = string.Empty;

        [Required, MinLength(32)]
        public string Key { get; set; } = string.Empty;

        [Range(1, 1440)]
        public int ExpiryMinutes { get; set; } = 60;
    }

    public class RabbitMqOptions
    {
        public const string SectionName = "RabbitMq";

        [Required]
        public string HostName { get; set; } = string.Empty;

        [Range(1, 65535)]
        public int Port { get; set; } = 5672;

        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Range(1, 1000)]
        public ushort PrefetchCount { get; set; } = 10;
    }

    public class ProcessingOptions
    {
        public const string SectionName = "Processing";

        [Range(0, 300)]
        public int DelaySeconds { get; set; } = 3;

        [Range(1, 1440)]
        public int StalePendingMinutes { get; set; } = 2;
    }

    public class CacheOptions
    {
        public const string SectionName = "Cache";

        [Range(1, 86400)]
        public int OrdersTtlSeconds { get; set; } = 120;
    }

    public class HangfireOptions
    {
        public const string SectionName = "Hangfire";

        [Range(1, 100)]
        public int WorkerCount { get; set; } = 5;
    }

    public class DemoUserOptions
    {
        public const string SectionName = "DemoUser";

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
