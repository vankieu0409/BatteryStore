using IdentityService.Domain.Common;
using IdentityService.Domain.Events;
using IdentityService.Domain.ValueObjects;

namespace IdentityService.Domain.Entities;

public class User : IAggregateRoot
{
    public Guid Id { get; private set; }
    public string UserName { get; private set; } = string.Empty;
    public Email Email { get; private set; }
    public string PasswordHash { get; private set; } = string.Empty;
    public string? PhoneNumber { get; private set; }
    public bool EmailConfirmed { get; private set; }
    public bool PhoneNumberConfirmed { get; private set; }
    public bool TwoFactorEnabled { get; private set; }
    public DateTime? LockoutEnd { get; private set; }
    public bool LockoutEnabled { get; private set; }
    public int AccessFailedCount { get; private set; }

    private readonly List<Role> _roles = new();
    public IReadOnlyCollection<Role> Roles => _roles.AsReadOnly();

    private readonly List<RefreshToken> _refreshTokens = new();
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    // Các collection khác
    private readonly List<UserClaim> _claims = new();
    public IReadOnlyCollection<UserClaim> Claims => _claims.AsReadOnly();

    private readonly List<UserLogin> _logins = new();
    public IReadOnlyCollection<UserLogin> Logins => _logins.AsReadOnly();

    private readonly List<UserToken> _tokens = new();
    public IReadOnlyCollection<UserToken> Tokens => _tokens.AsReadOnly();

    // Constructor cho EF Core
    private User() { }

    // Factory method - cách chính để tạo entity này
    public static User Create(string username, Email email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new DomainException("Username cannot be empty");

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("Password hash cannot be empty");

        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = username,
            Email = email,
            PasswordHash = passwordHash,
            EmailConfirmed = false,
            LockoutEnabled = true
        };

        // Phát sinh domain event
        user.AddDomainEvent(new UserCreatedEvent(user.Id, user.UserName, email.Value));

        return user;
    }

    // Behavior methods - Các phương thức thực thi logic nghiệp vụ
    public void ConfirmEmail()
    {
        if (EmailConfirmed)
            throw new DomainException("Email already confirmed");

        EmailConfirmed = true;
    }

    public void AddRole(Role role)
    {
        if (_roles.Any(r => r.Id == role.Id))
            return;

        _roles.Add(role);
    }

    public RefreshToken AddRefreshToken(string token, string deviceInfo, DateTime expiry)
    {
        var refreshToken = RefreshToken.Create(this, token, deviceInfo, expiry);
        _refreshTokens.Add(refreshToken);
        return refreshToken;
    }

    public void RevokeRefreshToken(string token)
    {
        var refreshToken = _refreshTokens.FirstOrDefault(rt => rt.Token == token);
        if (refreshToken != null)
        {
            refreshToken.Revoke();
        }
    }

    // Domain events implementation
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
