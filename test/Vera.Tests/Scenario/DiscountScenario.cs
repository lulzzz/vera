using System.Threading.Tasks;
using Vera.Models;
using Vera.Tests.Shared;
using Invoice = Vera.Models.Invoice;

namespace Vera.Tests.Scenario
{
    public class DiscountScenario : Scenario
    {
        private readonly decimal _taxRate;
        private readonly decimal _gross;
        private readonly decimal _discountRate;
        private readonly PaymentCategory _paymentCategory;

        public DiscountScenario(decimal taxRate, decimal gross, decimal discountRate, PaymentCategory paymentCategory)
        {
            _taxRate = taxRate;
            _gross = gross;
            _discountRate = discountRate;
            _paymentCategory = paymentCategory;
        }

        protected override Invoice Create()
        {
            var builder = new InvoiceBuilder();
            var director = new InvoiceDirector(builder, AccountId, SupplierSystemId);
            director.ConstructWithSettlement(_gross, _taxRate, _paymentCategory, _discountRate);
            
            return builder.Result;
        }
    }
}