using System;
using System.Collections.Generic;

namespace Ecommerce.Data.Models;

public partial class PaymentStatusMaster
{
    public int PaymentStatusId { get; set; }

    public string? StatusName { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
