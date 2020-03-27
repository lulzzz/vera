using System.Collections.Generic;
using System.Linq;
using Vera.StandardAuditFileTaxation;
using Invoice = Vera.Models.Invoice;

namespace Vera.Audit.Extract
{
    public class TaxTableAuditExtractor : IAuditDataExtractor
    {
        private readonly IDictionary<string, TaxTableEntry> _taxes;

        public TaxTableAuditExtractor()
        {
            _taxes = new Dictionary<string, TaxTableEntry>();
        }

        public void Extract(Invoice invoice)
        {
            var taxes = invoice.Lines
                .Select(l => l.Taxes)
                .Where(t => !_taxes.ContainsKey(t.Code));

            foreach (var t in taxes)
            {
                _taxes[t.Code] = new TaxTableEntry
                {
                    Details = new []
                    {
                        new TaxCodeDetails
                        {
                            Code = t.Code,
                            Percentage = t.Rate,
                        }
                    }
                };
            }
        }

        public void Apply(StandardAuditFileTaxation.Audit audit)
        {
            foreach (var entry in _taxes)
            {
                audit.MasterFiles.TaxTable.Add(entry.Value);
            }
        }
    }
}