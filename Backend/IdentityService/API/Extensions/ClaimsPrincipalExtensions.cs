using System.Security.Claims;

namespace IdentityService.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// L?y Id c?a ng??i d�ng t? claims
    /// </summary>
    public static string GetUserId(this ClaimsPrincipal user)
    {
        // Th? l?y t? c? 2 ??nh d?ng c� th? c�
        return user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
               user.FindFirst("sub")?.Value ?? 
               string.Empty;
    }

    /// <summary>
    /// L?y t�n ng??i d�ng t? claims
    /// </summary>
    public static string GetUsername(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Name)?.Value ?? 
               user.FindFirst("unique_name")?.Value ?? 
               string.Empty;
    }

    /// <summary>
    /// L?y email t? claims
    /// </summary>
    public static string GetEmail(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Email)?.Value ?? 
               user.FindFirst("email")?.Value ?? 
               string.Empty;
    }

    /// <summary>
    /// Ki?m tra ng??i d�ng c� role kh�ng
    /// </summary>
    public static bool IsInRole(this ClaimsPrincipal user, string role)
    {
        return user.HasClaim(c => 
            (c.Type == ClaimTypes.Role || c.Type == "role") && 
            c.Value.Equals(role, StringComparison.OrdinalIgnoreCase));
    }
}