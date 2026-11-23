namespace InterCase.Common.Dtos.Auth
{
    public sealed record TokenResponse(string AccessToken, DateTime ExpiresAt)
    {
        public string TokenType { get; init; } = "Bearer";
    }
}
