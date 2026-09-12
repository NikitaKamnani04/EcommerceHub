using System;
using System.Collections.Generic;

namespace Ecommerce.Data.Models;

public partial class ProductStatusHistory
{
    public int StatusHistoryId { get; set; }

    public int ProductId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? ChangedAt { get; set; }

    public int? ChangedBy { get; set; }

    public string? Remarks { get; set; }

    public virtual Product Product { get; set; } = null!;
}
