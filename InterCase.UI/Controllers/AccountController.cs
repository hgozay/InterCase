using System.Security.Claims;
using InterCase.Common;
using InterCase.Common.Dtos.Auth;
using InterCase.UI.Clients;
using InterCase.UI.Helpers;
using InterCase.UI.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace InterCase.UI.Controllers
{
    public sealed class AccountController(
        ApiClient apiClient,
        IOptions<DemoUserOptions> demoUserOptions,
        IWebHostEnvironment environment) : Controller
    {
        [AllowAnonymous]
        [HttpGet("login")]
        public async Task<IActionResult> Login(string? returnUrl, bool expired = false, CancellationToken ct = default)
        {
            if (User.Identity?.IsAuthenticated == true && User.GetUserId() is not null)
                return RedirectToLocal(returnUrl);

            if (expired)
                ViewData["Notice"] = UiText.SessionExpired;

            return await LoginViewAsync(new LoginViewModel { ReturnUrl = returnUrl }, ct);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginViewModel model, CancellationToken ct)
        {
            var request = new TokenRequest
            {
                Username = model.Username?.Trim() ?? string.Empty,
                Password = model.Password ?? string.Empty
            };
            var result = await apiClient.CreateTokenAsync(request, ct);

            if (result.IsSuccess)
            {
                var token = result.Value!;
                var identity = new ClaimsIdentity(
                    [
                        new Claim(ClaimTypes.Name, token.DisplayName),
                        new Claim(UiClaimTypes.UserId, token.UserId.ToString()),
                        new Claim(UiClaimTypes.Username, token.Username),
                        new Claim(UiClaimTypes.AccessToken, token.AccessToken)
                    ],
                    CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(identity),
                    new AuthenticationProperties
                    {
                        ExpiresUtc = DisplayFormat.AsUtc(token.ExpiresAt),
                        IsPersistent = false,
                        AllowRefresh = false
                    });

                return RedirectToLocal(model.ReturnUrl);
            }

            ModelState.Clear();
            ModelState.AddApiErrors(result.Problem, string.Empty, nameof(LoginViewModel.Username), nameof(LoginViewModel.Password));
            ViewData["Error"] = result.IsUnavailable
                ? UiText.ApiUnavailable
                : result.Problem?.Detail ?? result.Problem?.Title ?? UiText.UnexpectedApiResponse;

            return await LoginViewAsync(model, ct);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        private async Task<ViewResult> LoginViewAsync(LoginViewModel model, CancellationToken ct)
        {
            var demoPassword = demoUserOptions.Value.Password;
            if (environment.IsDevelopment() && !string.IsNullOrEmpty(demoPassword))
            {
                var demoUsers = await apiClient.GetDemoUsersAsync(ct);
                if (demoUsers.IsSuccess && demoUsers.Value!.Count > 0)
                    ViewData["DemoLogin"] = new DemoLoginViewModel(demoUsers.Value!, demoPassword);
            }

            return View(nameof(Login), model);
        }

        private IActionResult RedirectToLocal(string? returnUrl) =>
            Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl) : RedirectToAction(nameof(OrdersController.Index), "Orders");
    }
}
