using IdentityService.Application.DTOs;

namespace IdentityService.Application.Interfaces;

public interface IIdentityService
{
    // 1. Đăng ký tài khoản
    Task<RegisterResponse> RegisterAsync(RegisterRequest request);

    // 2. Đăng nhập
    Task<LoginResponse> LoginAsync(LoginRequest request);

    // 3. Đăng xuất
    Task<LogoutResponse> LogoutAsync(LogoutRequest request);

    // 4. Quản lý thông tin người dùng
    Task<UserProfileResponse> GetProfileAsync(string userId);
    Task<UserProfileResponse> UpdateProfileAsync(string userId, UpdateUserProfileRequest request);

    // 5. Đổi mật khẩu
    Task<ChangePasswordResponse> ChangePasswordAsync(string userId, ChangePasswordRequest request);

    // 6. Quên/Đặt lại mật khẩu
    Task<ForgotPasswordResponse> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<ForgotPasswordResponse> ResetPasswordAsync(ResetPasswordRequest request);

    // 7. Xác thực hai yếu tố
    Task<TwoFactorResponse> SetupTwoFactorAsync(TwoFactorSetupRequest request);
    Task<TwoFactorResponse> VerifyTwoFactorAsync(TwoFactorVerifyRequest request);

    // 8. Quản lý vai trò và phân quyền
    Task<RoleResponse> CreateRoleAsync(RoleRequest request);
    Task<RoleResponse> AssignRoleAsync(AssignRoleRequest request);

    // 9. Refresh Token
    Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenRequest request);

    // 10. Xác thực email/số điện thoại
    Task<VerificationResponse> VerifyEmailAsync(EmailVerificationRequest request);
    Task<VerificationResponse> VerifyPhoneAsync(PhoneVerificationRequest request);

    // 11. Quản lý phiên đăng nhập
    Task<SessionListResponse> GetSessionsAsync(string userId);
    Task<SessionInfoResponse> RevokeSessionAsync(string userId, string refreshToken);

    // 12. Đăng nhập ngoài
    Task<ExternalLoginResponse> ExternalLoginAsync(ExternalLoginRequest request);
}
