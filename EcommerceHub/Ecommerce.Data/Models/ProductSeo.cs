using System;
using System.Collections.Generic;

namespace Ecommerce.Data.Models;

public partial class ProductSeo
{
    public int ProductId { get; set; }

    public string? MetaTitle { get; set; }

    public string? MetaDescription { get; set; }

    public string? MetaKeywords { get; set; }

    public virtual Product Product { get; set; } = null!;
}
