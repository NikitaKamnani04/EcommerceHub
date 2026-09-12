namespace Ecommerce.Web.Models
{
    public class ShipmentModel
    {
        public int ShipmentID { get; set; }
        public int OrderID { get; set; }
        public string TrackingNumber { get; set; } = "";
        public string CourierName { get; set; } = "";
        public string ShipmentStatus { get; set; } = "";
        public DateTime? ShippedDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public DateTime? EstimatedDelivery { get; set; }
    }

    public class ShipmentCreateFormModel
    {
        public int OrderID { get; set; }
        public string TrackingNumber { get; set; } = "";
        public string CourierName { get; set; } = "";
        public DateTime EstimatedDelivery { get; set; } = DateTime.Today.AddDays(5);
    }
}
