using System;
using System.Collections.Generic;

namespace Ecommerce.Data.Models;

public partial class ProductDescription
{
    public int DescriptionId { get; set; }

    public int ProductId { get; set; }

    public string? ShortDescription { get; set; }

    public string? LongDescription { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Product Product { get; set; } = null!;
}
