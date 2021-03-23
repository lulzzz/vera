using System.Collections.Generic;
using Vera.Models;
using Vera.Tests.Shared;
using Invoice = Vera.Models.Invoice;

namespace Vera.Tests.Scenario
{
    public class ReturnInvoiceScenario : Scenario
    {
        private readonly Dictionary<TaxesCategory, decimal> _rates;
        private readonly decimal _gross;
        private readonly PaymentCategory _paymentCategory;

        public ReturnInvoiceScenario(TaxesCategory category, decimal rate, decimal gross, PaymentCategory paymentCategory)
        {
            _rates = new Dictionary<TaxesCategory, decimal> {{category, rate}};
            _gross = gross;
            _paymentCategory = paymentCategory;
        }
        
        public ReturnInvoiceScenario(Dictionary<TaxesCategory, decimal> rates, decimal gross, PaymentCategory paymentCategory)
        {
            _rates = rates;
            _gross = gross;
            _paymentCategory = paymentCategory;
        }
        
        protected override Invoice Create()
        {
            var builder = new InvoiceBuilder();
            var director = new InvoiceDirector(builder, AccountId, SupplierSystemId);
            director.ConstructAnonymousWithoutLines();

            var grossPerLine = _gross / _rates.Count;
            foreach (var (category, rate) in _rates)
            {
                var product = ProductFactory.CreateRandomProduct();

                builder.WithProductLine(1, grossPerLine / rate, rate, category, product);
            }

            builder.WithPayment(_gross, _paymentCategory);
            
            var originalInvoice = builder.Result;
            
            // Number is used when generating the return for reference to the original invoice
            originalInvoice.Number = "custom_return_number";
            
            director.ConstructReturn(originalInvoice);

            return builder.Result;
        }
    }
}