namespace Ecommerce.Web.Models
{
    // Order list page ke liye - Admin ko saare orders dikhane ke liye
    public class OrderListItemModel
    {
        public int OrderID { get; set; }
        public int UserID { get; set; }
        public DateTime? OrderDate { get; set; }
        public int? OrderStatusID { get; set; }
        public int? PaymentStatusID { get; set; }
        public decimal? TotalAmount { get; set; }
    }

    // Order status dropdown ke liye
    public class OrderStatusModel
    {
        public int OrderStatusID { get; set; }
        public string StatusName { get; set; } = "";
    }

    // Order Details page ke liye - poori info chahiye
    public class OrderDetailModel
    {
        public int OrderID { get; set; }
        public int UserID { get; set; }
        public DateTime? OrderDate { get; set; }
        public int? OrderStatusID { get; set; }
        public decimal? TotalAmount { get; set; }
        public List<OrderItemDetailModel> OrderItems { get; set; } = new();
    }

    public class OrderItemDetailModel
    {
        public int ProductID { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
