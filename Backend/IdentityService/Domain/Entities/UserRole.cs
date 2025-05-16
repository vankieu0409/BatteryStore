namespace IdentityService.Domain.Entities;

public class UserRole
{
    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }

    // Constructor cho EF Core
    private UserRole() { }

    public static UserRole Create(User user, Role role)
    {
        return new UserRole
        {
            UserId = user.Id,
            RoleId = role.Id
        };
    }
}
