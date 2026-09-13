using System;
using System.Collections.Generic;

namespace Ecommerce.Data.Models;

public partial class InventoryTransaction
{
    public int TransactionId { get; set; }

    public int InventoryId { get; set; }

    public string TransactionType { get; set; } = null!;

    public int Quantity { get; set; }

    public string? ReferenceType { get; set; }

    public int? ReferenceId { get; set; }

    public string? Remarks { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Inventory Inventory { get; set; } = null!;
}
