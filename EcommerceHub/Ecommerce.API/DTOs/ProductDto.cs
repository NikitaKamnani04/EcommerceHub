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
}
