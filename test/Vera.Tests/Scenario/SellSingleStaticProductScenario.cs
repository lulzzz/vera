using Vera.Models;
using Vera.Tests.Shared;
using Invoice = Vera.Models.Invoice;

namespace Vera.Tests.Scenario
{
    public class SellSingleStaticProductScenario : Scenario
    {
        private readonly decimal _taxRate;
        private readonly decimal _gross;
        private readonly PaymentCategory _paymentCategory;

        public SellSingleStaticProductScenario(decimal taxRate, decimal gross, PaymentCategory paymentCategory)
        {
            _taxRate = taxRate;
            _gross = gross;
            _paymentCategory = paymentCategory;
        }
        
        protected override Invoice Create()
        {
            var builder = new InvoiceBuilder();
            var director = new InvoiceDirector(builder, AccountId, SupplierSystemId);
            director.ConstructAnonymousWithSingleProductPaidWithCategory(_gross, 1, _taxRate, _paymentCategory);

            return builder.Result;
        }
    }
}