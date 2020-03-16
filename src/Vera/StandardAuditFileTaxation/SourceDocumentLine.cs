using System;

namespace Vera.StandardAuditFileTaxation
{
  public abstract class SourceDocumentLine
  {
    public string Number { get; set; }

    /// <summary>
    /// General ledger account identifier.
    /// </summary>
    // TODO(kevin): what is this? page 109
    public string AccountID { get; set; }

    // TODO(kevin): analysis

    public string Description { get; set; }

    // TODO(kevin): what is this? page 109
    public DateTime TaxPointDate { get; set; }

    public TaxInformation[] Taxes { get; set; }
  }
}