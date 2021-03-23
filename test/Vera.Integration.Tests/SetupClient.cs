using System.Threading.Tasks;
using Grpc.Core;
using Vera.Grpc;

namespace Vera.Integration.Tests
{
    public class SetupClient
    {
        private readonly Setup _setup;
        private readonly string _loginToken;

        public SetupClient(Setup setup, ChannelBase channel, string loginToken, string accountId)
        {
            _setup = setup;
            _loginToken = loginToken;
            AccountId = accountId;
            
            Invoice = new InvoiceService.InvoiceServiceClient(channel);
            Audit = new AuditService.AuditServiceClient(channel);
            Receipt = new ReceiptService.ReceiptServiceClient(channel);
            Supplier = new SupplierService.SupplierServiceClient(channel);
            Period = new PeriodService.PeriodServiceClient(channel);
        }

        public async Task OpenPeriod()
        {
            await Period.OpenPeriodAsync(new OpenPeriodRequest
            {
                SupplierSystemId = SupplierSystemId
            }, AuthorizedMetadata);
        }

        public string AccountId { get; }
        public string SupplierSystemId { get; set; }

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