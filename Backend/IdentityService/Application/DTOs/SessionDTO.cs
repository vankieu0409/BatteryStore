namespace IdentityService.Application.DTOs;

public class SessionInfoResponse
{
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime Expires { get; set; }
    public bool IsRevoked { get; set; }
}

public class SessionListResponse
{
    public List<SessionInfoResponse> Sessions { get; set; } = new();
}
