# Tài liệu triển khai IdentityService theo DDD & Clean Architecture

## Tổng quan kiến trúc
Identity Service được tái cấu trúc theo nguyên tắc Domain-Driven Design (DDD) và Clean Architecture:

1. **Domain Layer**: Chứa domain models, business rules, interfaces và domain events
2. **Application Layer**: Chứa use cases, DTO, interfaces service và triển khai nghiệp vụ 
3. **Infrastructure Layer**: Chứa triển khai cơ sở dữ liệu, các dịch vụ bên ngoài và triển khai repository
4. **API Layer**: Chứa controllers, middleware và các extension methods

## Domain Layer

### Entities
- `User`: Aggregate root đại diện cho người dùng trong hệ thống
- `Role`: Entity đại diện cho vai trò người dùng
- `RefreshToken`: Entity đại diện cho token làm mới
- Các entity khác: `UserClaim`, `UserLogin`, `UserToken`, `UserRole`

### Value Objects
- `Email`: Value object đại diện cho email với validation logic

### Domain Events
- `UserCreatedEvent`: Event phát sinh khi user mới được tạo

### Domain Exceptions
- `DomainException`: Ngoại lệ dùng cho các lỗi domain

### Repository Interfaces
- `IUserRepository`, `IRoleRepository`: Interfaces định nghĩa các thao tác với dữ liệu

### Domain Services
- `IPasswordHasher`: Interface định nghĩa việc hash và verify mật khẩu

## Application Layer

### DTOs (Data Transfer Objects)
- `RegisterUserDto`: DTO đầu vào cho việc đăng ký
- `UserDto`: DTO đầu ra cho thông tin user
- `RegisterResponseDto`: DTO đầu ra cho kết quả đăng ký

### Application Services
- `UserService`: Xử lý nghiệp vụ đăng ký, quản lý người dùng  

## Infrastructure Layer

### DbContext
- `IdentityDbContext`: Cấu hình Entity Framework Core

### Entity Configurations
- `UserConfiguration`: Cấu hình cho entity User
- `RoleConfiguration`: Cấu hình cho entity Role
- `RefreshTokenConfiguration`: Cấu hình cho entity RefreshToken

### Repositories
- `UserRepository`: Triển khai IUserRepository
- `RoleRepository`: Triển khai IRoleRepository

### Services
- `PasswordHasher`: Triển khai IPasswordHasher
- `DomainEventDispatcher`: Triển khai IDomainEventDispatcher

## API Layer

### Controllers
- `AccountController`: Xử lý endpoints liên quan đến đăng ký, đăng nhập

### Middleware
- `DeviceInfoMiddleware`: Middleware lấy thông tin thiết bị

### Extensions
- `ServiceExtensions`: Định nghĩa các extension methods để đăng ký services

## Flow đăng ký người dùng

1. Request đến `AccountController.Register`
2. Controller gọi `UserService.RegisterUserAsync`
3. Application service tạo domain entities thông qua factory methods:
   - `Email.Create` để tạo value object email
   - `User.Create` để tạo entity user
4. Các domain entities thực hiện validation và business rules
5. Khi tạo user, domain event `UserCreatedEvent` được phát sinh
6. `UserRepository` lưu user vào database
7. `DomainEventDispatcher` xử lý domain events (ví dụ: gửi email chào mừng)
8. Kết quả được trả về client dưới dạng `RegisterResponseDto`

## Lưu ý quan trọng

1. Domain entities chỉ có thể được tạo thông qua factory methods để đảm bảo invariants
2. Business logic nằm trong domain entities, không nằm trong application services 
3. Application services điều phối các entities, repositories và services
4. Domain layer không phụ thuộc vào bất kỳ layer nào khác
5. Domain events được dùng để xử lý side effects

## Tính năng nâng cao đã triển khai

### 1. Validation với FluentValidation

FluentValidation được sử dụng để xác thực dữ liệu đầu vào theo một cách khai báo, rõ ràng và dễ mở rộng:

- **Validators**: Định nghĩa các quy tắc xác thực cho Commands và DTOs
  - `RegisterUserCommandValidator`: Xác thực command đăng ký người dùng
  - `RegisterUserDtoValidator`: Xác thực DTO đăng ký người dùng
  
- **Validation Pipeline**: Xác thực tự động thông qua MediatR pipeline
  - `ValidationBehavior`: Middleware xử lý xác thực trước khi command được xử lý

### 2. CQRS Pattern với MediatR

Command Query Responsibility Segregation (CQRS) giúp tách biệt các thao tác đọc và ghi, được triển khai với MediatR:

- **Commands**: Đại diện cho các yêu cầu thay đổi dữ liệu
  - `RegisterUserCommand`: Yêu cầu đăng ký người dùng mới

