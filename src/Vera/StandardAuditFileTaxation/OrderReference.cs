using System;

namespace Vera.StandardAuditFileTaxation
{
  public sealed class OrderReference
  {
    /// <summary>
    /// Reference to the originating order.
    /// </summary>
    public string OrderID { get; set; }

    /// <summary>
    /// Date of the referencing order.
    /// </summary>
    public DateTime Date { get; set; }
  }
}