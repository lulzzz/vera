using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Vera.Grpc;
using Vera.Grpc.Models;
using Vera.Grpc.Shared;
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

        public override async Task<CreateSupplierReply> Create(CreateSupplierRequest request, ServerCallContext context)
        {
            var grpcSupplier = request.Supplier;
            var existingSupplier = await _supplierStore.GetBySystemId(grpcSupplier.SystemId);
            if (existingSupplier != null)
            {
                throw new RpcException(new Status(
                    StatusCode.InvalidArgument,
                    $"Supplier with systemId '{existingSupplier.SystemId}' already exists"
                ));
            }

            var supplier = grpcSupplier.Unpack();

            await _supplierStore.Store(supplier);

            return new CreateSupplierReply { Id = supplier.Id.ToString() };
        }

        public override async Task<Grpc.Shared.Supplier> Get(GetSupplierRequest request, ServerCallContext context)
        {
            var supplier = await GetAndValidateSupplier(request.SystemId);

            return supplier.Pack();
        }

        public override async Task<Grpc.Shared.Supplier> Update(UpdateSupplierRequest request, ServerCallContext context)
        {
            var supplier = await GetAndValidateSupplier(request.SystemId);
            var requestSupplier = request.Supplier;

            supplier.Name = requestSupplier.Name;
            supplier.RegistrationNumber = requestSupplier.RegistrationNumber;
            supplier.TaxRegistrationNumber = requestSupplier.TaxRegistrationNumber;
            supplier.Address = requestSupplier.Address.Unpack();
            //systemId is not updated

            await _supplierStore.Update(supplier);

            return requestSupplier;
        }

        public override async Task<Empty> Delete(DeleteSupplierRequest request, ServerCallContext context)
        {
            var supplier = await GetAndValidateSupplier(request.SystemId);

            await _supplierStore.Delete(supplier);

            return new Empty();
        }

        private async Task<Models.Supplier> GetAndValidateSupplier(string supplierSystemId)
        {
            var supplier = await _supplierStore.GetBySystemId(supplierSystemId);

            if (supplier == null)
            {
                throw new RpcException(new Status(StatusCode.FailedPrecondition, "Supplier does not exist"));
            }

            return supplier;
        }
    }
}
