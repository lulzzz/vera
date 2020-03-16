namespace Vera.StandardAuditFileTaxation
{
  public sealed class PaymentLine : SourceDocumentLine
  {
    public string SourceDocumentID { get; set; }

    public string CustomerID { get; set; }

    public string SupplierID { get; set; }

    public DebitCreditIndicator DebitCreditIndicator { get; set; }

    /// <summary>
    /// Amount for the transaction excluding taxes.
    /// </summary>
    public Amount Amount { get; set; }
  }
}