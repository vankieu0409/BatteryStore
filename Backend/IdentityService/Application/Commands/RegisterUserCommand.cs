using IdentityService.Application.DTOs;
using MediatR;

namespace IdentityService.Application.Commands;

public class RegisterUserCommand : IRequest<RegisterResponseDto>
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
}
