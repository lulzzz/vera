﻿using Grpc.Core;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Vera.Concurrency;
using Vera.Dependencies;
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
            // TODO(kevin): check supplier is valid for account
            
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
            // TODO(kevin): check supplier is valid for account
            
            var period = await GetAndValidate(request.Id, request.SupplierSystemId);

            await _periodStore.Update(new Models.Period
            {
                Id = period.Id,
                Supplier = period.Supplier,
                Opening = period.Opening,
                Closing = _dateProvider.Now
            });

            return new Empty();
        }

        public override async Task<Period> Get(GetPeriodRequest request, ServerCallContext context)
        {
            // TODO(kevin): check supplier is valid for account
            
            var period = await GetAndValidate(request.Id, request.SupplierSystemId);

            return period.Pack();
        }

        public override async Task<Period> GetCurrentPeriod(GetCurrentPeriodRequest request, ServerCallContext context)
        {
            // TODO(kevin): check supplier is valid for account
            
            var supplier = await _supplierStore.GetBySystemId(request.SupplierSystemId);
            if (supplier == null)
            {
                throw new RpcException(new Status(StatusCode.FailedPrecondition, "Supplier does not exist"));
            }

            var period = await _periodStore.GetOpenPeriodForSupplier(supplier.Id);

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
