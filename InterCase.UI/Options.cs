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
}
