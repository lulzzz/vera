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

namespace Vera.Host.Services
{
    [Authorize]
    public class RegisterService : Grpc.RegisterService.RegisterServiceBase
    {
        private readonly IPeriodStore _periodStore;
        private readonly ISupplierStore _supplierStore;
        private readonly IRegisterStore _registerStore;
        private readonly IAccountStore _accountStore;
        private readonly IAccountComponentFactoryCollection _accountComponentFactoryCollection;

        public RegisterService(IPeriodStore periodStore,
            ISupplierStore supplierStore,
            IRegisterStore registerStore,
            IAccountComponentFactoryCollection accountComponentFactoryCollection,
            IAccountStore accountStore)
        {
            _periodStore = periodStore;
            _supplierStore = supplierStore;
            _registerStore = registerStore;
            _accountComponentFactoryCollection = accountComponentFactoryCollection;
            _accountStore = accountStore;
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

            var register = await _registerStore.Get(Guid.Parse(request.RegisterId), supplier.Id);
            if (register == null)
            {
                throw new RpcException(new Status(StatusCode.FailedPrecondition, "Register does not exist"));
            }

            var registerEntry = new Models.PeriodRegisterEntry
            {
                OpeningAmount = request.OpeningAmount,
                RegisterId = register.Id
            };
            
            period.Registers.Add(registerEntry);

            await _periodStore.Update(period);

            return new OpenRegisterReply
            {
                Id = registerEntry.RegisterId.ToString()
            };
        }

        public override async Task<CloseRegisterReply> CloseRegister(CloseRegisterRequest request, ServerCallContext context)
        {
            var supplier = await _supplierStore.Get(context.GetAccountId(), request.SupplierSystemId);
            if (supplier == null)
            {
                throw new RpcException(new Status(StatusCode.FailedPrecondition, "Supplier does not exist"));
            }

            var register = await _registerStore.GetBySystemIdAndSupplierId(request.SystemId, supplier.Id);
            if (register == null)
            {
                throw new RpcException(new Status(StatusCode.FailedPrecondition, "Register does not exist"));
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
            var supplier = await _supplierStore.Get(context.GetAccountId(), request.SupplierSystemId);
            if (supplier == null)
            {
                throw new RpcException(new Status(StatusCode.FailedPrecondition, "Supplier does not exist"));
            }

            var newRegister = new Register
            {
                Name = request.Name,
                SupplierId = supplier.Id,
                SystemId = request.SystemId,
                Status = RegisterStatus.Pending,
            };

            var account = await context.ResolveAccount(_accountStore);
            
            var factory = _accountComponentFactoryCollection.GetComponentFactory(account);
            await factory.CreateRegisterInitializer().Initialize(newRegister);

            await _registerStore.Store(newRegister);

            return new CreateRegisterReply
            {
                Id = newRegister.Id.ToString()
            };
        }

        public override async Task<GetRegisterReply> Get(GetRegisterRequest request, ServerCallContext context)
        {
            var supplier = await _supplierStore.Get(context.GetAccountId(), request.SupplierSystemId);
            if (supplier == null)
            {
                throw new RpcException(new Status(StatusCode.FailedPrecondition, "Supplier does not exist"));
            }

            var register = await _registerStore.Get(Guid.Parse(request.Id), supplier.Id);
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
            var supplier = await _supplierStore.Get(context.GetAccountId(), request.SupplierSystemId);
            if (supplier == null)
            {
                throw new RpcException(new Status(StatusCode.FailedPrecondition, "Supplier does not exist"));
            }

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
