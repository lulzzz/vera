using System.Collections;
using System.Collections.Generic;
using Vera.Invoices;
using Vera.Invoices.Validation;
using Vera.Portugal.Invoices.InvoiceValidators;

namespace Vera.Portugal.Invoices
{
    public class PortugalInvoiceValidators : IEnumerable<IInvoiceValidator>
    {
        public IEnumerator<IInvoiceValidator> GetEnumerator()
        {
            yield return new FaturaInvoiceLimitValidator();
            yield return new MixedQuantitiesValidator();
            yield return new TaxExemptionValidator();
            yield return new CreditReferenceValidator();
            yield return new TotalPaidValidator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
