using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Vera.Bootstrap;
using Vera.Grpc;
using Vera.Grpc.Shared;
using Vera.Host.Mapping;
using Vera.Host.Security;
using Vera.Stores;
using System.Linq;
using Vera.Periods;
using Vera.Reports;

namespace Vera.Host.Services
{
    [Authorize]
    public class PeriodService : Grpc.PeriodService.PeriodServiceBase
    {
        private readonly ISupplierStore _supplierStore;
        private readonly IPeriodStore _periodStore;
        private readonly IAccountStore _accountStore;
        private readonly IRegisterStore _registerStore;
        private readonly IAccountComponentFactoryCollection _accountComponentFactoryCollection;
        private readonly IReportHandlerFactory _reportHandlerFactory;
        private readonly IPeriodOpener _periodOpener;
        private readonly IPeriodCloser _periodCloser;

        public PeriodService(
            ISupplierStore supplierStore,
            IPeriodStore periodStore,
            IAccountStore accountStore,
            IRegisterStore registerStore,
            IAccountComponentFactoryCollection accountComponentFactoryCollection,
            IReportHandlerFactory reportHandlerFactory,
            IPeriodOpener periodOpener,
            IPeriodCloser periodCloser
        )
        {
            _supplierStore = supplierStore;
            _periodStore = periodStore;
            _accountStore = accountStore;
            _registerStore = registerStore;
            _accountComponentFactoryCollection = accountComponentFactoryCollection;
            _reportHandlerFactory = reportHandlerFactory;
            _periodOpener = periodOpener;
            _periodCloser = periodCloser;
        }

        public override async Task<OpenPeriodReply> OpenPeriod(OpenPeriodRequest request, ServerCallContext context)
        {
            var supplier = await context.ResolveSupplier(_supplierStore, request.SupplierSystemId);
            var period = await _periodOpener.Open(supplier.Id);

            return new OpenPeriodReply
            {
                Id = period.Id.ToString()
            };
        }

        public override async Task<Empty> OpenRegister(OpenRegisterRequest request, ServerCallContext context)
        {
            var supplier = await context.ResolveSupplier(_supplierStore, request.SupplierSystemId);

            var period = await _periodStore.GetOpenPeriodForSupplier(supplier.Id) ??
                         throw new RpcException(new Status(StatusCode.FailedPrecondition, "no open period"));

            var register = await _registerStore.GetBySystemIdAndSupplierId(supplier.Id, request.RegisterSystemId) ??
                           throw new RpcException(new Status(StatusCode.NotFound, "register not found for supplier"));

            var registerEntry = new Models.PeriodRegisterEntry
            {
                OpeningAmount = request.OpeningAmount,
                RegisterId = register.Id,
                RegisterSystemId = register.SystemId
            };

            period.Registers.Add(registerEntry);

            await _periodStore.Update(period);

            return new Empty();
        }

        public override async Task<Empty> ClosePeriod(ClosePeriodRequest request, ServerCallContext context)
        {
            var account = await context.ResolveAccount(_accountStore);
            var supplier = await context.ResolveSupplier(_supplierStore, request.SupplierSystemId);
            var period = await GetPeriodOrThrow(Guid.Parse(request.Id), supplier.Id);
            var registers = await _registerStore.GetOpenRegistersForSupplier(supplier.Id);

            if (registers.Count != request.Registers.Count)
            {
                throw new RpcException(new Status(
                    StatusCode.FailedPrecondition,
                    $"expected {registers.Count} register(s) but got {request.Registers.Count}"
                ));
            }

            var periodClosingContext = new PeriodClosingContext
            {
                Account = account,
                Period = period,
                EmployeeId = request.EmployeeId
            };

            var registerLookup = registers.ToDictionary(x => x.SystemId);

            foreach (var e in request.Registers)
            {
                if (!registerLookup.TryGetValue(e.SystemId, out var register))
                {
                    throw new RpcException(new Status(
                        StatusCode.OutOfRange,
                        $"register {e.SystemId} does not exist"
                    ));
                }

                periodClosingContext.Registers.Add(new PeriodClosingContext.RegisterEntry
                {
                    Id = register.Id,
                    ClosingAmount = e.Amount
                });
            }

            var handler = _reportHandlerFactory.Create(_accountComponentFactoryCollection.GetComponentFactory(account));

            try
            {
                await _periodCloser.ClosePeriod(handler, periodClosingContext);
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
            var period = await GetPeriodOrThrow(Guid.Parse(request.Id), supplier.Id);

            return period.Pack();
        }

        public override async Task<Period> GetCurrentPeriod(GetCurrentPeriodRequest request, ServerCallContext context)
        {
            var supplier = await context.ResolveSupplier(_supplierStore, request.SupplierSystemId);

            var period = await _periodStore.GetOpenPeriodForSupplier(supplier.Id) ??
                         throw new RpcException(new Status(StatusCode.FailedPrecondition, "no open period"));

            return period.Pack();
        }

        private async Task<Models.Period> GetPeriodOrThrow(Guid periodId, Guid supplierId)
        {
            return await _periodStore.Get(periodId, supplierId) ??
                   throw new RpcException(new Status(StatusCode.FailedPrecondition, "period does not exist"));
        }
    }
}
