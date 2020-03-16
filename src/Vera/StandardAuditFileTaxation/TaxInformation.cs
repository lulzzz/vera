namespace Vera.StandardAuditFileTaxation
{
  public sealed class TaxInformation
  {
    /// <summary>
    /// Tax type for look up in tables.
    /// </summary>
    public string Type { get; set; }

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
    /// Description of the value in <see cref="Base"/> E.g. litres for excises on alcoholic beverages.
    /// </summary>
    public string BaseDescription { get; set; }

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
    /// Note: this is not a SAF-T field.
    /// </summary>
    public string ExemptionCode { get; set; }

    // TODO(kevin): what is this? page 118
    public string DeclarationPeriod { get; set; }
  }
}