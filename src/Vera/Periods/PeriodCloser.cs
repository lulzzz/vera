using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Vera.Dependencies;
using Vera.Dependencies.Handlers;
using Vera.Models;
using Vera.Reports;
using Vera.Stores;

namespace Vera.Periods
{
    public class PeriodCloser : IPeriodCloser
    {
        private readonly IRegisterReportGenerator _registerReportGenerator;
        private readonly IPeriodStore _periodStore;
        private readonly IDateProvider _dateProvider;

        public PeriodCloser(
            IRegisterReportGenerator registerReportGenerator,
            IPeriodStore periodStore,
            IDateProvider dateProvider
        )
        {
            _registerReportGenerator = registerReportGenerator;
            _periodStore = periodStore;
            _dateProvider = dateProvider;
        }

        public async Task ClosePeriod(IHandlerChain<RegisterReport> handler, PeriodClosingContext closingContext)
        {
            var period = closingContext.Period;

            if (period.Registers.Count != closingContext.Registers.Count)
            {
                // Mismatch in opened registers vs number that are being closed
                throw new ValidationException("missing one or more registers in the closing");
            }

            var registerLookup = closingContext.Registers.ToDictionary(x => x.Id);
            foreach (var registerInPeriod in period.Registers)
            {
                if (!registerLookup.TryGetValue(registerInPeriod.RegisterId, out var closingRegister))
                {
                    throw new ValidationException($"unknown register {registerInPeriod.RegisterId}");
                }

                registerInPeriod.ClosingAmount = closingRegister.ClosingAmount;
            }

            await GenerateEndOfDayReports(handler, closingContext);

            period.Closing = _dateProvider.Now;

            await _periodStore.Update(period);
        }

        private async Task GenerateEndOfDayReports(IHandlerChain<RegisterReport> handler, PeriodClosingContext closingContext)
        {
            var account = closingContext.Account;
            var period = closingContext.Period;

            var registerReportContext = new RegisterReportContext
            {
                AccountId = account.Id,
                CompanyId = account.CompanyId,
                SupplierId = period.SupplierId,
                RegisterReportType = RegisterReportType.EndOfDay,
                EmployeeId = closingContext.EmployeeId
            };

            foreach (var registerEntry in period.Registers)
            {
                registerReportContext.RegisterId = registerEntry.RegisterId;

                var report = await _registerReportGenerator.Generate(registerReportContext);

                await handler.Handle(report);
            }
        }
    }
}
