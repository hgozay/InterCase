using System.ComponentModel.DataAnnotations;
using InterCase.Common.Helpers;

namespace InterCase.Common.Dtos.Languages
{
    public sealed record SetLanguageRequest
    {
        [Required(ErrorMessage = Localization.Keys.LanguageRequired)]
        [RegularExpression("^(" + Localization.English + "|" + Localization.Turkish + ")$",
            ErrorMessage = Localization.Keys.LanguageNotSupported)]
        public string Language { get; init; } = string.Empty;
    }
}
