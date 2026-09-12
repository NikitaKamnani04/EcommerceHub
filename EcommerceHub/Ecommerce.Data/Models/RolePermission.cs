using System;
using System.Collections.Generic;

namespace Ecommerce.Data.Models;

public partial class RolePermission
{
    public int RolePermissionId { get; set; }

    public int RoleId { get; set; }

    public int PermissionId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Permission Permission { get; set; } = null!;

    public virtual UserRole Role { get; set; } = null!;
}
