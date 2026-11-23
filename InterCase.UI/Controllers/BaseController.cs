using InterCase.UI.Clients;
using InterCase.UI.Helpers;
using InterCase.UI.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterCase.UI.Controllers
{
    [Authorize]
    public abstract class BaseController : Controller
    {
        private ApiClient? _apiClient;

        protected ApiClient ApiClient =>
            _apiClient ??= HttpContext.RequestServices.GetRequiredService<ApiClient>();

        protected async Task<IActionResult> ExpireSessionAsync()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(AccountController.Login), "Account", new { expired = true });
        }

        protected IActionResult RedirectToLocal(string? returnUrl) =>
            Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl) : RedirectToAction(nameof(OrdersController.Index), "Orders");

        protected void ShowApiErrors(ApiProblem? problem, string prefix, params string[] fields)
        {
            ModelState.Clear();
            ModelState.AddApiErrors(problem, prefix, fields);
        }

        protected static string DescribeFailure<T>(ApiResult<T> result) =>
            result.IsUnavailable
                ? UiText.ApiUnavailable
                : result.Problem?.Detail ?? result.Problem?.Title ?? UiText.UnexpectedApiResponse;

        protected static string DescribeError<T>(ApiResult<T> result)
        {
            if (result.IsUnavailable)
                return UiText.ApiUnavailable;

            var fieldMessage = result.Problem?.Errors?.Values.SelectMany(m => m).FirstOrDefault();
            return fieldMessage ?? result.Problem?.Detail ?? result.Problem?.Title ?? UiText.UnexpectedApiResponse;
        }
    }
}
