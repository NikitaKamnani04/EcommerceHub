using System;
using System.Collections.Generic;

namespace Ecommerce.Data.Models;

public partial class ProductPricing
{
    public int PricingId { get; set; }

    public int ProductId { get; set; }

    public decimal BasePrice { get; set; }

    public decimal? DiscountPercent { get; set; }

    public decimal? FinalPrice { get; set; }

    public DateTime? EffectiveFrom { get; set; }

    public DateTime? EffectiveTo { get; set; }

    public virtual Product Product { get; set; } = null!;
}
