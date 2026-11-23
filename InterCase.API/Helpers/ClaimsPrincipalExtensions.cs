using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace InterCase.API.Helpers
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid? GetUserId(this ClaimsPrincipal principal) =>
            Guid.TryParse(principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var userId) ? userId : null;

        public static bool Owns(this ClaimsPrincipal principal, Guid userId) =>
            principal.GetUserId() == userId;
    }
}
