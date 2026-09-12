using System;
using System.Collections.Generic;

namespace Ecommerce.Data.Models;

public partial class ProductDiscount
{
    public int DiscountId { get; set; }

    public int ProductId { get; set; }

    public string DiscountType { get; set; } = null!;

    public decimal DiscountValue { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Product Product { get; set; } = null!;
}
