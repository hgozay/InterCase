using System.Security.Claims;
using System.Text;
using InterCase.Common;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace InterCase.API.Helpers
{
    public sealed record IssuedToken(string AccessToken, string TokenId, DateTime ExpiresAt);

    public static class JwtTokenHelper
    {
        public static IssuedToken CreateToken(JwtOptions options, Guid userId, string username, DateTime nowUtc)
        {
            var tokenId = Guid.NewGuid().ToString("N");
            var expiresAt = nowUtc.AddMinutes(options.ExpiryMinutes);
            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                [
                    new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                    new Claim(JwtRegisteredClaimNames.Name, username),
                    new Claim(JwtRegisteredClaimNames.Jti, tokenId)
                ]),
                Issuer = options.Issuer,
                Audience = options.Audience,
                IssuedAt = nowUtc,
                NotBefore = nowUtc,
                Expires = expiresAt,
                SigningCredentials = new SigningCredentials(CreateSigningKey(options), SecurityAlgorithms.HmacSha256)
            };

            return new IssuedToken(new JsonWebTokenHandler().CreateToken(descriptor), tokenId, expiresAt);
        }

        public static TokenValidationParameters CreateValidationParameters(JwtOptions options) => new()
        {
            ValidateIssuer = true,
            ValidIssuer = options.Issuer,
            ValidateAudience = true,
            ValidAudience = options.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = CreateSigningKey(options),
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = JwtRegisteredClaimNames.Name
        };

        private static SymmetricSecurityKey CreateSigningKey(JwtOptions options) =>
            new(Encoding.UTF8.GetBytes(options.Key));
    }
}
