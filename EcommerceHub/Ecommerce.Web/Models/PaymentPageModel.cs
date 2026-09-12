
namespace Ecommerce.Web.Models
{
    public class PaymentPageModel
    {
        public int OrderID { get; set; }
        public decimal Amount { get; set; }
        public string UpiQrCodeBase64 { get; set; } = "";   // QR code image (base64 string mein)
        public string UpiLink { get; set; } = ""; // Direct UPI link bhi (mobile pe click karke bhi khul sake)
    }
}
    

