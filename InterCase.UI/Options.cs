using System.ComponentModel.DataAnnotations;

namespace InterCase.UI
{
    public class ApiOptions
    {
        public const string SectionName = "Api";

        [Required(ErrorMessage = "Api:BaseUrl is required.")]
        [Url(ErrorMessage = "Api:BaseUrl must be an absolute URL.")]
        public string BaseUrl { get; set; } = string.Empty;

        [Range(1, 120, ErrorMessage = "Api:TimeoutSeconds must be between 1 and 120.")]
        public int TimeoutSeconds { get; set; } = 10;
    }

    public class RabbitMqPanelOptions
    {
        public const string SectionName = "RabbitMqPanel";

        [Url(ErrorMessage = "RabbitMqPanel:Url must be an absolute URL.")]
        public string? Url { get; set; }

        public string KeyId { get; set; } = "intercase-ui";

        [MinLength(32, ErrorMessage = "RabbitMqPanel:SigningKey must be at least 32 characters for HS256.")]
        public string? SigningKey { get; set; }

        public string ResourceServerId { get; set; } = "rabbitmq";

        [Range(1, 1440, ErrorMessage = "RabbitMqPanel:TokenLifetimeMinutes must be between 1 and 1440.")]
        public int TokenLifetimeMinutes { get; set; } = 480;

        public bool IsConfigured => !string.IsNullOrWhiteSpace(Url) && !string.IsNullOrWhiteSpace(SigningKey);
    }
}
