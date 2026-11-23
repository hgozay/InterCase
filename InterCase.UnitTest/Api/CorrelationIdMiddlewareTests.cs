using InterCase.API.Middlewares;
using InterCase.Common;
using Microsoft.AspNetCore.Http;

namespace InterCase.UnitTest.Api
{
    [TestClass]
    public sealed class CorrelationIdMiddlewareTests
    {
        private static async Task<(string CorrelationId, bool NextCalled)> InvokeAsync(string? incoming)
        {
            var nextCalled = false;
            var middleware = new CorrelationIdMiddleware(_ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });
            var httpContext = new DefaultHttpContext();
            if (incoming is not null)
                httpContext.Request.Headers[AppHeaderNames.CorrelationId] = incoming;
            var correlation = new CorrelationContext();

            await middleware.InvokeAsync(httpContext, correlation);

            return (correlation.CorrelationId, nextCalled);
        }

        [TestMethod]
        [DataRow("abc-123_XYZ")]
        [DataRow("9f2c4a1e0b7d4c3e8a5f6b2d1c0e9a8b")]
        public async Task WhenIncomingIdIsSafe_KeepsIt(string incoming)
        {
            var (correlationId, nextCalled) = await InvokeAsync(incoming);

            Assert.AreEqual(incoming, correlationId);
            Assert.IsTrue(nextCalled);
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("has space")]
        [DataRow("<script>alert(1)</script>")]
        [DataRow("line\nbreak")]
        public async Task WhenIncomingIdIsMissingOrUnsafe_GeneratesANewOne(string? incoming)
        {
            var (correlationId, nextCalled) = await InvokeAsync(incoming);

            Assert.AreNotEqual(incoming, correlationId);
            Assert.AreEqual(32, correlationId.Length);
            Assert.IsTrue(Guid.TryParseExact(correlationId, "N", out _));
            Assert.IsTrue(nextCalled);
        }

        [TestMethod]
        public async Task WhenIncomingIdIsLongerThan64Characters_GeneratesANewOne()
        {
            var (correlationId, _) = await InvokeAsync(new string('a', 65));

            Assert.AreEqual(32, correlationId.Length);
        }
    }
}
