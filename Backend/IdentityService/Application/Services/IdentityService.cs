//using IdentityService.Application.DTOs;
//using IdentityService.Application.Interfaces;
//using IdentityService.Application.Common.Helpers;
//using IdentityService.Domain.Entities;
//using Microsoft.Extensions.Configuration;
//using IdentityService.Infrastructure;
//using Microsoft.EntityFrameworkCore;

//namespace IdentityService.Application.Services;

//public class IdentityService : IIdentityService
//{
//    private readonly IdentityDbContext _db;
//    private readonly JwtHelper _jwtHelper;

//    public IdentityService(IdentityDbContext db, IConfiguration configuration)
//    {
//        _db = db;
//        _jwtHelper = new JwtHelper(configuration);
//    }

//    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
//    {
//        // Ki?m tra tr�ng email/username
//        if (await _db.Users.AnyAsync(u => u.Email == request.Email || u.UserName == request.UserName))
//        {
//            return new RegisterResponse { Success = false, Message = "Email ho?c username ?� t?n t?i." };
//        }        // Hash password
//        var passwordHash = Common.Helpers.PasswordHelper.HashPassword(request.Password);
//        var user = new User
//        {
//            Id = Guid.NewGuid(),
//            UserName = request.UserName,
//            Email = request.Email,
//            PhoneNumber = request.PhoneNumber,
//            PasswordHash = passwordHash,
//            EmailConfirmed = false,
//            PhoneNumberConfirmed = false,
//            TwoFactorEnabled = false,
//            LockoutEnabled = false,
//            AccessFailedCount = 0
//        };
//        _db.Users.Add(user);
//        await _db.SaveChangesAsync();
//        return new RegisterResponse { Success = true, Message = "??ng k� th�nh c�ng." };
//    }

//    public async Task<LoginResponse> LoginAsync(LoginRequest request)
//    {
//        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == request.UserNameOrEmail || u.Email == request.UserNameOrEmail);
//        if (user == null)
//        {
//            return new LoginResponse { Success = false, Message = "T�i kho?n kh�ng t?n t?i." };
//        }
//        if (!Common.Helpers.PasswordHelper.VerifyPassword(request.Password, user.PasswordHash))
//        {
//            return new LoginResponse { Success = false, Message = "M?t kh?u kh�ng ?�ng." };
//        }
        
//        // Sinh AccessToken (JWT) s? d?ng JwtHelper
//        var accessToken = _jwtHelper.GenerateAccessToken(user);

//        // L?y th�ng tin thi?t b? t? request
//        string deviceInfo = request.DeviceInfo ?? "Unknown Device";

//        // Sinh RefreshToken
//        var refreshTokenValue = Guid.NewGuid().ToString();
//        var refreshToken = new Infrastructure.Models.RefreshToken
//        {
//            Id = Guid.NewGuid(),
//            UserId = user.Id,
//            Token = refreshTokenValue,
//            Expires = DateTime.UtcNow.AddDays(_jwtHelper.GetRefreshTokenExpiryDays()),
//            Created = DateTime.UtcNow,
//            CreatedByIp = "system", // C� th? l?y t? request
//            IsRevoked = false,
//            DeviceInfo = deviceInfo
//        };
//        _db.RefreshTokens.Add(refreshToken);
//        await _db.SaveChangesAsync();

//        return new LoginResponse
//        {
//            Success = true,
//            Message = "??ng nh?p th�nh c�ng.",
//            AccessToken = accessToken,
//            RefreshToken = refreshTokenValue
//        };
//    }

//    public async Task<LogoutResponse> LogoutAsync(LogoutRequest request)
//    {
//        // T�m refresh token trong database
//        var refreshToken = await _db.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);
//        if (refreshToken == null)
//        {
//            return new LogoutResponse { Success = false, Message = "Refresh token kh�ng h?p l? ho?c ?� b? thu h?i." };
//        }
//        if (refreshToken.IsRevoked)
//        {
//            return new LogoutResponse { Success = false, Message = "Refresh token ?� b? thu h?i." };
//        }
//        refreshToken.IsRevoked = true;
//        refreshToken.RevokedAt = DateTime.UtcNow;
//        await _db.SaveChangesAsync();
//        return new LogoutResponse { Success = true, Message = "??ng xu?t th�nh c�ng." };
//    }

