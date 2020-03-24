using System.Collections.Generic;

namespace Vera.StandardAuditFileTaxation
{
  public sealed class SourceDocuments<T> where T : SourceDocument
  {
    public decimal TotalDebitExTax { get; private set; }
    public decimal TotalDebitInTax { get; private set; }
    public decimal TotalCreditExTax { get; private set; }
    public decimal TotalCreditInTax { get; private set; }

    public ICollection<T> Sources { get; } = new List<T>();

    public void Add(T source)
    {
      Sources.Add(source);

      var gross = source.Totals.Gross;
      var net = source.Totals.Net;

      if (gross > 0)
      {
        TotalCreditExTax += decimal.Round(net, 2);
        TotalCreditInTax += decimal.Round(gross, 2);
      }
      else
      {
        TotalDebitExTax += decimal.Round(net, 2);
        TotalDebitInTax += decimal.Round(gross, 2);
      }
    }
  }

  public sealed class AuditSources
  {
    public SourceDocuments<Invoice> SalesInvoices { get; } = new SourceDocuments<Invoice>();
    public SourceDocuments<Payment> Payments { get; } = new SourceDocuments<Payment>();
  }

  public sealed class Audit
  {
    public Audit(Header header)
    {
      Header = header;
    }

    public Header Header { get; }
    public MasterFile MasterFiles { get; } = new MasterFile();
    public AuditSources SourceDocuments { get; } = new AuditSources();
  }
}