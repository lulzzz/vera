using System.Collections.Generic;
using System.Threading.Tasks;
using Vera.Models;
using Vera.Tests.Shared;
using Invoice = Vera.Models.Invoice;

namespace Vera.Tests.Scenario
{
    public class MultipleTaxRateScenario : Scenario
    {
        private readonly IDictionary<TaxesCategory, decimal> _categoriesToRates;
        private readonly decimal _gross;

        public MultipleTaxRateScenario(IDictionary<TaxesCategory, decimal> categoriesToRates, decimal gross)
        {
            _categoriesToRates = categoriesToRates;
            _gross = gross;
        }

        protected override Invoice Create()
        {
            var grossPerLine = _gross / _categoriesToRates.Count;
            
            var builder = new InvoiceBuilder();
            var director = new InvoiceDirector(builder, AccountId, SupplierSystemId);
            director.ConstructWithTaxRates(grossPerLine, PaymentCategory.Cash, _categoriesToRates);

            return builder.Result;
        }
    }
}