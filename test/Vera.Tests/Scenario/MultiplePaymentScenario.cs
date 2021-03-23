using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vera.Models;
using Vera.Tests.Shared;
using Invoice = Vera.Models.Invoice;

namespace Vera.Tests.Scenario
{
    public class MultiplePaymentScenario : Scenario
    {
        private readonly decimal _taxRate;
        private readonly IEnumerable<(PaymentCategory, decimal)> _payments;

        public MultiplePaymentScenario(decimal taxRate, IEnumerable<(PaymentCategory, decimal)> payments)
        {
            _taxRate = taxRate;
            _payments = payments;
        }

        protected override Invoice Create()
        {
            var totalPaidAmount = _payments.Sum(x => x.Item2);
        
            var builder = new InvoiceBuilder();
            var director = new InvoiceDirector(builder, AccountId, SupplierSystemId);
            director.ConstructAnonymousWithoutLines();

            foreach (var (paymentCategory, amount) in _payments)
            {
                builder.WithPayment(amount, paymentCategory);
            }

            builder.WithAmount(totalPaidAmount, _taxRate);

            return builder.Result;
        }
    }
}