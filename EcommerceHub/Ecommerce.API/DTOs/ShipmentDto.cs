namespace Ecommerce.API.DTOs
{
    public class ShipmentCreateDto
    {
        public int OrderID { get; set; }
        public string TrackingNumber { get; set; } = "";
        public string CourierName { get; set; } = "";
        public DateTime? EstimatedDelivery { get; set; }
    }

    public class ShipmentUpdateDto
    {
        public string? ShipmentStatus { get; set; }   // "In Transit", "Delivered" etc.
        public DateTime? DeliveredDate { get; set; }
    }
}
