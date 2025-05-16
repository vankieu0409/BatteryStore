namespace IdentityService.Application.DTOs;

public class RegisterRequest
{
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
}

public class RegisterResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
}
