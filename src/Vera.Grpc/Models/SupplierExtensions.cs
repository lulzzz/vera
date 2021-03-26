namespace Vera.Grpc.Models
{
    public static class SupplierExtensions
    {
        public static Shared.Supplier Pack(this Vera.Models.Supplier supplier)
        {
            var result = new Shared.Supplier
            {
                Name = supplier.Name,
                RegistrationNumber = supplier.RegistrationNumber ?? string.Empty,
                TaxRegistrationNumber = supplier.TaxRegistrationNumber ?? string.Empty,
                SystemId = supplier.SystemId,
                Address = supplier.Address.Pack()
            };

            return result;
        }

        public static Vera.Models.Supplier Unpack(this Shared.Supplier supplier)
        {
            var result = new Vera.Models.Supplier
            {
                Name = supplier.Name,
                RegistrationNumber = supplier.RegistrationNumber ?? string.Empty,
                TaxRegistrationNumber = supplier.TaxRegistrationNumber ?? string.Empty,
                SystemId = supplier.SystemId,
                Address = supplier.Address.Unpack()
            };

            return result;
        }
    }
}
