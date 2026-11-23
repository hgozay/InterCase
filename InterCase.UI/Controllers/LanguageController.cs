using InterCase.Common.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace InterCase.UI.Controllers
{
    [AllowAnonymous]
    public sealed class LanguageController : BaseController
    {
        [HttpPost("language")]
        public IActionResult Set(string? language, string? returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(Localization.Normalize(language))),
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    HttpOnly = true,
                    IsEssential = true,
                    SameSite = SameSiteMode.Lax
                });

            return RedirectToLocal(returnUrl);
        }
    }
}
