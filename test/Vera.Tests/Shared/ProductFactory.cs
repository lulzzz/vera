using Bogus;
using Vera.Models;

namespace Vera.Tests.Shared
{
    public static class ProductFactory
    {
        private static readonly Faker Faker = new();
        
        public static Product CreateRandomProduct() => new()
        {
            SystemId = Faker.Random.Number(1, 9999).ToString(),
            Barcode = Faker.Commerce.Ean13(),
            Code = Faker.Random.AlphaNumeric(8).ToUpper(),
            Description = Faker.Commerce.ProductName(),
            Type = ProductType.Goods
        };

        public static Product CreateCocaCola() => new()
        {
            Code = "COCA",
            Description = "Coca Cola",
            Type = ProductType.Goods
        };
    }
}