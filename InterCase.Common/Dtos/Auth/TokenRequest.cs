using System.ComponentModel.DataAnnotations;
using InterCase.Common.Helpers;

namespace InterCase.Common.Dtos.Auth
{
    public sealed record TokenRequest
    {
        [Required(ErrorMessage = Localization.Keys.UsernameRequired)]
        public string Username { get; init; } = string.Empty;

        [Required(ErrorMessage = Localization.Keys.PasswordRequired)]
        public string Password { get; init; } = string.Empty;
    }
}
