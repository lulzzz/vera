using System;

namespace Vera.Grpc.Models
{
    public static class RegisterExtensions
    {
        public static Vera.Models.Register Unpack(this Shared.Register register)
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