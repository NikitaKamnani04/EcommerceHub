namespace Ecommerce.Web.Models
{
    public class ProductViewModel
    {
        public int ProductId { get; set; }        // Product ki unique ID
        public string ProductName { get; set; } = "";   // Product ka naam
        public decimal Price { get; set; }          // Product ki price
        public int StockQuantity { get; set; }
    }
}
