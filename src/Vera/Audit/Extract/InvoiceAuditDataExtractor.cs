using System.Collections.Generic;
using System.Linq;
using Vera.StandardAuditFileTaxation;
using Invoice = Vera.Models.Invoice;
using InvoiceLine = Vera.Models.InvoiceLine;

namespace Vera.Audit.Extract
{
    public class InvoiceAuditDataExtractor : IAuditDataExtractor
    {
        private readonly ICollection<StandardAuditFileTaxation.Invoice> _invoices;

        public InvoiceAuditDataExtractor()
        {
            _invoices = new List<StandardAuditFileTaxation.Invoice>();
        }

        public void Extract(Invoice invoice)
        {
            // TODO(kevin): map other fields
            var model = new StandardAuditFileTaxation.Invoice
            {
                SystemID = invoice.SystemId,
                Date = invoice.Date,
                Number = invoice.Number,
                IsManual = invoice.Manual,
                TerminalID = invoice.TerminalId,
                SourceID = invoice.Employee.SystemID,

                Signature = invoice.Signature,
                RawSignature = invoice.RawSignature,

                // TODO(kevin): where to get this field from?
                SignatureKeyVersion = 1,

                Period = invoice.FiscalPeriod,
                PeriodYear = invoice.FiscalYear,

                Lines = invoice.Lines.Select((l, i) => ExtractLine(i, l)).ToList()
            };

            _invoices.Add(model);
        }

        public void Apply(StandardAuditFileTaxation.Audit audit)
        {
            foreach (var invoice in _invoices)
            {
                audit.SourceDocuments.SalesInvoices.Add(invoice);
            }
        }

        private StandardAuditFileTaxation.InvoiceLine ExtractLine(int i, InvoiceLine l)
        {
            var settlements = new List<Settlement>();

            if (l.Settlements != null)
            {
                settlements.AddRange(l.Settlements.Select(settlement => new Settlement
                {
                    Amount = new Amount(settlement.Amount), Description = settlement.Description,
                    SystemID = settlement.SystemId
                }));
            }

            return new StandardAuditFileTaxation.InvoiceLine
            {
                Number = (i + 1).ToString(),
                ProductCode = l.Product?.Code,
                Description = l.Description,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice,
                Amount = new Amount(l.Gross),
                Settlements = settlements,
                Taxes = new []
                {
                    new TaxInformation
                    {
                        Code = l.Taxes.Code,
                        Rate = l.Taxes.Rate
                    }
                }
            };
        }
    }
}