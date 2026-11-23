using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace InterCase.UI.Helpers
{
    public static class RabbitMqTokenHelper
    {
        public static string CreateToken(RabbitMqPanelOptions options, string username, DateTime nowUtc)
        {
            var resource = options.ResourceServerId;
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SigningKey!)) { KeyId = options.KeyId };

            var descriptor = new SecurityTokenDescriptor
            {
                Issuer = "InterCase.UI",
                Audience = resource,
                IssuedAt = nowUtc,
                NotBefore = nowUtc,
                Expires = nowUtc.AddMinutes(options.TokenLifetimeMinutes),
                Claims = new Dictionary<string, object>
                {
                    [JwtRegisteredClaimNames.Sub] = username,
                    ["preferred_username"] = username,
                    ["scope"] = $"{resource}.tag:administrator {resource}.read:*/* {resource}.write:*/* {resource}.configure:*/*"
                },
                SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
            };

            return new JsonWebTokenHandler().CreateToken(descriptor);
        }
    }
}
