using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Vera.Bootstrap;
using Vera.Concurrency;
using Vera.Dependencies;
using Vera.Grpc;
using Vera.Grpc.Shared;
using Vera.Host.Mapping;
using Vera.Host.Security;
using static Vera.Bootstrap.PeriodManager;
using Vera.Stores;
using System.Linq;

namespace Vera.Host.Services
{
    [Authorize]
    public class PeriodService : Grpc.PeriodService.PeriodServiceBase
    {
        private readonly ISupplierStore _supplierStore;
        private readonly IPeriodStore _periodStore;
        private readonly IAccountStore _accountStore;
        private readonly IRegisterStore _registerStore;
        private readonly IDateProvider _dateProvider;
        private readonly ILocker _locker;
        private readonly PeriodManager periodManager;

        public PeriodService(
            ISupplierStore supplierStore,
            IPeriodStore periodStore,
            IAccountStore accountStore,
            IRegisterStore registerStore,
            IDateProvider dateProvider,
        ILocker locker,
            PeriodManager periodManager)
        {
            _supplierStore = supplierStore;
            _periodStore = periodStore;
            _accountStore = accountStore;
            _registerStore = registerStore;
            _dateProvider = dateProvider;
            _locker = locker;
            this.periodManager = periodManager;
        }

        public override async Task<OpenPeriodReply> OpenPeriod(OpenPeriodRequest request, ServerCallContext context)
        {
            var supplier = await context.ResolveSupplier(_supplierStore, request.SupplierSystemId);

            await using (await _locker.Lock(supplier.Id.ToString(), TimeSpan.FromSeconds(5)))
            {
                var currentPeriod = await _periodStore.GetOpenPeriodForSupplier(supplier.Id);
                if (currentPeriod != null)
                {
                    throw new RpcException(new Status(StatusCode.AlreadyExists, "period is already open"));
                }

                var period = new Models.Period
                {
                    Opening = _dateProvider.Now,
                    SupplierId = supplier.Id
                };

                await _periodStore.Store(period);

                return new OpenPeriodReply
                {
                    Id = period.Id.ToString()
                };
            }
        }

        public override async Task<Empty> ClosePeriod(ClosePeriodRequest request, ServerCallContext context)
        {
            var account = await context.ResolveAccount(_accountStore);
            var supplier = await context.ResolveSupplier(_supplierStore, request.SupplierSystemId);

            var period = await GetAndValidate(new PeriodValidationModel
            {
                SupplierId = supplier.Id,
                PeriodId = request.Id
            });

            var registersToClose = request.Registers.Select(x => Guid.Parse(x.Key));

            var registers = await _registerStore.GetRegistersBasedOnSupplier(registersToClose, supplier.Id);

            if (registers.Count != request.Registers.Count)
            {
                throw new RpcException(new Status(
                    StatusCode.FailedPrecondition, 
                    $"expected {registers.Count} register(s) but got {request.Registers.Count}"
                ));
            }

            var periodRegisterEntries = registers.Select(r => new Models.PeriodRegisterEntry
            {
                RegisterId = r.Id, 
                ClosingAmount = request.Registers[r.Id.ToString()].ClosingAmount
            }).ToList();

            try
            {
                await periodManager.ClosePeriod(new ClosePeriodModel
                {
                    Account = account,
                    Period = period,
                    Registers = periodRegisterEntries,
                    EmployeeId = request.EmployeeId
                });
            }
            catch (ValidationException validationException)
            {
                throw new RpcException(new Status(StatusCode.FailedPrecondition, validationException.Message));
            }

            return new Empty();
        }

        public override async Task<Period> Get(GetPeriodRequest request, ServerCallContext context)
        {
            var supplier = await context.ResolveSupplier(_supplierStore, request.SupplierSystemId);

            var period = await GetAndValidate(new PeriodValidationModel
            {
                PeriodId = request.Id,
                SupplierId = supplier.Id
            });

            return period.Pack();
        }

        public override async Task<Period> GetCurrentPeriod(GetCurrentPeriodRequest request, ServerCallContext context)
        {
            var supplier = await context.ResolveSupplier(_supplierStore, request.SupplierSystemId);

            var period = await _periodStore.GetOpenPeriodForSupplier(supplier.Id);
            if (period == null)
            {
                throw new RpcException(new Status(StatusCode.FailedPrecondition, "no open period"));
            }

            return period.Pack();
        }

        private async Task<Models.Period> GetAndValidate(PeriodValidationModel model)
        {
            var period = await _periodStore.Get(Guid.Parse(model.PeriodId), model.SupplierId);
            if (period == null)
            {
                throw new RpcException(new Status(StatusCode.FailedPrecondition, "period does not exist"));
            }

            return period;
        }

        public class PeriodValidationModel
        {
            public Guid SupplierId { get; set; }
            public string PeriodId { get; set; }
        }
    }
}