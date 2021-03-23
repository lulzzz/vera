using System.Collections.Generic;
using Vera.Models;
using Vera.Tests.Shared;
using Invoice = Vera.Models.Invoice;

namespace Vera.Tests.Scenario
{
    public class MixedTaxRatesWithDiscountScenario : Scenario
    {
        private readonly IDictionary<TaxesCategory, decimal> _rates;
        private readonly decimal _gross;
        private readonly decimal _settlement;

        public MixedTaxRatesWithDiscountScenario(IDictionary<TaxesCategory, decimal> rates, decimal gross, decimal settlement)
        {
            _rates = rates;
            _gross = gross;
            _settlement = settlement;
        }

        protected override Invoice Create()
        {
            var builder = new InvoiceBuilder();
            var director = new InvoiceDirector(builder, AccountId, SupplierSystemId);
            director.ConstructAnonymousWithoutLines();

            var grossPerLine = _gross / _rates.Count;
            var discountAmountPerLine = _settlement / _rates.Count;
            foreach (var (category, rate) in _rates)
            {
                builder.WithProductLineSettlement(
                    ProductFactory.CreateRandomProduct(), 
                    category, 
                    (grossPerLine / rate) - (discountAmountPerLine / rate), 
                    rate, 
                    discountAmountPerLine
                );
            }

            builder.WithPayment(_gross, PaymentCategory.Cash);

            return builder.Result;
        }
    }
}