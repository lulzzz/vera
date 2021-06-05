using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;
using Vera.Bootstrap;
using Vera.Grpc;
using Vera.Host.Mapping;
using Vera.Host.Security;
using Vera.Models;
using Vera.Stores;
using Register = Vera.Models.Register;
using RegisterStatus = Vera.Models.RegisterStatus;

namespace Vera.Host.Services
{
    [Authorize]
    public class RegisterService : Grpc.RegisterService.RegisterServiceBase
    {
        private readonly ISupplierStore _supplierStore;
        private readonly IRegisterStore _registerStore;
        private readonly IAccountStore _accountStore;
        private readonly IAccountComponentFactoryCollection _accountComponentFactoryCollection;

        public RegisterService(
            ISupplierStore supplierStore,
            IRegisterStore registerStore,
            IAccountStore accountStore,
            IAccountComponentFactoryCollection accountComponentFactoryCollection
        )
        {
            _supplierStore = supplierStore;
            _registerStore = registerStore;
            _accountStore = accountStore;
            _accountComponentFactoryCollection = accountComponentFactoryCollection;
        }

        public override async Task<CloseRegisterReply> CloseRegister(CloseRegisterRequest request, ServerCallContext context)
        {
            var supplier = await context.ResolveSupplier(_supplierStore, request.SupplierSystemId);

            var register = await _registerStore.GetBySystemIdAndSupplierId(supplier.Id, request.SystemId);
            if (register == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Register does not exist"));
            }

            var account = await context.ResolveAccount(_accountStore);

            var factory = _accountComponentFactoryCollection.GetComponentFactory(account);
            await factory.CreateRegisterCloser().Close(register);

            await _registerStore.Update(register);

            return new CloseRegisterReply
            {
                Id = register.Id.ToString()
            };
        }

        public override async Task<CreateRegisterReply> CreateRegister(CreateRegisterRequest request, ServerCallContext context)
        {
            var supplier = await context.ResolveSupplier(_supplierStore, request.SupplierSystemId);

            var newRegister = new Register
            {
                Name = request.Name,
                SupplierId = supplier.Id,
                SystemId = request.SystemId,
                Status = RegisterStatus.Pending,
            };

            var account = await context.ResolveAccount(_accountStore);

            var factory = _accountComponentFactoryCollection.GetComponentFactory(account);

            var registerInitializationContext = new RegisterInitializationContext(account, supplier, newRegister);

            await factory.CreateRegisterInitializer().Initialize(registerInitializationContext);

            await _registerStore.Store(newRegister);

            return new CreateRegisterReply
            {
                Id = newRegister.Id.ToString()
            };
        }

        public override async Task<GetRegisterReply> Get(GetRegisterRequest request, ServerCallContext context)
        {
            var supplier = await context.ResolveSupplier(_supplierStore, request.SupplierSystemId);

            var register = await _registerStore.Get(supplier.Id, Guid.Parse(request.Id));
            if (register == null)
            {
                return new GetRegisterReply();
            }

            return new GetRegisterReply
            {
                Register = register.Pack()
            };
        }

        public override async Task<GetAllRegistersReply> GetAll(GetAllRegistersRequest request, ServerCallContext context)
        {
            var supplier = await context.ResolveSupplier(_supplierStore, request.SupplierSystemId);

            var registers = await _registerStore.GetOpenRegistersForSupplier(supplier.Id);

            var reply = new GetAllRegistersReply();

            foreach (var register in registers)
            {
                reply.Registers.Add(register.Pack());
            }

            return reply;
        }
    }
}
