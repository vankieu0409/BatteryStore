namespace IdentityService.Application.DTOs;

public class ExternalLoginRequest
{
    public string Provider { get; set; } = string.Empty;
    public string ProviderKey { get; set; } = string.Empty;
    public string? AccessToken { get; set; }
}

public class ExternalLoginResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
}
