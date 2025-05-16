namespace IdentityService.Application.DTOs;

public class EmailVerificationRequest
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}

public class PhoneVerificationRequest
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}

public class VerificationResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
}
