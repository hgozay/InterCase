using System.Security.Claims;
using System.Text;
using InterCase.Common;
using InterCase.Common.Dtos.Auth;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace InterCase.API.Helpers
{
    public static class JwtTokenHelper
    {
        public static TokenResponse CreateToken(JwtOptions options, string username, DateTime nowUtc)
        {
            var expiresAt = nowUtc.AddMinutes(options.ExpiryMinutes);
            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                [
                    new Claim(JwtRegisteredClaimNames.Sub, username),
                    new Claim(JwtRegisteredClaimNames.Name, username),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
                ]),
                Issuer = options.Issuer,
                Audience = options.Audience,
                IssuedAt = nowUtc,
                NotBefore = nowUtc,
                Expires = expiresAt,
                SigningCredentials = new SigningCredentials(CreateSigningKey(options), SecurityAlgorithms.HmacSha256)
            };

            return new TokenResponse(new JsonWebTokenHandler().CreateToken(descriptor), expiresAt);
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
