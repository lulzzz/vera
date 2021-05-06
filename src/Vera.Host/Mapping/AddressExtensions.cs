using Vera.Grpc.Shared;

namespace Vera.Host.Mapping
{
    public static class AddressExtensions
    {
        public static Vera.Models.Address Unpack(this Address a)
        {
            if (a == null) return null;

            return new Vera.Models.Address
            {
                City = a.City,
                Country = a.Country,
                Number = a.Number,
                Region = a.Region,
                Street = a.Street,
                PostalCode = a.PostalCode
            };
        }

        public static Address Pack(this Vera.Models.Address a)
        {
            if (a == null) return null;

            return new Address
            {
                City = a.City,
                Country = a.Country,
                Number = a.Number,
                Region = a.Region,
                Street = a.Street,
                PostalCode = a.PostalCode
            };
        }
    }
}