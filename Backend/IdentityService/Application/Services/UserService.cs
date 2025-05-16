using IdentityService.Application.DTOs;
using IdentityService.Application.Interfaces;
using IdentityService.Domain.Common;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Interfaces;
using IdentityService.Domain.Services;
using IdentityService.Domain.ValueObjects;

namespace IdentityService.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IDomainEventDispatcher _eventDispatcher;

    public UserService(
        IUserRepository userRepository, 
        IRoleRepository roleRepository, 
        IPasswordHasher passwordHasher,
        IDomainEventDispatcher eventDispatcher)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<RegisterResponseDto> RegisterUserAsync(RegisterUserDto registerDto)
    {
        try
        {
            // Kiểm tra xem username hoặc email đã tồn tại chưa
            if (await _userRepository.ExistsAsync(registerDto.Username, registerDto.Email))
            {
                return new RegisterResponseDto
                {
                    Success = false,
                    ErrorMessage = "Username or email already exists"
                };
            }
            
            // Tạo email value object
            var email = Email.Create(registerDto.Email);
            
            // Hash mật khẩu
            var passwordHash = _passwordHasher.HashPassword(registerDto.Password);
            
            // Tạo user mới (từ factory method trong domain)
            var user = User.Create(registerDto.Username, email, passwordHash);
            
            // Thiết lập thông tin bổ sung
            if (!string.IsNullOrWhiteSpace(registerDto.PhoneNumber))
            {
                // Giả sử có setter trong domain object hoặc method để thiết lập phone number
                // user.SetPhoneNumber(registerDto.PhoneNumber);
            }

            // Thêm vai trò User cho người dùng mới
            var userRole = await _roleRepository.GetByNameAsync("User");
            if (userRole != null)
            {
                user.AddRole(userRole);
            }
            
            // Lưu user vào cơ sở dữ liệu
            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();
            
            // Xử lý domain events
            foreach (var domainEvent in user.DomainEvents)
            {
                await _eventDispatcher.DispatchAsync(domainEvent);
            }
            user.ClearDomainEvents();
            
            // Trả về kết quả
            return new RegisterResponseDto
            {
                Success = true,
                User = new UserDto
                {
                    Id = user.Id,
                    Username = user.UserName,
                    Email = email.Value,
                    PhoneNumber = user.PhoneNumber,
                    EmailConfirmed = user.EmailConfirmed,
                    Roles = user.Roles.Select(r => r.Name)
                }
            };
        }
        catch (DomainException ex)
        {
            return new RegisterResponseDto
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
        catch (Exception)
        {
            return new RegisterResponseDto
            {
                Success = false,
                ErrorMessage = "An error occurred while registering the user"
            };
        }
    }
}
