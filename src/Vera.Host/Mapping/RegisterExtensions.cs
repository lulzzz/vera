using System;

namespace Vera.Host.Mapping
{
    public static class RegisterExtensions
    {
        public static Vera.Models.Register Unpack(this Grpc.Shared.Register register)
        {
            if (register == null) return null;

            var result = new Vera.Models.Register
            {
                ClosingAmount = register.ClosingAmount,
                Id = Guid.Parse(register.Id),
                OpeningAmount = register.OpeningAmount
            };

            return result;
        }
    }
}