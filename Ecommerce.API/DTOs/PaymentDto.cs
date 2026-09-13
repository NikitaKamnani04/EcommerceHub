namespace Ecommerce.API.DTOs
{
    public class PaymentCreateDto
    {
        public int OrderID { get; set; }
        public string PaymentMethod { get; set; }      // "UPI", "CreditCard", "COD" etc.
        public string? TransactionID { get; set; }       // Payment gateway se mila transaction id
        public decimal PaidAmount { get; set; }
        public string? PaymentGateway { get; set; }       // "Razorpay", "Stripe" etc.
        public string PaymentStatus { get; set; }         // "Success", "Failed"
    }
}
