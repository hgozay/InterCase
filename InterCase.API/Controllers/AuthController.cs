using System.Security.Cryptography;
using System.Text;
using InterCase.API.Helpers;
using InterCase.Common;
using InterCase.Common.Dtos.Auth;
using InterCase.Common.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace InterCase.API.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("auth")]
    public sealed class AuthController(
        IOptions<JwtOptions> jwtOptions,
        IOptions<DemoUserOptions> demoUserOptions,
        TimeProvider timeProvider,
        ILogger<AuthController> logger) : ControllerBase
    {
        [HttpPost("token")]
        [ProducesResponseType<TokenResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
        public ActionResult<TokenResponse> Token(TokenRequest request)
        {
            var demoUser = demoUserOptions.Value;
            var isValid = FixedTimeEquals(request.Username, demoUser.Username)
                & FixedTimeEquals(request.Password, demoUser.Password);

            if (!isValid)
            {
                logger.LogWarning("Token request rejected for {Username}", request.Username);
                return Problem(
                    statusCode: StatusCodes.Status401Unauthorized,
                    detail: Localization.Get(Localization.Keys.InvalidCredentials));
            }

            var token = JwtTokenHelper.CreateToken(jwtOptions.Value, demoUser.Username, timeProvider.GetUtcNow().UtcDateTime);
            logger.LogInformation("Token issued for {Username}, expires at {ExpiresAt}", demoUser.Username, token.ExpiresAt);
            return token;
        }

        private static bool FixedTimeEquals(string left, string right) =>
            CryptographicOperations.FixedTimeEquals(
                SHA256.HashData(Encoding.UTF8.GetBytes(left)),
                SHA256.HashData(Encoding.UTF8.GetBytes(right)));
    }
}
