namespace Ecommerce.Web.Models
{
    public class ProductViewModel
    {
        public int ProductId { get; set; }        // Product ki unique ID
        public string ProductName { get; set; } = "";   // Product ka naam
        public decimal Price { get; set; }          // Product ki price
        public int StockQuantity { get; set; }
    }
    public class RecommendedProductViewModel
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; } = "";
        public decimal Price { get; set; }
    }

    public class PagedProductsViewModel
    {
        public List<ProductViewModel> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public string? SearchTerm { get; set; }   // Taaki search box mein pehle se type kiya hua text dikhe
    }   
}
