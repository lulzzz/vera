namespace Vera.StandardAuditFileTaxation
{
  public sealed class TaxInformation
  {
    /// <summary>
    /// Tax code for look up in tables.
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// Percentage as a factor. (E.g. 1.21 for 21%).
    /// </summary>
    public decimal Rate { get; set; }

    /// <summary>
    /// Base on which the tax is calculated. This can be an amount or a quantity (e.g. litres).
    /// </summary>
    public decimal Base { get; set; }

    /// <summary>
    /// Amount of the tax.
    /// </summary>
    public Amount Amount { get; set; }

    /// <summary>
    /// Reason for the tax exemption, if any.
    /// </summary>
    public string ExemptionReason { get; set; }

    /// <summary>
    /// Tax code for the exemption.
    /// </summary>
    public string ExemptionCode { get; set; }

    public TaxInformationCategory Category { get; set; }
  }

  public enum TaxInformationCategory
  {
    High,
    Low,
    Zero,
    Exempt,
    Intermediate
  }
}