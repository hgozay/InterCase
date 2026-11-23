using InterCase.API.Helpers;
using InterCase.Common;
using InterCase.Common.Dtos.Auth;
using InterCase.Common.Dtos.Users;
using InterCase.Common.Helpers;
using InterCase.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace InterCase.API.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("auth")]
    public sealed class AuthController(
        IServiceManager serviceManager,
        IOptions<JwtOptions> jwtOptions,
        TimeProvider timeProvider,
        IWebHostEnvironment environment) : ControllerBase
    {
        [HttpPost("token")]
        public async Task<ActionResult<TokenResponse>> Token(TokenRequest request, CancellationToken ct)
        {
            var user = await serviceManager.User.AuthenticateAsync(request.Username, request.Password, ct);
            if (user is null)
            {
                return Problem(
                    statusCode: StatusCodes.Status401Unauthorized,
                    detail: Localization.Get(Localization.Keys.InvalidCredentials));
            }

            var token = JwtTokenHelper.CreateToken(jwtOptions.Value, user.Id, user.Username, timeProvider.GetUtcNow().UtcDateTime);
            await serviceManager.User.RecordLoginAsync(user.Id, token.TokenId, token.AccessToken, token.ExpiresAt, ct);

            return new TokenResponse(token.AccessToken, token.ExpiresAt, user.Id, user.Username, user.DisplayName);
        }

        [HttpGet("demo-users")]
        public async Task<ActionResult<IReadOnlyList<UserResponse>>> DemoUsers(CancellationToken ct)
        {
            if (!environment.IsDevelopment())
                return NotFound();

            return Ok(await serviceManager.User.GetDemoUsersAsync(ct));
        }
    }
}
