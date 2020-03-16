namespace Vera.StandardAuditFileTaxation
{
  public sealed class Billable
  {
    public string SystemID { get; set; }

    public string RegistrationNumber { get; set; }
    public string TaxRegistrationNumber { get; set; }

    public string Name { get; set; }

    /// <summary>
    /// Address to bill this customer/supplier on.
    /// </summary>
    public Address Address { get; set; }
  }
}