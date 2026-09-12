namespace Ecommerce.API.DTOs
{
    public class OrderItemCreateDto
    {
        public int ProductId {  get; set; }
        public int Quantity { get; set; }
    }

    public class  OrderCreateDto
    {
        public int UserId { get; set; }
        public List<OrderItemCreateDto> Items { get; set; } = new();
        public string? CouponCode { get; set; } // Optional - agar customer ne coupon use kiya

    }

    public class OrderStatusUpdateDto
    {
        public string StatusName { get; set; }
    }
}
