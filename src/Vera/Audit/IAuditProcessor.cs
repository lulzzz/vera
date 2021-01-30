using System.Threading.Tasks;
using Vera.Stores;

namespace Vera.Audit
{
    public interface IAuditProcessor
    {
        // TODO: should return stuff needed in the ArchiveService
        Task Process(AuditCriteria criteria);
    }

    public class AuditProcessor : IAuditProcessor
    {
        private readonly IInvoiceStore _invoiceStore;

        public AuditProcessor(IInvoiceStore invoiceStore)
        {
            _invoiceStore = invoiceStore;
        }

        public Task Process(AuditCriteria criteria)
        {
            // get invoices for range and account
            // create model with audit creator
            // store model that was created
            // profit?

            // one certification outputs a file, others don't

            // this would contain the steps to create an audit (file) for the given criteria

            return Task.CompletedTask;
        }
    }
}