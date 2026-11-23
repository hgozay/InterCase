namespace InterCase.Common.Dtos.Languages
{
    public sealed record LanguageResponse(string Current, IReadOnlyList<string> Supported);
}
