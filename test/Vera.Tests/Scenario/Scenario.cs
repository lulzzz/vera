using System;
using Vera.Models;

namespace Vera.Tests.Scenario
{
    public abstract class Scenario
    {
        public ScenarioResult Execute()
        {
            var invoice = Create();
            invoice.Remark = GetType().Name;
            
            return new(invoice);
        }

        protected abstract Invoice Create();

        public Guid AccountId { get; set; }
        public string SupplierSystemId { get; set; }
    }

    public class ScenarioResult
    {
        public ScenarioResult(Models.Invoice invoice)
        {
            Invoice = invoice;
        }
        
        public Models.Invoice Invoice { get; set; }
    }
}