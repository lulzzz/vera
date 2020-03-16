using System;

namespace Vera.StandardAuditFileTaxation
{
  public sealed class ShippingPoint
  {
    /// <summary>
    /// Identifier of the delivery.
    /// </summary>
    public string DeliveryID { get; set; }

    /// <summary>
    /// Date the goods are delivered.
    /// </summary>
    public DateTime DeliveryDate { get; set; }

    /// <summary>
    /// Identifier of the warehouse where the goods are held. Also to identify work-in-progress and stock-in-transit.
    /// </summary>
    public string WarehouseID { get; set; }

    /// <summary>
    /// Identifier of the location of the goods in the warehouse.
    /// </summary>
    public string LocationID { get; set; }

    /// <summary>
    /// Unique consignment reference number.
    /// </summary>
    public string UCR { get; set; }

    public Address Address { get; set; }
  }
}