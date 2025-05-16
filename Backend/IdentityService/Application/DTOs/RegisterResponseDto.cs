namespace IdentityService.Application.DTOs;

public class RegisterResponseDto
{
    public bool Success { get; set; }
    public UserDto? User { get; set; }
    public string? ErrorMessage { get; set; }
}