//    public Task<UserProfileResponse> GetProfileAsync(string userId) => Task.FromResult(new UserProfileResponse());
//    public Task<UserProfileResponse> UpdateProfileAsync(string userId, UpdateUserProfileRequest request) => Task.FromResult(new UserProfileResponse());
//    public Task<ChangePasswordResponse> ChangePasswordAsync(string userId, ChangePasswordRequest request) => Task.FromResult(new ChangePasswordResponse());
//    public Task<ForgotPasswordResponse> ForgotPasswordAsync(ForgotPasswordRequest request) => Task.FromResult(new ForgotPasswordResponse());
//    public Task<ForgotPasswordResponse> ResetPasswordAsync(ResetPasswordRequest request) => Task.FromResult(new ForgotPasswordResponse());
//    public Task<TwoFactorResponse> SetupTwoFactorAsync(TwoFactorSetupRequest request) => Task.FromResult(new TwoFactorResponse());
//    public Task<TwoFactorResponse> VerifyTwoFactorAsync(TwoFactorVerifyRequest request) => Task.FromResult(new TwoFactorResponse());
//    public Task<RoleResponse> CreateRoleAsync(RoleRequest request) => Task.FromResult(new RoleResponse());
//    public Task<RoleResponse> AssignRoleAsync(AssignRoleRequest request) => Task.FromResult(new RoleResponse());
//    public async Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenRequest request)
//    {
//        // T�m RefreshToken trong database
//        var refreshToken = await _db.RefreshTokens
//            .Include(rt => rt.User)
//            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);
        
//        // Ki?m tra refreshToken c� t?n t?i kh�ng
//        if (refreshToken == null)
//        {
//            return new RefreshTokenResponse { Success = false, Message = "Refresh token kh�ng h?p l?." };
//        }
        
//        // Ki?m tra token c� b? thu h?i kh�ng
//        if (refreshToken.IsRevoked)
//        {
//            return new RefreshTokenResponse { Success = false, Message = "Refresh token ?� b? thu h?i." };
//        }
        
//        // Ki?m tra token c� h?t h?n kh�ng
//        if (refreshToken.Expires < DateTime.UtcNow)
//        {
//            refreshToken.IsRevoked = true;
//            refreshToken.RevokedAt = DateTime.UtcNow;
//            await _db.SaveChangesAsync();
//            return new RefreshTokenResponse { Success = false, Message = "Refresh token ?� h?t h?n." };
//        }
        
//        var user = refreshToken.User;
//        if (user == null)
//        {
//            return new RefreshTokenResponse { Success = false, Message = "Kh�ng t�m th?y th�ng tin ng??i d�ng." };
//        }
        
//        // Sinh AccessToken m?i s? d?ng JwtHelper
//        var accessToken = _jwtHelper.GenerateAccessToken(user);
        
//        // T?o RefreshToken m?i
//        var newRefreshTokenValue = Guid.NewGuid().ToString();
//        var newRefreshToken = new Infrastructure.Models.RefreshToken
//        {
//            Id = Guid.NewGuid(),
//            UserId = user.Id,
//            Token = newRefreshTokenValue,
//            Expires = DateTime.UtcNow.AddDays(_jwtHelper.GetRefreshTokenExpiryDays()),
//            Created = DateTime.UtcNow,
//            CreatedByIp = refreshToken.CreatedByIp,
//            IsRevoked = false,
//            DeviceInfo = refreshToken.DeviceInfo
//        };
        
//        // Thu h?i RefreshToken c?
//        refreshToken.IsRevoked = true;
//        refreshToken.RevokedAt = DateTime.UtcNow;
        
//        // L?u RefreshToken m?i
//        _db.RefreshTokens.Add(newRefreshToken);
//        await _db.SaveChangesAsync();
        
//        return new RefreshTokenResponse
//        {
//            Success = true,
//            Message = "Refresh token th�nh c�ng.",
//            AccessToken = accessToken,
//            RefreshToken = newRefreshTokenValue
//        };
//    }
//    public Task<VerificationResponse> VerifyEmailAsync(EmailVerificationRequest request) => Task.FromResult(new VerificationResponse());
//    public Task<VerificationResponse> VerifyPhoneAsync(PhoneVerificationRequest request) => Task.FromResult(new VerificationResponse());
//    public Task<SessionListResponse> GetSessionsAsync(string userId) => Task.FromResult(new SessionListResponse());
//    public Task<SessionInfoResponse> RevokeSessionAsync(string userId, string refreshToken) => Task.FromResult(new SessionInfoResponse());
//    public Task<ExternalLoginResponse> ExternalLoginAsync(ExternalLoginRequest request) => Task.FromResult(new ExternalLoginResponse());
//}
