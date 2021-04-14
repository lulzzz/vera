using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;
using Vera.Bootstrap;
using Vera.Grpc;
using Vera.Grpc.Models;
using Vera.Grpc.Shared;
using Vera.Host.Security;
using Vera.Reports;
using Vera.Stores;

namespace Vera.Host.Services
{
    [Authorize]
    public class ReportService : Grpc.ReportService.ReportServiceBase
    {
        private readonly IRegisterReportGenerator _registerReportGenerator;
        private readonly IAccountStore _accountStore;
        private readonly IAccountComponentFactoryCollection _accountComponentFactoryCollection;
        private readonly IReportHandlerFactory _reportHandlerFactory;

        public ReportService(IRegisterReportGenerator registerReportGenerator,
            IAccountStore accountStore,
            IAccountComponentFactoryCollection accountComponentFactoryCollection, 
            IReportHandlerFactory reportHandlerFactory)
        {
            _registerReportGenerator = registerReportGenerator;
            _accountStore = accountStore;
            _accountComponentFactoryCollection = accountComponentFactoryCollection;
            _reportHandlerFactory = reportHandlerFactory;
        }

        public override async Task<RegisterReport> GenerateDailyXReport(GenerateDailyXReportRequest request, ServerCallContext context)
        {
            if (string.IsNullOrEmpty(request.SupplierSystemId) || string.IsNullOrEmpty(request.RegisterId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Failed preconditions, invalid arguments"));
            }

            var registerReportContext = new RegisterReportContext
            {
                AccountId = context.GetAccountId(),
                CompanyId = context.GetCompanyId(),
                SupplierSystemId = request.SupplierSystemId,
                RegisterId = request.RegisterId
            };

            var report = await _registerReportGenerator.Generate(registerReportContext);

            var account = await context.ResolveAccount(_accountStore);
            var factory = _accountComponentFactoryCollection.GetComponentFactory(account);
            var handler = _reportHandlerFactory.Create(factory);

            await handler.Handle(report);

            var registerReport = new RegisterReport
            {
                Number = report.Number,
                Sequence = report.Sequence,
                Signature = new Signature
                {
                    Input = ByteString.CopyFromUtf8(report.Signature.Input),
                    Output = ByteString.CopyFrom(report.Signature.Output)
                },
                Timestamp = report.Date.ToTimestamp(),
                AccountName = report.Account.Name,
                TaxNumber = report.Account.TaxRegistrationNumber,
                SupplierId = report.SupplierId.ToString(),
                Discount = new DiscountReport
                {
                    Amount = report.Discount.Amount,
                    Count = report.Discount.Count
                },
                Return = new ReturnReport
                {
                    Amount = report.Return.Amount,
                    Count = report.Return.Count
                },
                Totals = new TotalsReport
                {
                    Cash = report.Totals.Cash,
                    Gross = report.Totals.Gross,
                    Net = report.Totals.Net,
                    Return = report.Totals.Return
                }
            };

            registerReport.Payments.AddRange(report.Payments.Select(r => r.Pack()));
            registerReport.Taxes.AddRange(report.Taxes.Select(r => r.Pack()));
            registerReport.Products.AddRange(report.Products.Select(r => r.Pack()));
            registerReport.EmployeePayments.AddRange(report.PaymentsPerEmployee.Select(r => r.Pack()));

            return registerReport;
        }
    }
}
