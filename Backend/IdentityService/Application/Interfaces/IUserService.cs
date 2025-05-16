using IdentityService.Application.DTOs;

namespace IdentityService.Application.Interfaces;

public interface IUserService
{
    Task<RegisterResponseDto> RegisterUserAsync(RegisterUserDto registerDto);
}
