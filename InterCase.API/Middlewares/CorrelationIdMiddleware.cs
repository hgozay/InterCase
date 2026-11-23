using System.Text.RegularExpressions;
using InterCase.Common;
using Serilog.Context;

namespace InterCase.API.Middlewares
{
    public sealed partial class CorrelationIdMiddleware(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context, ICorrelationContext correlation)
        {
            var incoming = context.Request.Headers[AppHeaderNames.CorrelationId].ToString();
            var correlationId = ValidCorrelationId().IsMatch(incoming) ? incoming : Guid.NewGuid().ToString("N");

            correlation.CorrelationId = correlationId;
            context.Response.OnStarting(() =>
            {
                context.Response.Headers[AppHeaderNames.CorrelationId] = correlationId;
                return Task.CompletedTask;
            });

            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                await next(context);
            }
        }

        [GeneratedRegex("^[A-Za-z0-9_-]{1,64}$")]
        private static partial Regex ValidCorrelationId();
    }
}
