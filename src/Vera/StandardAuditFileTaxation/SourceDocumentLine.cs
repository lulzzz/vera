using System;

namespace Vera.StandardAuditFileTaxation
{
  public abstract class SourceDocumentLine
  {
    public string Number { get; set; }

    public string Description { get; set; }

    public TaxInformation[] Taxes { get; set; }
  }
}