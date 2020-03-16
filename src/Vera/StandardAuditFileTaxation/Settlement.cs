using System;

namespace Vera.StandardAuditFileTaxation
{
  public sealed class Settlement
  {
    public string SystemID { get; set; }
    public string Description { get; set; }
    public Amount Amount { get; set; }
    public DateTime Date { get; set; }
    public string PaymentMechanism { get; set; }
  }
}