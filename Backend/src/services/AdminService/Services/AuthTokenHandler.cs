using Microsoft.AspNetCore.Http;

namespace AdminService.Services;

/// <summary>
/// Forwards incoming bearer token to downstream service-to-service HTTP requests.
/// </summary>
public class AuthTokenHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthTokenHandler"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">The HTTP context accessor.</param>
    public AuthTokenHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Attaches the caller's JWT access token to outgoing requests when available.
    /// </summary>
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