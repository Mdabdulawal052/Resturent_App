using Restaurant.Domain.Common;

namespace Restaurant.Domain.Entities;

public class User : AuditableEntity
{
    public long RestaurantId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public string? PinHash { get; set; } // 4 or 6 digit PIN hash for rapid cashier / waiter switching on POS
    public bool IsActive { get; set; } = true;

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}

public class Role : BaseEntity
{
    public long RestaurantId { get; set; }
    public string Name { get; set; } = string.Empty; // Owner, BranchManager, Cashier, Waiter, Kitchen, Auditor
    public bool IsSystemRole { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

public class Permission : BaseEntity
{
    public string Code { get; set; } = string.Empty; // e.g. POS.ORDER.CREATE, POS.DISCOUNT.OVERRIDE
    public string Name { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
}

public class UserRole : BaseEntity
{
    public long UserId { get; set; }
    public User? User { get; set; }
    public long RoleId { get; set; }
    public Role? Role { get; set; }
    public long? BranchId { get; set; }
}

public class RolePermission : BaseEntity
{
    public long RoleId { get; set; }
    public Role? Role { get; set; }
    public long PermissionId { get; set; }
    public Permission? Permission { get; set; }
}
