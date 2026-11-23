using System.Net;

namespace InterCase.UI.Models
{
    public sealed record ApiProblem
    {
        public string? Title { get; init; }
        public string? Detail { get; init; }
        public int? Status { get; init; }
        public Dictionary<string, string[]>? Errors { get; init; }
        public string? CorrelationId { get; init; }
    }

    public sealed record ApiResult<T>
    {
        public bool IsSuccess { get; private init; }
        public bool IsUnavailable { get; private init; }
        public HttpStatusCode? StatusCode { get; private init; }
        public T? Value { get; private init; }
        public ApiProblem? Problem { get; private init; }
        public string? CorrelationId { get; private init; }

        public bool IsUnauthorized => StatusCode == HttpStatusCode.Unauthorized;

        public static ApiResult<T> Success(T value, HttpStatusCode statusCode, string? correlationId) =>
            new() { IsSuccess = true, Value = value, StatusCode = statusCode, CorrelationId = correlationId };

        public static ApiResult<T> Failure(HttpStatusCode statusCode, ApiProblem problem, string? correlationId) =>
            new() { StatusCode = statusCode, Problem = problem, CorrelationId = problem.CorrelationId ?? correlationId };

        public static ApiResult<T> Unavailable() =>
            new() { IsUnavailable = true };
    }
}
