using Grpc.Core;
using System.Threading.Tasks;
using Bogus;
using Vera.Grpc;
using Vera.Grpc.Shared;
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
            MessageTemplateClient = new MessageTemplateService.MessageTemplateServiceClient(channel);
        }

        public async Task OpenPeriod()
        {
            await Period.OpenPeriodAsync(new OpenPeriodRequest
            {
                SupplierSystemId = SupplierSystemId
            }, AuthorizedMetadata);
        }

        public async Task<string> OpenRegister(decimal amount)
        {
            var createRegisterRequest = new CreateRegisterRequest()
            {
                SupplierSystemId = SupplierSystemId,
                SystemId = new Faker().Random.AlphaNumeric(16)
            };

            await Register.CreateRegisterAsync(createRegisterRequest, AuthorizedMetadata);

            await Period.OpenRegisterAsync(new OpenRegisterRequest
            {
                SupplierSystemId = SupplierSystemId,
                OpeningAmount = amount,
                RegisterSystemId = createRegisterRequest.SystemId,
            }, AuthorizedMetadata);

            return createRegisterRequest.SystemId;
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

        public MessageTemplateService.MessageTemplateServiceClient MessageTemplateClient { get; }
    }
}
