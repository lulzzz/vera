using System.Collections.Generic;
using Vera.StandardAuditFileTaxation;
using Invoice = Vera.Models.Invoice;
using Payment = Vera.StandardAuditFileTaxation.Payment;

namespace Vera.Audit.Extract
{
    public class PaymentAuditDataExtractor : IAuditDataExtractor
    {
        private readonly ICollection<Payment> _payments;

        public PaymentAuditDataExtractor()
        {
            _payments = new List<Payment>();
        }

        public void Extract(Invoice invoice)
        {
            foreach (var p in invoice.Payments)
            {
                _payments.Add(new Payment
                {
                    Reference = p.SystemId,
                    TransactionDate = p.Date,
                    Description = p.Description,
                    Method = p.Category.ToString()
                });
            }
        }

        public void Apply(StandardAuditFileTaxation.Audit audit)
        {
            foreach (var payment in _payments)
            {
                audit.SourceDocuments.Payments.Add(payment);
            }
        }
    }
}