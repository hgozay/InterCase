using System.ComponentModel.DataAnnotations;

namespace InterCase.Common
{
    public class JwtOptions
    {
        public const string SectionName = "Jwt";

        [Required(ErrorMessage = "Jwt:Issuer is required.")]
        public string Issuer { get; set; } = string.Empty;

        [Required(ErrorMessage = "Jwt:Audience is required.")]
        public string Audience { get; set; } = string.Empty;

        [Required(ErrorMessage = "Jwt:Key is required.")]
        [MinLength(32, ErrorMessage = "Jwt:Key must be at least 32 characters for HS256.")]
        public string Key { get; set; } = string.Empty;

        [Range(1, 1440, ErrorMessage = "Jwt:ExpiryMinutes must be between 1 and 1440.")]
        public int ExpiryMinutes { get; set; } = 60;
    }

    public class RabbitMqOptions
    {
        public const string SectionName = "RabbitMq";

        [Required(ErrorMessage = "RabbitMq:HostName is required.")]
        public string HostName { get; set; } = string.Empty;

        [Range(1, 65535, ErrorMessage = "RabbitMq:Port must be between 1 and 65535.")]
        public int Port { get; set; } = 5672;

        [Required(ErrorMessage = "RabbitMq:UserName is required.")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "RabbitMq:Password is required.")]
        public string Password { get; set; } = string.Empty;

        [Range(1, 1000, ErrorMessage = "RabbitMq:PrefetchCount must be between 1 and 1000.")]
        public ushort PrefetchCount { get; set; } = 10;
    }

    public class ProcessingOptions
    {
        public const string SectionName = "Processing";

        [Range(0, 300, ErrorMessage = "Processing:DelaySeconds must be between 0 and 300.")]
        public int DelaySeconds { get; set; } = 3;

        [Range(1, 1440, ErrorMessage = "Processing:StalePendingMinutes must be between 1 and 1440.")]
        public int StalePendingMinutes { get; set; } = 2;
    }

    public class CacheOptions
    {
        public const string SectionName = "Cache";

        [EnumDataType(typeof(CacheProvider), ErrorMessage = "Cache:Provider is not a supported cache provider.")]
        public CacheProvider Provider { get; set; } = CacheProvider.Redis;

        [Range(1, 86400, ErrorMessage = "Cache:OrdersTtlSeconds must be between 1 and 86400.")]
        public int OrdersTtlSeconds { get; set; } = 120;
    }

    public class QueueOptions
    {
        public const string SectionName = "Queue";

        [EnumDataType(typeof(QueueProvider), ErrorMessage = "Queue:Provider is not a supported queue provider.")]
        public QueueProvider Provider { get; set; } = QueueProvider.RabbitMq;
    }

    public class HangfireOptions
    {
        public const string SectionName = "Hangfire";

        [Range(1, 100, ErrorMessage = "Hangfire:WorkerCount must be between 1 and 100.")]
        public int WorkerCount { get; set; } = 5;
    }

    public class DemoUserOptions
    {
        public const string SectionName = "DemoUser";

        [Required(ErrorMessage = "DemoUser:Username is required.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "DemoUser:Password is required.")]
        public string Password { get; set; } = string.Empty;
    }
}