- **Command Handlers**: Xử lý các commands
  - `RegisterUserCommandHandler`: Xử lý logic đăng ký người dùng

- **Queries**: (Sẽ được triển khai sau) - Đại diện cho các yêu cầu đọc dữ liệu
- **Query Handlers**: (Sẽ được triển khai sau) - Xử lý các queries

### 3. Logging và Exception Handling tập trung

#### Logging với NLog

- **Cấu hình NLog**: File `nlog.config` định nghĩa các target và rules ghi log
- **Log Targets**: 
  - File logs: Ghi tất cả logs vào files theo ngày
  - Console logs: Ghi logs về hosting lifetime ra console

#### Exception Handling

- **Middleware**: 
  - `ErrorHandlingMiddleware`: Middleware bắt và xử lý tất cả exception, trả về response chuẩn
  
- **Exception Filter**: 
  - `ApiExceptionFilterAttribute`: Filter để xử lý exception ở tầng controller
  
- **Phân loại Exception**:
  - `ValidationException`: Lỗi xác thực dữ liệu đầu vào
  - `DomainException`: Lỗi business logic
  - `NotFoundException`: Không tìm thấy tài nguyên
  - `UnauthorizedAccessException`: Không có quyền truy cập
  - `ForbiddenAccessException`: Không được phép thực hiện thao tác

## Flow đăng ký người dùng (mới)

1. Request HTTP đến `AccountController.Register`
2. Controller chuyển request thành `RegisterUserCommand` và gửi đến `Mediator`
3. `ValidationBehavior` xác thực command với `RegisterUserCommandValidator`
4. Nếu hợp lệ, `RegisterUserCommandHandler` xử lý command:
   - Kiểm tra tồn tại username/email
   - Tạo value objects (`Email`)
   - Tạo domain entities thông qua factory methods (`User.Create`)
   - Lưu vào database thông qua repository
   - Phát sinh và xử lý domain events
5. Kết quả được trả về cho controller dưới dạng `RegisterResponseDto`
6. Controller trả về response HTTP với dữ liệu phù hợp
7. Bất kỳ exception nào được phát sinh đều được xử lý bởi `ErrorHandlingMiddleware` hoặc `ApiExceptionFilterAttribute`

## Lưu ý về kiến trúc

- **Single Responsibility**: Mỗi thành phần chỉ có một trách nhiệm duy nhất
- **Separation of Concerns**: Tách biệt rõ ràng giữa validation, business logic và error handling
- **CQRS**: Tách biệt thao tác đọc và ghi để tối ưu hiệu suất và mở rộng
- **Dependency Inversion**: Tất cả các thành phần đều phụ thuộc vào abstractions, không phải implementations

    // 2. ??ng nh?p
    Task<LoginResponse> LoginAsync(LoginRequest request);

    // 3. ??ng xu?t
    Task<LogoutResponse> LogoutAsync(LogoutRequest request);

    // 4. Qu?n l� th�ng tin ng??i d�ng
    Task<UserProfileResponse> GetProfileAsync(string userId);
    Task<UserProfileResponse> UpdateProfileAsync(string userId, UpdateUserProfileRequest request);

    // 5. ??i m?t kh?u
    Task<ChangePasswordResponse> ChangePasswordAsync(string userId, ChangePasswordRequest request);

    // 6. Qu�n/??t l?i m?t kh?u
    Task<ForgotPasswordResponse> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<ForgotPasswordResponse> ResetPasswordAsync(ResetPasswordRequest request);

    // 7. X�c th?c hai y?u t?
    Task<TwoFactorResponse> SetupTwoFactorAsync(TwoFactorSetupRequest request);
    Task<TwoFactorResponse> VerifyTwoFactorAsync(TwoFactorVerifyRequest request);

    // 8. Qu?n l� vai tr� v� ph�n quy?n
    Task<RoleResponse> CreateRoleAsync(RoleRequest request);
    Task<RoleResponse> AssignRoleAsync(AssignRoleRequest request);

    // 9. Refresh Token
    Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenRequest request);

    // 10. X�c th?c email/s? ?i?n tho?i
    Task<VerificationResponse> VerifyEmailAsync(EmailVerificationRequest request);
    Task<VerificationResponse> VerifyPhoneAsync(PhoneVerificationRequest request);

    // 11. Qu?n l� phi�n ??ng nh?p
    Task<SessionListResponse> GetSessionsAsync(string userId);
    Task<SessionInfoResponse> RevokeSessionAsync(string userId, string refreshToken);

    // 12. ??ng nh?p ngo�i
    Task<ExternalLoginResponse> ExternalLoginAsync(ExternalLoginRequest request);