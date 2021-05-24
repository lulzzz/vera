using System;
using Vera.Grpc;
using Vera.Grpc.Shared;

namespace Vera.Host.Mapping
{
    public static class RegisterExtensions
    {
        public static Models.Register Unpack(this Register register)
        {
            if (register == null) return null;

            var result = new Models.Register
            {
                Id = Guid.Parse(register.Id),
                Name = register.Name,
                Status = register.Status.Unpack(),
                SupplierId = Guid.Parse(register.SupplierId),
                SystemId = register.SystemId,
                Data = register.Data
            };

            return result;
        }

        public static Models.Register Unpack(this CreateRegisterRequest register, Guid supplierId)
        {
            if (register == null) return null;

            var result = new Models.Register
            {
                Name = register.Name,
                SupplierId = supplierId,
                SystemId = register.SystemId,
                Status = Models.RegisterStatus.Pending,
            };

            return result;
        }

        public static Register Pack(this Models.Register register)
        {
            var result = new Register
            {
                Id = register.Id.ToString(),
                Name = register.Name,
                Status = register.Status.Pack(),
                SupplierId = register.SupplierId.ToString(),
                SystemId = register.SystemId,
            };

            if (register.Data != null)
            {
                result.Data.Add(register.Data);
            }

            return result;
        }

        public static Models.RegisterStatus Unpack(this RegisterStatus registerStatus)
        {
            return registerStatus switch
            {
                RegisterStatus.Pending => Models.RegisterStatus.Pending,
                RegisterStatus.Open => Models.RegisterStatus.Open,
                RegisterStatus.Closed => Models.RegisterStatus.Closed,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public static RegisterStatus Pack(this Models.RegisterStatus registerStatus)
        {
            return registerStatus switch
            {
                Models.RegisterStatus.Pending => RegisterStatus.Pending,
                Models.RegisterStatus.Open => RegisterStatus.Open,
                Models.RegisterStatus.Closed => RegisterStatus.Closed,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}