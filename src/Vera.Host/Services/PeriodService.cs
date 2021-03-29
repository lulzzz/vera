using Grpc.Core;
using System;
using System.Threading.Tasks;
using Vera.Concurrency;
using Vera.Grpc;
using Vera.Grpc.Models;
using Vera.Grpc.Shared;
using Vera.Stores;

namespace Vera.Host.Services
{
    public class PeriodService : Grpc.PeriodService.PeriodServiceBase
    {
        private readonly ISupplierStore _supplierStore;
        private readonly IPeriodStore _periodStore;
        private readonly ILocker _locker;

        public PeriodService(ISupplierStore supplierStore, IPeriodStore periodStore, ILocker locker)
        {
            _supplierStore = supplierStore;
            _periodStore = periodStore;
            _locker = locker;
        }

        public override async Task<OpenPeriodReply> OpenPeriod(OpenPeriodRequest request, ServerCallContext context)
        {
            var supplier = await _supplierStore.GetBySystemId(request.SupplierSystemId);
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
                    Opening = DateTime.UtcNow,
                    Supplier = supplier
                };
                await _periodStore.Store(period);

                return new OpenPeriodReply { Id = period.Id.ToString() };
            }
        }

        public override async Task<Empty> ClosePeriod(ClosePeriodRequest request, ServerCallContext context)
        {
            var period = await GetAndValidate(request.Id, request.SupplierSystemId);

            await _periodStore.Update(new Models.Period
            {
                Id = period.Id,
                Supplier = period.Supplier,
                Opening = period.Opening,
                Closing = DateTime.UtcNow,
            });

            return new Empty();
        }

        public override async Task<Period> Get(GetPeriodRequest request, ServerCallContext context)
        {
            var period = await GetAndValidate(request.Id, request.SupplierSystemId);

            return period.Pack();
        }

        private async Task<Models.Period> GetAndValidate(string periodId, string supplierSystemId)
        {
            var supplier = await _supplierStore.GetBySystemId(supplierSystemId);
            if (supplier == null)
            {
                throw new RpcException(new Status(StatusCode.FailedPrecondition, "Supplier does not exist"));
            }

            var period = await _periodStore.Get(Guid.Parse(periodId), supplier.Id);
            if (period == null)
            {
                throw new RpcException(new Status(StatusCode.FailedPrecondition, "Period does not exist"));
            }

            return period;
        }
    }
}
