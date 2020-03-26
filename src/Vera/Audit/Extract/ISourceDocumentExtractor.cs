using System.Collections.Generic;
using System.Linq;
using Vera.StandardAuditFileTaxation;
using Invoice = Vera.Models.Invoice;
using InvoiceLine = Vera.Models.InvoiceLine;

namespace Vera.Audit.Extract
{
    public class InvoiceSourceDocumentExtractor : IAuditDataExtractor
    {
        private readonly ICollection<StandardAuditFileTaxation.Invoice> _invoices;

        public InvoiceSourceDocumentExtractor()
        {
            _invoices = new List<StandardAuditFileTaxation.Invoice>();
        }

        public void Extract(Invoice invoice)
        {
            // TODO(kevin): map other fields
            var i = new StandardAuditFileTaxation.Invoice
            {
                SystemID = invoice.SystemId,
                Date = invoice.Date,
                Number = invoice.Number,
                IsManual = invoice.Manual,
                TerminalID = invoice.TerminalId,
                SourceID = invoice.Employee.SystemID,

                Signature = invoice.Signature,
                RawSignature = invoice.RawSignature,
                
                Period = invoice.FiscalPeriod,
                PeriodYear = invoice.FiscalYear,
                
                Lines = invoice.Lines.Select(ExtractLine).ToList()
            };

            _invoices.Add(i);
        }

        public void Apply(StandardAuditFileTaxation.Audit audit)
        {
            foreach (var invoice in _invoices)
            {
                audit.SourceDocuments.SalesInvoices.Add(invoice);
            }
        }
        
        private StandardAuditFileTaxation.InvoiceLine ExtractLine(InvoiceLine l)
        {
            return new StandardAuditFileTaxation.InvoiceLine
            {
                Description = l.Description
            };
        }
    }
}