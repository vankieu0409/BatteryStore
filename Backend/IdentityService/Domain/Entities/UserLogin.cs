using IdentityService.Domain.Common;

namespace IdentityService.Domain.Entities;

public class UserLogin
{
    public string LoginProvider { get; private set; } = string.Empty;
    public string ProviderKey { get; private set; } = string.Empty;
    public string ProviderDisplayName { get; private set; } = string.Empty;
    public Guid UserId { get; private set; }

    // Constructor cho EF Core
    private UserLogin() { }

    public static UserLogin Create(User user, string loginProvider, string providerKey, string providerDisplayName)
    {
        if (string.IsNullOrEmpty(loginProvider))
            throw new DomainException("Login provider cannot be empty");

        if (string.IsNullOrEmpty(providerKey))
            throw new DomainException("Provider key cannot be empty");

        return new UserLogin
        {
            UserId = user.Id,
            LoginProvider = loginProvider,
            ProviderKey = providerKey,
            ProviderDisplayName = providerDisplayName ?? string.Empty
        };
    }
}
