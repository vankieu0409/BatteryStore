namespace IdentityService.Application.DTOs;

public class UserProfileResponse
{
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
}

public class UpdateUserProfileRequest
{
    public string? PhoneNumber { get; set; }
}
