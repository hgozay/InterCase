using InterCase.Common;
using InterCase.Common.Helpers;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace InterCase.API.Handlers
{
    public sealed class GlobalExceptionHandler(
        IProblemDetailsService problemDetailsService,
        ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
            {
                logger.LogInformation("Request {Method} {Path} was cancelled by the client", httpContext.Request.Method, httpContext.Request.Path);
                httpContext.Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
                return true;
            }

            var problem = exception switch
            {
                NotFoundException => new ProblemDetails { Status = StatusCodes.Status404NotFound },
                ConflictException conflict => new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Detail = Localization.Get(conflict.MessageKey)
                },
                BadHttpRequestException => new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Detail = Localization.Get(Localization.Keys.InvalidRequestBody)
                },
                _ => new ProblemDetails { Status = StatusCodes.Status500InternalServerError }
            };

            if (problem.Status == StatusCodes.Status500InternalServerError)
                logger.LogError(exception, "Unhandled exception for {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
            else
                logger.LogWarning("{ExceptionType} for {Method} {Path}: {Message}", exception.GetType().Name, httpContext.Request.Method, httpContext.Request.Path, exception.Message);

            httpContext.Response.StatusCode = problem.Status!.Value;
            return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = problem
            });
        }
    }
}
