using Grpc.Core;
using System.Threading.Tasks;
using Vera.Grpc;
using Vera.Host.Security;

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
            Register = new RegisterService.RegisterServiceClient(channel);
            Report = new ReportService.ReportServiceClient(channel);
            EventLog = new EventLogService.EventLogServiceClient(channel);
        }

        public async Task OpenPeriod()
        {
            await Period.OpenPeriodAsync(new OpenPeriodRequest
            {
                SupplierSystemId = SupplierSystemId
            }, AuthorizedMetadata);
        }

        public async Task<OpenRegisterReply> OpenRegister(decimal amount)
        {
            var createRegisterRequest = new CreateRegisterRequest()
            {
                SupplierSystemId = SupplierSystemId,
            };

            var register = await Register.CreateRegisterAsync(createRegisterRequest, AuthorizedMetadata);

            var reply = await Register.OpenRegisterAsync(new OpenRegisterRequest
            {
                SupplierSystemId = SupplierSystemId,
                OpeningAmount = amount,
                RegisterId = register.Id,
            }, AuthorizedMetadata);

            return reply;
        }

        public string AccountId { get; }
        public string SupplierSystemId { get; set; }

        public Metadata AuthorizedMetadata => new()
        {
            { MetadataKeys.Authorization, $"bearer {_loginToken}" },
            { MetadataKeys.AccountId, AccountId }
        };

        public AccountService.AccountServiceClient Account => _setup.AccountClient;
        public InvoiceService.InvoiceServiceClient Invoice { get; }
        public AuditService.AuditServiceClient Audit { get; }
        public ReceiptService.ReceiptServiceClient Receipt { get; }
        public SupplierService.SupplierServiceClient Supplier { get; }
        public PeriodService.PeriodServiceClient Period { get; }
        public RegisterService.RegisterServiceClient Register { get; set; }
        public ReportService.ReportServiceClient Report { get; set; }

        public EventLogService.EventLogServiceClient EventLog { get; set; }
    }
}