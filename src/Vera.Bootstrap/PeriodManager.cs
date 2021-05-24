using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Vera.Dependencies;
using Vera.Models;
using Vera.Reports;
using Vera.Stores;

namespace Vera.Bootstrap
{
    public class PeriodManager
    {
        private readonly IRegisterReportGenerator _registerReportGenerator;
        private readonly IReportHandlerFactory _reportHandlerFactory;
        private readonly IAccountComponentFactoryCollection _accountComponentFactoryCollection;
        private readonly IPeriodStore _periodStore;
        private readonly IDateProvider _dateProvider;

        public PeriodManager(
            IRegisterReportGenerator registerReportGenerator,
            IReportHandlerFactory reportHandlerFactory,
            IAccountComponentFactoryCollection accountComponentFactoryCollection,
            IPeriodStore periodStore,
            IDateProvider dateProvider)
        {
            _registerReportGenerator = registerReportGenerator;
            _reportHandlerFactory = reportHandlerFactory;
            _accountComponentFactoryCollection = accountComponentFactoryCollection;
            _periodStore = periodStore;
            _dateProvider = dateProvider;
        }

        public async Task ClosePeriod(ClosePeriodModel closePeriodModel)
        {
            var period = closePeriodModel.Period;
            if (period.Registers.Count == 0)
            {
                await Update();
                return;
            }

            CheckAreValidRegisters(period.Registers, closePeriodModel.Registers);
            await GenerateZReports(closePeriodModel);
            await Update();

            Task Update()
            {
                period.Closing = _dateProvider.Now;

                return _periodStore.Update(period);
            }
        }

        private async Task GenerateZReports(ClosePeriodModel model)
        {
            var account = model.Account;
            var period = model.Period;
            var factory = _accountComponentFactoryCollection.GetComponentFactory(account);
            var handler = _reportHandlerFactory.Create(factory);
            var registerReportContext = new RegisterReportContext
            {
                AccountId = account.Id,
                CompanyId = account.CompanyId,
                SupplierSystemId = period.Supplier.SystemId,
                ReportType = ReportType.Z,
                EmployeeId = model.EmployeeId
            };

            foreach (var registerEntry in period.Registers)
            {
                registerReportContext.RegisterId = registerEntry.RegisterId.ToString();
                var report = await _registerReportGenerator.Generate(registerReportContext);

                await handler.Handle(report);
            }
        }

        private void CheckAreValidRegisters(ICollection<PeriodRegisterEntry> openRegisters, IEnumerable<PeriodRegisterEntry> registersToClose)
        {
            if (openRegisters.Count != registersToClose.Count())
            {
                throw new ValidationException("Missing one or more registers in the closing");
            }

            var closingRegistersIds = registersToClose.ToDictionary(r => r.RegisterId);
            foreach (var registerEntry in openRegisters)
            {
                if (!closingRegistersIds.TryGetValue(registerEntry.RegisterId, out var closingRegister))
                {
                    throw new ValidationException($"Unkown register {registerEntry.RegisterId}");
                }

                registerEntry.ClosingAmount = closingRegister.ClosingAmount;
            }
        }

        public class ClosePeriodModel
        {
            public Account Account { get; set; }
            public Period Period { get; set; }
            public IEnumerable<PeriodRegisterEntry> Registers { get; set; }
            public string EmployeeId { get; set; }
        }
    }
}
