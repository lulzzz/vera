using System;

namespace Vera.StandardAuditFileTaxation
{
  /// <summary>
  /// Type: HeaderStructure
  /// </summary>
  public sealed class Header
  {
    /// <summary>
    /// Identification of the standard audit file version.
    /// </summary>
    public string Version { get; set; }

    /// <summary>
    /// ISO 3166-1
    /// </summary>
    public string Country { get; set; }

    /// <summary>
    /// ISO 3166-2
    /// </summary>
    public string Region { get; set; }

    public DateTime CreationTime { get; set; }

    public string SoftwareCompanyName { get; set; }
    public string SoftwareName { get; set; }
    public string SoftwareVersion { get; set; }

    public Company Company { get; set; }

    /// <summary>
    /// ISO 4217
    /// </summary>
    public string DefaultCurrencyCode { get; set; }

    public SelectionCriteria SelectionCriteria { get; set; }

    /// <summary>
    /// Invoice accounting, cash accounting, delivery, etc.
    /// </summary>
    public string TaxAccountingBasis { get; set; }

    /// <summary>
    /// Company, division or branch reference.
    /// </summary>
    public string TaxEntity { get; set; }
  }
}