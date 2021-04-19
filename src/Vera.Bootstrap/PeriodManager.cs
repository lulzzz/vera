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

        public async Task ClosePeriod(Period period, Account account, IEnumerable<Register> registersToClose)
        {
            if (period.Registers.Count == 0)
            {
                await Update();
                return;
            }

            CheckAreValidRegisters(period.Registers, registersToClose);
            await GenerateZReports(period, account);
            await Update();

            Task Update()
            {
                period.Closing = _dateProvider.Now;

                return _periodStore.Update(period);
            }
        }

        private async Task GenerateZReports(Period period, Account account)
        {
            var factory = _accountComponentFactoryCollection.GetComponentFactory(account);
            var handler = _reportHandlerFactory.Create(factory);
            var registerReportContext = new RegisterReportContext
            {
                AccountId = account.Id,
                CompanyId = account.CompanyId,
                SupplierSystemId = period.Supplier.SystemId,
                ReportType = ReportType.Z
            };

            foreach (var register in period.Registers)
            {
                registerReportContext.RegisterId = register.Id.ToString();
                var report = await _registerReportGenerator.Generate(registerReportContext);

                await handler.Handle(report);
            }
        }

        private void CheckAreValidRegisters(ICollection<Register> openRegisters, IEnumerable<Register> registersToClose)
        {
            if (openRegisters.Count != registersToClose.Count())
            {
                throw new ValidationException("Missing one or more registers in the closing");
            }

            var closingRegistersIds = registersToClose.ToDictionary(r => r.Id);
            foreach (var register in openRegisters)
            {
                if (!closingRegistersIds.TryGetValue(register.Id, out var closingRegister))
                {
                    throw new ValidationException($"Unkown register {register.Id}");
                }

                register.ClosingAmount = closingRegister.ClosingAmount;
            }
        }
    }
}
