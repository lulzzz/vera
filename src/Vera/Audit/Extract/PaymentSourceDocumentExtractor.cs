using System.Collections.Generic;
using Vera.Models;
using Payment = Vera.StandardAuditFileTaxation.Payment;

namespace Vera.Audit.Extract
{
    public class PaymentSourceDocumentExtractor : IAuditDataExtractor
    {
        private readonly ICollection<Payment> _payments;

        public PaymentSourceDocumentExtractor()
        {
            _payments = new List<Payment>();
        }

        public void Extract(Invoice invoice)
        {
            foreach (var p in invoice.Payments)
            {
                // TODO(kevin): fully map these
                _payments.Add(new Payment
                {
                    Description = p.Description
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