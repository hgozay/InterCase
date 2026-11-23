using System.Security.Claims;

namespace InterCase.UI.Helpers
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid? GetUserId(this ClaimsPrincipal principal) =>
            Guid.TryParse(principal.FindFirst(UiClaimTypes.UserId)?.Value, out var userId) ? userId : null;
    }
}
