using System;
using System.Collections.Generic;

namespace Ecommerce.Data.Models;

public partial class Return
{
    public int ReturnId { get; set; }

    public int OrderItemId { get; set; }

    public int UserId { get; set; }

    public string? ReturnReason { get; set; }

    public string? ReturnStatus { get; set; }

    public decimal? RefundAmount { get; set; }

    public DateTime? RequestedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public virtual OrderItem OrderItem { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
