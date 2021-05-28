using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Vera.Grpc;
using Vera.Grpc.Shared;
using Vera.Host.Mapping;
using Vera.Host.Security;
using Vera.Stores;

namespace Vera.Host.Services
{
    [Authorize]
    public class SupplierService : Grpc.SupplierService.SupplierServiceBase
    {
        private readonly ISupplierStore _supplierStore;

        public SupplierService(ISupplierStore supplierStore)
        {
            _supplierStore = supplierStore;
        }

        public override async Task<CreateSupplierReply> CreateIfNotExists(CreateSupplierRequest request, ServerCallContext context)
        {
            var supplier = await _supplierStore.Get(context.GetAccountId(), request.Supplier.SystemId);
            
            if (supplier == null)
            {
                supplier = request.Supplier.Unpack();
                supplier.AccountId = context.GetAccountId();

                await _supplierStore.Store(supplier);
            }

            return new CreateSupplierReply
            {
                Id = supplier.Id.ToString()
            };
        }

        public override async Task<CreateSupplierReply> Create(CreateSupplierRequest request, ServerCallContext context)
        {
            var existingSupplier = await _supplierStore.Get(context.GetAccountId(), request.Supplier.SystemId);
            if (existingSupplier != null)
            {
                throw new RpcException(new Status(
                    StatusCode.InvalidArgument,
                    $"Supplier with systemId '{existingSupplier.SystemId}' already exists"
                ));
            }

            var supplier = request.Supplier.Unpack();
            supplier.AccountId = context.GetAccountId();

            await _supplierStore.Store(supplier);

            return new CreateSupplierReply { Id = supplier.Id.ToString() };
        }

        public override async Task<Supplier> Get(GetSupplierRequest request, ServerCallContext context)
        {
            var supplier = await GetAndValidateSupplier(request.SystemId, context);

            return supplier.Pack();
        }

        public override async Task<Supplier> Update(UpdateSupplierRequest request, ServerCallContext context)
        {
            var supplier = await GetAndValidateSupplier(request.SystemId, context);
            var requestSupplier = request.Supplier;

            supplier.Name = requestSupplier.Name;
            supplier.RegistrationNumber = requestSupplier.RegistrationNumber;
            supplier.TaxRegistrationNumber = requestSupplier.TaxRegistrationNumber;
            supplier.Address = requestSupplier.Address.Unpack();
            supplier.TimeZone = requestSupplier.TimeZone;
            // systemId is not updated

            await _supplierStore.Update(supplier);

            return requestSupplier;
        }

        public override async Task<Empty> Delete(DeleteSupplierRequest request, ServerCallContext context)
        {
            var supplier = await GetAndValidateSupplier(request.SystemId, context);

            await _supplierStore.Delete(supplier);

            return new Empty();
        }

        private async Task<Models.Supplier> GetAndValidateSupplier(string supplierSystemId, ServerCallContext context)
        {
            var supplier = await _supplierStore.Get(context.GetAccountId(), supplierSystemId);
            if (supplier == null)
            {
                throw new RpcException(new Status(StatusCode.FailedPrecondition, "Supplier does not exist"));
            }

            var accountId = context.GetAccountId();
            if (supplier.AccountId != accountId)
            {
                throw new RpcException(new Status(StatusCode.PermissionDenied, "Supplier cannot be accessed by this account"));
            }

            return supplier;
        }
    }
}
