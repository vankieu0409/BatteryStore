namespace IdentityService.Application.DTOs;

public class TwoFactorSetupRequest
{
    public string UserId { get; set; } = string.Empty;
}

public class TwoFactorVerifyRequest
{
    public string UserId { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

public class TwoFactorResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
}
