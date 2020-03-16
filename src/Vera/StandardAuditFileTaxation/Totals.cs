using System.Collections.Generic;

namespace Vera.StandardAuditFileTaxation
{
  public sealed class Totals
  {
    /// <summary>
    /// Summary of all the tax totals.
    /// </summary>
    public ICollection<TaxInformation> Taxes { get; set; }

    /// <summary>
    /// Total amount of freight charges.
    /// </summary>
    public decimal ShippingCosts { get; set; }

    /// <summary>
    /// Total amount excluding tax and shipping costs.
    /// </summary>
    public decimal Net { get; set; }

    /// <summary>
    /// Total amount including tax and shipping costs.
    /// </summary>
    public decimal Gross { get; set; }
  }
}