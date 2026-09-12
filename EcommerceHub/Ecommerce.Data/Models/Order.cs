using System;
using System.Collections.Generic;

namespace Ecommerce.Data.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int? UserId { get; set; }

    public DateTime? OrderDate { get; set; }

    public int? OrderStatusId { get; set; }

    public int? PaymentStatusId { get; set; }

    public decimal? TotalAmount { get; set; }

    public virtual ICollection<OrderCoupon> OrderCoupons { get; set; } = new List<OrderCoupon>();

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual OrderStatusMaster? OrderStatus { get; set; }

    public virtual PaymentStatusMaster? PaymentStatus { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
}
