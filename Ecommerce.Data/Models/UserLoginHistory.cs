using System;
using System.Collections.Generic;

namespace Ecommerce.Data.Models;

public partial class UserLoginHistory
{
    public int LoginHistoryId { get; set; }

    public int UserId { get; set; }

    public DateTime? LoginTime { get; set; }

    public DateTime? LogoutTime { get; set; }

    public string? Ipaddress { get; set; }

    public string? LoginStatus { get; set; }

    public virtual User User { get; set; } = null!;
}
