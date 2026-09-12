namespace Ecommerce.Web.Models
{
    public class CartPageModel
    {
        public int CartID { get; set; }
        public List<CartPageItemModel> CartItems { get; set; } = new();
    }

    public class CartPageItemModel
    {
        public int CartItemID { get; set; }
        public int ProductID { get; set; }
        public int Quantity { get; set; }
        public CartPageProductModel? Product { get; set; }
    }

    public class CartPageProductModel
    {
        public string ProductName { get; set; } = "";
        public decimal Price { get; set; }
    }

    public class OrderResultModel
    {
        public int OrderID { get; set; }
        public decimal TotalAmount { get; set; }
    }
}