namespace Vera.StandardAuditFileTaxation
{
  /// <summary>
  /// Note that a customer is actually also a company in disguise, it inherently has the same
  /// set of fields available and can also be fiscally registered.
  /// </summary>
  public class Customer : Company
  {
    public bool SelfBilling { get; set; }
    public string AccountID { get; set; }
  }
}