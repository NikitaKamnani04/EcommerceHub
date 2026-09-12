using System;
using System.Collections.Generic;

namespace Ecommerce.Data.Models;

public partial class RecentlyViewedProduct
{
    public int RecentlyViewedId { get; set; }

    public int UserId { get; set; }

    public int ProductId { get; set; }

    public DateTime? ViewedAt { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
