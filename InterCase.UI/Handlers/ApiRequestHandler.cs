using System.Net.Http.Headers;
using InterCase.Common.Helpers;

namespace InterCase.UI.Handlers
{
    public sealed class ApiRequestHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var accessToken = httpContextAccessor.HttpContext?.User.FindFirst(UiClaimTypes.AccessToken)?.Value;
            if (!string.IsNullOrEmpty(accessToken))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            request.Headers.AcceptLanguage.Clear();
            request.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue(Localization.CurrentLanguage));

            return base.SendAsync(request, cancellationToken);
        }
    }
}
