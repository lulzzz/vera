using Grpc.Core;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Vera.Concurrency;
using Vera.Dependencies;
using Vera.Grpc;
using Vera.Grpc.Models;
using Vera.Grpc.Shared;
using Vera.Stores;
using Vera.Host.Security;
using System.Linq;

namespace Vera.Host.Services
{
    [Authorize]
    public class PeriodService : Grpc.PeriodService.PeriodServiceBase
    {
        private readonly ISupplierStore _supplierStore;
        private readonly IPeriodStore _periodStore;
        private readonly IDateProvider _dateProvider;
        private readonly ILocker _locker;

        public PeriodService(
            ISupplierStore supplierStore,
            IPeriodStore periodStore,
            IDateProvider dateProvider,
            ILocker locker
        )
        {
            _supplierStore = supplierStore;
            _periodStore = periodStore;
            _dateProvider = dateProvider;
            _locker = locker;
        }

        public override async Task<OpenPeriodReply> OpenPeriod(OpenPeriodRequest request, ServerCallContext context)
        {
            var supplier = await _supplierStore.Get(context.GetAccountId(), request.SupplierSystemId);

            if (supplier == null)
            {
                throw new RpcException(new Status(StatusCode.FailedPrecondition, "Supplier does not exist"));
            }

            await using (await _locker.Lock(supplier.Id.ToString(), TimeSpan.FromSeconds(5)))
            {
                var currentPeriod = await _periodStore.GetOpenPeriodForSupplier(supplier.Id);
                if (currentPeriod != null)
                {
                    throw new RpcException(new Status(StatusCode.AlreadyExists, "Open period already exists"));
                }

                var period = new Models.Period
                {
                    Opening = _dateProvider.Now,
                    Supplier = supplier
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
            var period = await GetAndValidate(new PeriodValidationModel
            {
                AccountId = context.GetAccountId(),
                SupplierSystemId = request.SupplierSystemId,
                PeriodId = request.Id
            });

            async Task<Empty> Update()
            {
                period.Closing = _dateProvider.Now;

                await _periodStore.Update(period);

                return new Empty();
            }

            var registers = period.Registers;
            var registersToClose = request.Registers?.Count ?? 0;
            var registersOpened = registers.Count;

            if (registersOpened == 0)
            {
                return await Update();
            }

            if (registersOpened != registersToClose)
            {
                throw new RpcException(new Status(StatusCode.FailedPrecondition, "Missing one or more registers in the closing"));
            }

            var closingRegistersIds = request.Registers.ToDictionary(r => r.Id);
            foreach (var register in registers)
            {
                if (!closingRegistersIds.TryGetValue(register.Id.ToString(), out var closingRegister))
                {
                    throw new RpcException(new Status(StatusCode.FailedPrecondition, $"Unkown register {register.Id}"));
                }

                register.ClosingAmount = closingRegister.ClosingAmount;
            }

            return await Update();
        }

        public override async Task<Period> Get(GetPeriodRequest request, ServerCallContext context)
        {
            var period = await GetAndValidate(new PeriodValidationModel
            {
                AccountId = context.GetAccountId(),
                SupplierSystemId = request.SupplierSystemId,
                PeriodId = request.Id
            });

            return period.Pack();
        }

        public override async Task<Period> GetCurrentPeriod(GetCurrentPeriodRequest request, ServerCallContext context)
        {
            var supplier = await _supplierStore.Get(context.GetAccountId(), request.SupplierSystemId);
            if (supplier == null)
            {
                throw new RpcException(new Status(StatusCode.FailedPrecondition, "Supplier does not exist"));
            }

            var period = await _periodStore.GetOpenPeriodForSupplier(supplier.Id); 
            if (period == null)
            {
                throw new RpcException(new Status(StatusCode.FailedPrecondition, "Period does not exist"));
            }

            return period.Pack();
        }

        private async Task<Models.Period> GetAndValidate(PeriodValidationModel model)
        {
            var supplier = await _supplierStore.Get(model.AccountId, model.SupplierSystemId);
            if (supplier == null)
            {
                throw new RpcException(new Status(StatusCode.FailedPrecondition, "Supplier does not exist"));
            }

            var period = await _periodStore.Get(Guid.Parse(model.PeriodId), supplier.Id);
            if (period == null)
            {
                throw new RpcException(new Status(StatusCode.FailedPrecondition, "Period does not exist"));
            }

            return period;
        }

        public class PeriodValidationModel
        {
            public Guid AccountId { get; set; }
            public string SupplierSystemId { get; set; }
            public string PeriodId { get; set; }
        }
    }
}