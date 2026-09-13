using System;
using System.Collections.Generic;

namespace Ecommerce.Data.Models;

public partial class StockAdjustment
{
    public int AdjustmentId { get; set; }

    public int InventoryId { get; set; }

    public string AdjustmentType { get; set; } = null!;

    public int Quantity { get; set; }

    public string Reason { get; set; } = null!;

    public int? AdjustedBy { get; set; }

    public DateTime? AdjustedAt { get; set; }

    public string? Remarks { get; set; }

    public virtual Inventory Inventory { get; set; } = null!;
}
