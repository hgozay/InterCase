using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterCase.UI.Controllers
{
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    public sealed class ErrorController : BaseController
    {
        [Route("error/{statusCode:int?}")]
        public IActionResult Index(int? statusCode)
        {
            Response.StatusCode = statusCode ?? StatusCodes.Status500InternalServerError;
            return View("Error", Response.StatusCode);
        }
    }
}
