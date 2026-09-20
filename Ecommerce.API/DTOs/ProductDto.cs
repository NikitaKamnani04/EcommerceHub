namespace Ecommerce.API.DTOs
{
    public class ProductCreateDto
    {
        public int CategoryID { get; set; }
        public int SubCategoryID { get;set; }
        public int BrandID { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }

    }

    public class ProductUpdateDto
    {
        public int? CategoryID { get; set; }
        public int? SubCategoryID { get; set; }
        public int? BrandID { get; set; }
        public string? ProductName { get; set; }
        public decimal? Price { get; set; }
        public int? StockQuantity { get; set; }
    }

    public class RecommendedProductDto
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; } = "";
        public decimal Price { get; set; }
        public int TimesBoughtTogether { get; set; }
    }

    // Generic wrapper - kisi bhi type "T" ki paginated list ke liye use ho sakta hai
    public class PagedResultDto<T>
    {
        public List<T> Items { get; set; } = new();   // Is page ke actual items
        public int TotalCount { get; set; }             // Total kitne items hain (saare pages milake)
        public int Page { get; set; }                    // Abhi kaunsa page hai
        public int PageSize { get; set; }                 // Ek page mein kitne items
        public int TotalPages { get; set; }               // Total kitne pages hain
    }
}
