using IdentityService.Domain.Common;

namespace IdentityService.Domain.Entities;

public class UserToken
{
    public Guid UserId { get; private set; }
    public string LoginProvider { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;

    // Constructor cho EF Core
    private UserToken() { }

    public static UserToken Create(User user, string loginProvider, string name, string value)
    {
        if (string.IsNullOrEmpty(loginProvider))
            throw new DomainException("Login provider cannot be empty");

        if (string.IsNullOrEmpty(name))
            throw new DomainException("Name cannot be empty");

        return new UserToken
        {
            UserId = user.Id,
            LoginProvider = loginProvider,
            Name = name,
            Value = value ?? string.Empty
        };
    }
}
