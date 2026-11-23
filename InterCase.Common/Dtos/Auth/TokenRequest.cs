namespace InterCase.Common.Dtos.Auth
{
    public sealed record TokenRequest
    {
        public required string Username { get; init; }
        public required string Password { get; init; }
    }
}
