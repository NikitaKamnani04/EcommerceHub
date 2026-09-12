using System;
using System.Collections.Generic;

namespace Ecommerce.Data.Models;

public partial class UserActivityLog
{
    public int ActivityLogId { get; set; }

    public int UserId { get; set; }

    public string ActivityType { get; set; } = null!;

    public string? ActivityDescription { get; set; }

    public string? Ipaddress { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
