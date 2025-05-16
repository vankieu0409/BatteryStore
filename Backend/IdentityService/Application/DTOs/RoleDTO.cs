namespace IdentityService.Application.DTOs;

public class RoleRequest
{
    public string Name { get; set; } = string.Empty;
}

public class RoleResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
}

public class AssignRoleRequest
{
    public string UserId { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
}
