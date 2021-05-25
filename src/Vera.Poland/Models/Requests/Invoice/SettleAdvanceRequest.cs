namespace Vera.Poland.Models.Requests.Invoice
{
  public class SettleAdvanceRequest : OrderLinePrinterRequest
  {
    public decimal AdvanceAmount { get; set; }

    public string ToDocument { get; set; }

    public decimal SupplementAmount { get; set; }

    public string SettlementOfTheAdvanceComment { get; set; }
  }
}