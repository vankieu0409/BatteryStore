using IdentityService.Domain.Common;

namespace IdentityService.Domain.Entities;

public class UserClaim
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string ClaimType { get; private set; } = string.Empty;
    public string ClaimValue { get; private set; } = string.Empty;

    // Constructor cho EF Core
    private UserClaim() { }

    public static UserClaim Create(User user, string claimType, string claimValue)
    {
        if (string.IsNullOrEmpty(claimType))
            throw new DomainException("Claim type cannot be empty");

        return new UserClaim
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            ClaimType = claimType,
            ClaimValue = claimValue ?? string.Empty
        };
    }
}
