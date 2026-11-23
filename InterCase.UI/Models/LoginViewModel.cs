using InterCase.Common.Dtos.Users;

namespace InterCase.UI.Models
{
    public sealed class LoginViewModel
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? ReturnUrl { get; set; }
    }

    public sealed record DemoLoginViewModel(IReadOnlyList<UserResponse> Users, string Password);
}
