namespace Ecommerce.API.DTOs
{
    public class CouponCreateDto
    {
        public string CouponCode { get; set; } = "";
        public string DiscountType { get; set; } = "";   // "Percentage" ya "Flat"
        public decimal DiscountValue { get; set; }
        public decimal MinOrderAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    // Checkout ke waqt coupon apply karne ke liye
    public class ApplyCouponDto
    {
        public string CouponCode { get; set; } = "";
        public decimal OrderAmount { get; set; }   // Cart ka current total - validate karne ke liye
    }
}
