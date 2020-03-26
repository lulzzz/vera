using System.Collections.Generic;
using System.Linq;
using Vera.StandardAuditFileTaxation;
using Invoice = Vera.Models.Invoice;

namespace Vera.Audit.Extract
{
    public class TaxTableAuditExtractor : IAuditDataExtractor
    {
        private readonly ICollection<TaxTableEntry> _taxes;

        public TaxTableAuditExtractor()
        {
            _taxes = new List<TaxTableEntry>();
        }

        public void Extract(Invoice invoice)
        {
            // TODO(kevin): where to get the tax codes from?
            throw new System.NotImplementedException();
        }

        public void Apply(StandardAuditFileTaxation.Audit audit)
        {
            throw new System.NotImplementedException();
        }
    }
}