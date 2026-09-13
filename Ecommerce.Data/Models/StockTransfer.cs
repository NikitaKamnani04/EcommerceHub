using System;
using System.Collections.Generic;

namespace Ecommerce.Data.Models;

public partial class StockTransfer
{
    public int TransferId { get; set; }

    public int ProductId { get; set; }

    public int FromWarehouseId { get; set; }

    public int ToWarehouseId { get; set; }

    public int Quantity { get; set; }

    public string? TransferStatus { get; set; }

    public int? RequestedBy { get; set; }

    public DateTime? RequestedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public string? Remarks { get; set; }

    public virtual Warehouse FromWarehouse { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;

    public virtual Warehouse ToWarehouse { get; set; } = null!;
}
