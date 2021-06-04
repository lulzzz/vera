namespace Vera.Host.Mapping
{
    public static class SupplierExtensions
    {
        public static Grpc.Shared.Supplier Pack(this Vera.Models.Supplier supplier)
        {
            var result = new Grpc.Shared.Supplier
            {
                Name = supplier.Name,
                RegistrationNumber = supplier.RegistrationNumber ?? string.Empty,
                TaxRegistrationNumber = supplier.TaxRegistrationNumber ?? string.Empty,
                SystemId = supplier.SystemId,
                Address = supplier.Address.Pack(),
                TimeZone = supplier.TimeZone,
                EmailAddress = supplier.EmailAddress
            };

            return result;
        }

        public static Vera.Models.Supplier Unpack(this Grpc.Shared.Supplier supplier)
        {
            var result = new Vera.Models.Supplier
            {
                Name = supplier.Name,
                RegistrationNumber = supplier.RegistrationNumber ?? string.Empty,
                TaxRegistrationNumber = supplier.TaxRegistrationNumber ?? string.Empty,
                SystemId = supplier.SystemId,
                Address = supplier.Address.Unpack(),
                TimeZone = supplier.TimeZone
            };

            return result;
        }
    }
}
