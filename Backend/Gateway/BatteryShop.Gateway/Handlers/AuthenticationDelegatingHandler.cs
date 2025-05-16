using System.Net.Http.Headers;

namespace BatteryShop.Gateway.Handlers;

/// <summary>
/// Handler ?? chuy?n ti?p authentication headers t? client ??n các microservices
/// </summary>
public class AuthenticationDelegatingHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthenticationDelegatingHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        
        // L?y token t? Authorization header c?a request g?c
        if (httpContext?.Request.Headers.TryGetValue("Authorization", out var authHeader) == true)
        {
            request.Headers.Add("Authorization", authHeader.ToArray());
        }

        // L?y token t? cookie (n?u dùng HttpOnly cookie cho authentication)
        else if (httpContext?.Request.Cookies.TryGetValue("AccessToken", out var token) == true)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}