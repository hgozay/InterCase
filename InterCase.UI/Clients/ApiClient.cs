using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using InterCase.Common;
using InterCase.Common.Dtos.Auth;
using InterCase.Common.Dtos.Notifications;
using InterCase.Common.Dtos.Orders;
using InterCase.Common.Dtos.Paging;
using InterCase.Common.Dtos.Users;
using InterCase.UI.Helpers;
using InterCase.UI.Models;

namespace InterCase.UI.Clients
{
    public sealed class ApiClient(HttpClient httpClient, ILogger<ApiClient> logger)
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
        {
            Converters = { new JsonStringEnumConverter() }
        };

        public Task<ApiResult<TokenResponse>> CreateTokenAsync(TokenRequest request, CancellationToken ct) =>
            SendAsync<TokenResponse>(HttpMethod.Post, "auth/token", request, ct);

        public Task<ApiResult<List<UserResponse>>> GetDemoUsersAsync(CancellationToken ct) =>
            SendAsync<List<UserResponse>>(HttpMethod.Get, "auth/demo-users", null, ct);

        public Task<ApiResult<OrderResponse>> CreateOrderAsync(OrderFormModel form, CancellationToken ct)
        {
            var payload = new
            {
                userId = NullIfBlank(form.UserId),
                productId = NullIfBlank(form.ProductId),
                quantity = form.Quantity,
                paymentMethod = form.PaymentMethod
            };

            return SendAsync<OrderResponse>(HttpMethod.Post, "orders", payload, ct);
        }

        public Task<ApiResult<List<OrderResponse>>> GetOrdersAsync(Guid userId, CancellationToken ct) =>
            SendAsync<List<OrderResponse>>(HttpMethod.Get, $"orders/{userId}", null, ct);

        public Task<ApiResult<PagedResponse<OrderLogResponse>>> GetOrderLogsAsync(Guid userId, int page, int pageSize, CancellationToken ct) =>
            SendAsync<PagedResponse<OrderLogResponse>>(HttpMethod.Get, $"orders/{userId}/logs?page={page}&pageSize={pageSize}", null, ct);

        public Task<ApiResult<PagedResponse<NotificationResponse>>> GetNotificationsAsync(Guid userId, int page, int pageSize, CancellationToken ct) =>
            SendAsync<PagedResponse<NotificationResponse>>(HttpMethod.Get, $"notifications/{userId}?page={page}&pageSize={pageSize}", null, ct);

        private async Task<ApiResult<T>> SendAsync<T>(HttpMethod method, string path, object? body, CancellationToken ct)
        {
            using var request = new HttpRequestMessage(method, path);
            if (body is not null)
                request.Content = JsonContent.Create(body, body.GetType(), options: JsonOptions);

            try
            {
                using var response = await httpClient.SendAsync(request, ct);
                var correlationId = response.Headers.TryGetValues(AppHeaderNames.CorrelationId, out var values)
                    ? values.FirstOrDefault()
                    : null;

                if (response.IsSuccessStatusCode)
                {
                    var value = await response.Content.ReadFromJsonAsync<T>(JsonOptions, ct)
                        ?? throw new JsonException($"{method} {path} returned an empty body.");
                    return ApiResult<T>.Success(value, response.StatusCode, correlationId);
                }

                var problem = await ReadProblemAsync(response, ct);
                logger.LogInformation("{Method} {Path} returned {StatusCode} ({CorrelationId})", method, path, (int)response.StatusCode, problem.CorrelationId ?? correlationId);
                return ApiResult<T>.Failure(response.StatusCode, problem, correlationId);
            }
            catch (Exception ex) when (ex is HttpRequestException || (ex is TaskCanceledException && !ct.IsCancellationRequested))
            {
                logger.LogWarning(ex, "InterCase API is unreachable at {BaseAddress}", httpClient.BaseAddress);
                return ApiResult<T>.Unavailable();
            }
        }

        private static async Task<ApiProblem> ReadProblemAsync(HttpResponseMessage response, CancellationToken ct)
        {
            try
            {
                var problem = await response.Content.ReadFromJsonAsync<ApiProblem>(JsonOptions, ct);
                if (problem is not null)
                    return problem;
            }
            catch (JsonException)
            {
            }

            return new ApiProblem { Title = UiText.UnexpectedApiResponse, Status = (int)response.StatusCode };
        }

        private static string? NullIfBlank(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
