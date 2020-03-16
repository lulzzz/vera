using System;

namespace Vera.StandardAuditFileTaxation
{
  public sealed class TaxCodeDetails
  {
    /// <summary>
    /// <see cref="EnumDataModel.Name"/>
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// Start date.
    /// </summary>
    public DateTime? EffectiveDate { get; set; }

    /// <summary>
    /// End date.
    /// </summary>
    public DateTime? ExpirationDate { get; set; }

    public string Description { get; set; }

    /// <summary>
    /// Multiplier factor.
    /// </summary>
    public decimal Percentage { get; set; }

    /// <summary>
    /// ISO 3166-1.
    /// </summary>
    public string Country { get; set; }

    /// <summary>
    /// ISO 3166-2.
    /// </summary>
    public string Region { get; set; }
  }

  public sealed class TaxTableEntry
  {
    /// <summary>
    /// <see cref="EnumDataModel.Name"/>
    /// </summary>
    public string Type { get; set; }

    public string Description { get; set; }

    public TaxCodeDetails[] Details { get; set; }
  }
}