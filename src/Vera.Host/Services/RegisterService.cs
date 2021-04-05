using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;
using Vera.Grpc;
using Vera.Host.Security;
using Vera.Stores;

namespace Vera.Host.Services
{
    [Authorize]
    public class RegisterService : Grpc.RegisterService.RegisterServiceBase
    {
        private readonly IPeriodStore _periodStore;
        private readonly ISupplierStore _supplierStore;

        public RegisterService(IPeriodStore periodStore, ISupplierStore supplierStore)
        {
            _periodStore = periodStore;
            _supplierStore = supplierStore;
        }

        public override async Task<OpenRegisterReply> OpenRegister(OpenRegisterRequest request, ServerCallContext context)
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

            var register = new Models.Register
            {
                OpeningAmount = request.OpeningAmount
            };
            period.Registers.Add(register);

            await _periodStore.Update(period);

            return new OpenRegisterReply { Id = register.Id.ToString() };
        }
    }
}
