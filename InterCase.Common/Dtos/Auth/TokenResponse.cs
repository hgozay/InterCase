namespace InterCase.Common.Dtos.Auth
{
    public sealed record TokenResponse(
        string AccessToken,
        DateTime ExpiresAt,
        Guid UserId,
        string Username,
        string DisplayName)
    {
        public string TokenType { get; init; } = "Bearer";
    }
}
