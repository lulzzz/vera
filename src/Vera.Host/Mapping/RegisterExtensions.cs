using System;
using Vera.Grpc.Shared;

namespace Vera.Host.Mapping
{
    public static class RegisterExtensions
    {
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

        public static RegisterStatus Pack(this Models.RegisterStatus registerStatus)
        {
            return registerStatus switch
            {
                Models.RegisterStatus.Pending => RegisterStatus.Pending,
                Models.RegisterStatus.Open => RegisterStatus.Open,
                Models.RegisterStatus.Closed => RegisterStatus.Closed,
                _ => throw new ArgumentOutOfRangeException(nameof(registerStatus))
            };
        }
    }
}
