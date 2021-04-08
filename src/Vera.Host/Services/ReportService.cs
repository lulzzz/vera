using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;
using Vera.Grpc;
using Vera.Grpc.Models;
using Vera.Host.Security;
using Vera.Reports;

namespace Vera.Host.Services
{
    [Authorize]
    public class ReportService : Grpc.ReportService.ReportServiceBase
    {
        private readonly IRegisterReportGenerator _registerReportGenerator;

        public ReportService(IRegisterReportGenerator terminalReportGenerator)
        {
            _registerReportGenerator = terminalReportGenerator;
        }

        public override async Task<Grpc.RegisterReport> GenerateDailyXReport(GenerateDailyXReportRequest request, ServerCallContext context)
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

            var registerReport = new Grpc.RegisterReport
            {
                Timestamp = report.Date.ToTimestamp(),
                AccountName = report.Account.Name,
                TaxNumber = report.Account.TaxRegistrationNumber,
                Supplier = report.Supplier.Pack(),
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
