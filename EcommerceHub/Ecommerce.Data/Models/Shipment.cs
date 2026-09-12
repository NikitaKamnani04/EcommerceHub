using System;
using System.Collections.Generic;

namespace Ecommerce.Data.Models;

public partial class Shipment
{
    public int ShipmentId { get; set; }

    public int OrderId { get; set; }

    public string? TrackingNumber { get; set; }

    public string? CourierName { get; set; }

    public string? ShipmentStatus { get; set; }

    public DateTime? ShippedDate { get; set; }

    public DateTime? DeliveredDate { get; set; }

    public DateTime? EstimatedDelivery { get; set; }

    public virtual Order Order { get; set; } = null!;
}
