using Microsoft.AspNetCore.Http;

namespace AdminService.Services;

public class AuthTokenHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthTokenHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Extract JWT from incoming request and forward it
        var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"]
            .ToString();

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer", token.Replace("Bearer ", ""));
        }

        return await base.SendAsync(request, cancellationToken);
    }
}