using System.Security.Cryptography;
using Vera.Audit;
using Vera.Concurrency;
using Vera.Invoices;
using Vera.Portugal.Models;
using Vera.Signing;
using Vera.Stores;

namespace Vera.Portugal
{
    public sealed class ComponentFactory : IComponentFactory
    {
        private readonly IInvoiceStore _invoiceStore;
        private readonly ILocker _locker;
        private readonly RSA _rsa;

        public ComponentFactory(IInvoiceStore invoiceStore, ILocker locker, RSA rsa)
        {
            _invoiceStore = invoiceStore;
            _locker = locker;
            _rsa = rsa;
        }

        public ILocker CreateLocker()
        {
            return _locker;
        }

        public IInvoiceBucketGenerator CreateInvoiceBucketGenerator()
        {
            return new InvoiceBucketGenerator();
        }

        public IInvoiceNumberGenerator CreateInvoiceNumberGenerator()
        {
            return new InvoiceNumberGenerator();
        }

        public IPackageSigner CreatePackageSigner()
        {
            return new PackageSigner(_rsa);
        }

        public IAuditFacade CreateFacade()
        {
            return new AuditFacade<AuditFile>(
                new AuditTransformer(), 
                new AuditArchive(), 
                _invoiceStore
            );
        }
    }
}