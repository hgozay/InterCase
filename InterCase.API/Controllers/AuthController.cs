using InterCase.API.Helpers;
using InterCase.Common;
using InterCase.Common.Dtos.Auth;
using InterCase.Common.Dtos.Users;
using InterCase.Common.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace InterCase.API.Controllers
{
    [AllowAnonymous]
    [Route("auth")]
    public sealed class AuthController(
        IOptions<JwtOptions> jwtOptions,
        TimeProvider timeProvider,
        IWebHostEnvironment environment) : BaseController
    {
        [HttpPost("token")]
        public async Task<ActionResult<TokenResponse>> Token(TokenRequest request, CancellationToken ct)
        {
            var user = await ServiceManager.User.AuthenticateAsync(request.Username, request.Password, ct);
            if (user is null)
            {
                return Problem(
                    statusCode: StatusCodes.Status401Unauthorized,
                    detail: Localization.Get(Localization.Keys.InvalidCredentials));
            }

            var token = JwtTokenHelper.CreateToken(jwtOptions.Value, user.Id, user.Username, timeProvider.GetUtcNow().UtcDateTime);
            await ServiceManager.User.RecordLoginAsync(user.Id, token.TokenId, token.AccessToken, token.ExpiresAt, ct);

            return new TokenResponse(token.AccessToken, token.ExpiresAt, user.Id, user.Username, user.DisplayName);
        }

        [HttpGet("demo-users")]
        public async Task<ActionResult<IReadOnlyList<UserResponse>>> DemoUsers(CancellationToken ct)
        {
            if (!environment.IsDevelopment())
                return NotFound();

            return Ok(await ServiceManager.User.GetDemoUsersAsync(ct));
        }
    }
}
