namespace IdentityService.Application.DTOs;

public class LogoutRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class LogoutResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
}
