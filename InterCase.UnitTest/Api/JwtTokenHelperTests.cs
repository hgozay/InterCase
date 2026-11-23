using InterCase.API.Helpers;
using InterCase.Common;
using Microsoft.IdentityModel.JsonWebTokens;

namespace InterCase.UnitTest.Api
{
    [TestClass]
    public sealed class JwtTokenHelperTests
    {
        private static readonly JwtOptions Options = new()
        {
            Issuer = "intercase-test",
            Audience = "intercase-test-clients",
            Key = "unit-test-signing-key-that-is-long-enough-for-hs256",
            ExpiryMinutes = 60
        };

        private static Task<Microsoft.IdentityModel.Tokens.TokenValidationResult> ValidateAsync(string token, JwtOptions options) =>
            new JsonWebTokenHandler().ValidateTokenAsync(token, JwtTokenHelper.CreateValidationParameters(options));

        [TestMethod]
        public async Task CreateToken_ProducesATokenThatCarriesUserIdNameAndTokenId()
        {
            var userId = Guid.NewGuid();

            var issued = JwtTokenHelper.CreateToken(Options, userId, "demo01", DateTime.UtcNow);
            var result = await ValidateAsync(issued.AccessToken, Options);

            Assert.IsTrue(result.IsValid, result.Exception?.Message);
            Assert.AreEqual(userId.ToString(), result.Claims[JwtRegisteredClaimNames.Sub]);
            Assert.AreEqual(issued.TokenId, result.Claims[JwtRegisteredClaimNames.Jti]);
            Assert.AreEqual("demo01", result.ClaimsIdentity.Name);
        }

        [TestMethod]
        public void CreateToken_ExpiresAfterTheConfiguredMinutes()
        {
            var now = new DateTime(2026, 11, 23, 12, 0, 0, DateTimeKind.Utc);

            var issued = JwtTokenHelper.CreateToken(Options, Guid.NewGuid(), "demo01", now);

            Assert.AreEqual(now.AddMinutes(Options.ExpiryMinutes), issued.ExpiresAt);
            Assert.AreEqual(issued.ExpiresAt, new JsonWebToken(issued.AccessToken).ValidTo);
        }

        [TestMethod]
        public void CreateToken_GivesEveryTokenItsOwnId()
        {
            var first = JwtTokenHelper.CreateToken(Options, Guid.NewGuid(), "demo01", DateTime.UtcNow);
            var second = JwtTokenHelper.CreateToken(Options, Guid.NewGuid(), "demo01", DateTime.UtcNow);

            Assert.AreNotEqual(first.TokenId, second.TokenId);
        }

        [TestMethod]
        public async Task Validation_WhenTokenHasExpired_Fails()
        {
            var issued = JwtTokenHelper.CreateToken(Options, Guid.NewGuid(), "demo01", DateTime.UtcNow.AddHours(-2));

            var result = await ValidateAsync(issued.AccessToken, Options);

            Assert.IsFalse(result.IsValid);
        }

        [TestMethod]
        public async Task Validation_WhenSignedWithAnotherKey_Fails()
        {
            var other = new JwtOptions { Issuer = Options.Issuer, Audience = Options.Audience, Key = "another-signing-key-that-is-also-long-enough-123", ExpiryMinutes = 60 };
            var issued = JwtTokenHelper.CreateToken(other, Guid.NewGuid(), "demo01", DateTime.UtcNow);

            var result = await ValidateAsync(issued.AccessToken, Options);

            Assert.IsFalse(result.IsValid);
        }

        [TestMethod]
        public async Task Validation_WhenAudienceDiffers_Fails()
        {
            var other = new JwtOptions { Issuer = Options.Issuer, Audience = "someone-else", Key = Options.Key, ExpiryMinutes = 60 };
            var issued = JwtTokenHelper.CreateToken(other, Guid.NewGuid(), "demo01", DateTime.UtcNow);

            var result = await ValidateAsync(issued.AccessToken, Options);

            Assert.IsFalse(result.IsValid);
        }
    }
}
