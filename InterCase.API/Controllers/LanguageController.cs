using InterCase.Common.Helpers;
using InterCase.Common.Dtos.Languages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace InterCase.API.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("language")]
    public sealed class LanguageController : ControllerBase
    {
        [HttpGet]
        public ActionResult<LanguageResponse> Get() =>
            new LanguageResponse(Localization.CurrentLanguage, Localization.SupportedLanguages);

        [HttpPut]
        public ActionResult<LanguageResponse> Set(SetLanguageRequest request)
        {
            var language = Localization.Normalize(request.Language);

            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(language)),
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    HttpOnly = true,
                    IsEssential = true,
                    SameSite = SameSiteMode.Lax
                });

            return new LanguageResponse(language, Localization.SupportedLanguages);
        }
    }
}
