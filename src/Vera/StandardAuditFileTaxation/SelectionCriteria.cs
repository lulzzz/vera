using System;

namespace Vera.StandardAuditFileTaxation
{
  public sealed class SelectionCriteria
  {
    /// <summary>
    /// Identifies the tax jurisdiction for whose purpose the SAF has been created. Principally for use where a single revenue body covers more than
    /// one territory.
    /// </summary>
    public string TaxReportingJurisdiction { get; set; }

    /// <summary>
    /// For use where data has been extracted from the full data set by reference to a specific corporate entity.
    /// </summary>
    public string CompanyEntity { get; set; }

    public DateTime SelectionStartDate { get; set; }
    public DateTime SelectionEndDate { get; set; }

    public int PeriodStart { get; set; }
    public int PeriodEnd { get; set; }

    public int PeriodStartYear { get; set; }
    public int PeriodEndYear { get; set; }

    public string DocumentType { get; set; }
  }
}