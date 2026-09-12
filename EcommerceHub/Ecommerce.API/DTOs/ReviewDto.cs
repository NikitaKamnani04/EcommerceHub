namespace Ecommerce.API.DTOs
{
    public class ReviewCreateDto
    {
        public int ProductID { get; set; }
        public int UserID { get; set; }
        public int Rating { get; set; }        // 1 to 5
        public string? ReviewText { get; set; }
    }
}
