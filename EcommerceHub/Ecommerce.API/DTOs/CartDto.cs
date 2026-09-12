namespace Ecommerce.API.DTOs
{
    public class AddToCartDto
    {
        public int UserID { get; set; }
        public int ProductID { get; set; }
        public int Quantity { get; set; } = 1;
    }

    public class UpdateCartItemDto
    {
        public int Quantity { get; set; }
    }
}
