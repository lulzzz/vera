using Vera.Models;
using Vera.Tests.Shared;

namespace Vera.Tests.Scenario
{
    public class SellProductScenario : Scenario
    {
        private readonly Product _product;

        public SellProductScenario(Product product)
        {
            _product = product;
        }

        protected override Invoice Create()
        {
            var builder = new InvoiceBuilder();
            var director = new InvoiceDirector(builder, AccountId, SupplierSystemId);
            director.ConstructAnonymousWithSingleProductPaidWithCash(_product);

            return builder.Result;
        }
    }
}