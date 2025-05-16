using System.Security.Claims;

namespace IdentityService.API.Extensions;

public static class ClaimsPrincipalExtensions
{    /// <summary>
    /// Lấy Id của người dùng từ claims
    /// </summary>
    public static string GetUserId(this ClaimsPrincipal user)
    {
        // Thử lấy từ cả 2 định dạng có thể có
        return user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
               user.FindFirst("sub")?.Value ?? 
               string.Empty;
    }    /// <summary>
    /// Lấy tên người dùng từ claims
    /// </summary>
    public static string GetUsername(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Name)?.Value ?? 
               user.FindFirst("unique_name")?.Value ?? 
               string.Empty;
    }    /// <summary>
    /// Lấy email từ claims
    /// </summary>
    public static string GetEmail(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Email)?.Value ?? 
               user.FindFirst("email")?.Value ?? 
               string.Empty;
    }    /// <summary>
    /// Kiểm tra người dùng có role không
    /// </summary>
    public static bool IsInRole(this ClaimsPrincipal user, string role)
    {
        return user.HasClaim(c => 
            (c.Type == ClaimTypes.Role || c.Type == "role") && 
            c.Value.Equals(role, StringComparison.OrdinalIgnoreCase));
    }
}