using BatteryShop.Logging;
using IdentityService.Application.Commands;
using IdentityService.Application.DTOs;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Interfaces;
using IdentityService.Domain.Services;
using IdentityService.Domain.ValueObjects;
using IdentityService.Application.Interfaces;
using IdentityService.Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IdentityService.Application.Handlers;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, RegisterResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IDomainEventDispatcher _eventDispatcher;
    private readonly ILogger<RegisterUserCommandHandler> _logger;

    public RegisterUserCommandHandler(
        IUserRepository userRepository, 
        IRoleRepository roleRepository, 
        IPasswordHasher passwordHasher,
        IDomainEventDispatcher eventDispatcher,
        ILogger<RegisterUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
        _eventDispatcher = eventDispatcher;
        _logger = logger;
    }    public async Task<RegisterResponseDto> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // Sử dụng WithContext để thêm thông tin vào log context
        return await LogHelper.WithContextAsync<RegisterResponseDto>("RegistrationInfo", 
            new { Username = request.Username, Email = request.Email }, 
            async () =>
        {
            try
            {
                _logger.LogInformation("Đang xử lý đăng ký người dùng");

            // Kiểm tra xem username hoặc email đã tồn tại chưa
            if (await _userRepository.ExistsAsync(request.Username, request.Email))
            {
                _logger.LogWarning("Tên đăng nhập hoặc email đã tồn tại: {Username}, {Email}", 
                    request.Username, request.Email);
                
                return new RegisterResponseDto
                {
                    Success = false,
                    ErrorMessage = "Tên đăng nhập hoặc email đã tồn tại"
                };
            }
            
            // Tạo email value object
            var email = Email.Create(request.Email);
            
            // Hash mật khẩu
            var passwordHash = _passwordHasher.HashPassword(request.Password);
            
            // Tạo user mới (từ factory method trong domain)
            var user = User.Create(request.Username, email, passwordHash);
            
            // Thiết lập thông tin bổ sung
            if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                // Giả sử có setter trong domain object hoặc method để thiết lập phone number
                // user.SetPhoneNumber(request.PhoneNumber);
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

            _logger.LogInformation("Đăng ký người dùng thành công: {UserId}", user.Id);
            
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
            _logger.LogError(ex, "Lỗi domain khi đăng ký người dùng: {ErrorMessage}", ex.Message);
            
            return new RegisterResponseDto
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
        catch (Exception ex)
        {            _logger.LogError(ex, "Lỗi không xác định khi đăng ký người dùng: {ErrorMessage}", ex.Message);
            
            return new RegisterResponseDto
            {
                Success = false,
                ErrorMessage = "Đã xảy ra lỗi khi đăng ký người dùng"
            };
        }
        });
    }
}
