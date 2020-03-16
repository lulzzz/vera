namespace Vera.StandardAuditFileTaxation
{
  public enum AddressType
  {
    Street,
    Postal,
    Billing,
    ShipTo,
    ShipFrom
  }

  public sealed class Address
  {
    public string Street { get; set; }
    public string Number { get; set; }
    public string City { get; set; }
    public string PostalCode { get; set; }
    /// <summary>
    /// ISO 3166-1
    /// </summary>
    public string Country { get; set; }
    /// <summary>
    /// ISO 3166-2
    /// </summary>
    public string Region { get; set; }

    public AddressType Type { get; set; }
  }
}