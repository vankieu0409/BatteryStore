using IdentityService.Domain.Common;

namespace IdentityService.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public string DeviceInfo { get; private set; } = string.Empty;
    public DateTime ExpiryDate { get; private set; }
    public DateTime CreatedDate { get; private set; }
    public DateTime? RevokedDate { get; private set; }
    public bool IsExpired => DateTime.UtcNow >= ExpiryDate;
    public bool IsActive => RevokedDate == null && !IsExpired;
    public Guid UserId { get; private set; }

    // Constructor cho EF Core
    private RefreshToken() { }

    public static RefreshToken Create(User user, string token, string deviceInfo, DateTime expiryDate)
    {
        if (string.IsNullOrEmpty(token))
            throw new DomainException("Token cannot be empty");

        if (expiryDate <= DateTime.UtcNow)
            throw new DomainException("Expiry date must be in the future");

        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = token,
            DeviceInfo = deviceInfo,
            ExpiryDate = expiryDate,
            CreatedDate = DateTime.UtcNow,
            UserId = user.Id
        };
    }

    public void Revoke()
    {
        if (RevokedDate != null)
            throw new DomainException("Token already revoked");

        RevokedDate = DateTime.UtcNow;
    }
}
