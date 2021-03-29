using Grpc.Core;
using Vera.Grpc;

namespace Vera.Integration.Tests
{
    public class SetupClient
    {
        private readonly Setup _setup;
        private readonly string _loginToken;
        private readonly string _accountId;

        public SetupClient(Setup setup, ChannelBase channel, string loginToken, string accountId)
        {
            _setup = setup;
            _loginToken = loginToken;
            _accountId = accountId;
            
            Invoice = new InvoiceService.InvoiceServiceClient(channel);
            Audit = new AuditService.AuditServiceClient(channel);
            Receipt = new ReceiptService.ReceiptServiceClient(channel);
            Supplier = new SupplierService.SupplierServiceClient(channel);
            Period = new PeriodService.PeriodServiceClient(channel);
        }

        public string AccountId => _accountId;
        
        public Metadata AuthorizedMetadata => new()
        {
            {"authorization", $"bearer {_loginToken}"}
        };

        public AccountService.AccountServiceClient Account => _setup.AccountClient;
        public InvoiceService.InvoiceServiceClient Invoice { get; }
        public AuditService.AuditServiceClient Audit { get; }
        public ReceiptService.ReceiptServiceClient Receipt { get; }
        public SupplierService.SupplierServiceClient Supplier { get; }
        public PeriodService.PeriodServiceClient Period { get; }
    }
}