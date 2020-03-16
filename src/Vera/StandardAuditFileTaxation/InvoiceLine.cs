using System;
using System.Collections.Generic;

namespace Vera.StandardAuditFileTaxation
{
  public sealed class InvoiceLine : SourceDocumentLine
  {
    public OrderReference[] OrderReferences { get; set; }

    public ShippingPoint ShipFrom { get; set; }

    public ShippingPoint ShipTo { get; set; }

    public ProductTypes Type { get; set; }

    public DateTime Date { get; set; }

    public string ProductCode { get; set; }

    // TODO(kevin): delivery

    public int Quantity { get; set; }

    /// <summary>
    /// Unit of measure of the <see cref="Quantity"/>.
    /// </summary>
    public string Unit { get; set; }

    /// <summary>
    /// Factor to multiply the <see cref="Quantity"/> in the base unit of measure (e.g. from box to each).
    /// </summary>
    public decimal UnitBaseConversionFactor { get; set; } = 1m;

    /// <summary>
    /// Unit price for the unit/group of units per <see cref="Unit"/>.
    /// </summary>
    public decimal UnitPrice { get; set; }
    public decimal UnitPriceInTax { get; set; }

    public ICollection<CreditNote> CreditReferences { get; set; } = new List<CreditNote>();

    /// <summary>
    /// Amount for the line excluding taxes and freight charges, but including any discounts.
    /// </summary>
    public Amount Amount { get; set; }

    /// <summary>
    /// Amount for the line including taxes and discounts.
    /// </summary>
    public Amount AmountInTax { get; set; }

    /// <summary>
    /// Amount for the freight charges.
    /// </summary>
    public Amount ShippingCostsAmount { get; set; }

    public ICollection<Settlement> Settlements { get; set; }
  }
}

