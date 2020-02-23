using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Vera.Models;
using Vera.Signing;
using Vera.Stores;
using Xunit;

namespace Vera.Portugal.Tests
{
    public class PackageSignerTests
    {
        [Fact]
        public async Task Should_build_signature_in_correct_format()
        {
            var package = new Package
            {
                Timestamp = new DateTime(1999, 10, 20, 13, 31, 22),
                Number = "t123/1",
                Net = -123.2323m,
                Gross =- 123.2323m,
                PreviousSignature= "abcdefg"
            };

            const string expectedSignature = "1999-10-20;1999-10-20T13:31:22;t123/1;123.23;abcdefg";

            var signer = new PackageSigner(RSA.Create());
            var result = await signer.Sign(package);

            Assert.NotNull(result.Input);
            Assert.NotNull(result.Output);

            Assert.Equal(expectedSignature, result.Input);
            Assert.Equal(256, result.Output.Length);
        }

        [Fact]
        public async Task Should_work()
        {
            const string blobConnectionString =
                "DefaultEndpointsProtocol=https;AccountName=dev525;AccountKey=EuZbqMh6tNFHet6bMzjS6W9NK6VgoZ6MNFfe4EtyWaepypdx/8cVUtULI923Qqa85VAHnhq9+noy3nx/GBQupw==;EndpointSuffix=core.windows.net";
            const string cosmosConnectionString =
                "AccountEndpoint=https://cdb-dev-eva.documents.azure.com:443/;AccountKey=xb8nifoQ3WuFOzOeVkvpUENUXF9dc6oSVbH1AC5uxZ6j0OiNmRLbXv7AJjUFO44jHYWxaH5BdwKWSb3wjL2U6g==;";

            var storeTest = new CosmosStore(cosmosConnectionString, "EVA", "invoices");
            var factory = new ComponentFactory(blobConnectionString, RSA.Create());

            var facade = new InvoiceFacade(
                storeTest,
                factory.CreateLocker(),
                factory.CreateInvoiceBucketGenerator(),
                factory.CreateInvoiceNumberGenerator(),
                factory.CreatePackageSigner()
            );


            var sw = Stopwatch.StartNew();

            await facade.Process(new Invoice
            {
                FiscalYear = 2020,
                FiscalPeriod = 1,
                StoreNumber = "0001"
            });

            sw.Stop();

            var ms1 = sw.ElapsedMilliseconds;

            sw.Restart();

            await facade.Process(new Invoice
            {
                FiscalYear = 2020,
                FiscalPeriod = 1,
                StoreNumber = "0001"
            });

            sw.Stop();

            var ms2 = sw.ElapsedMilliseconds;

            sw.Restart();

            await facade.Process(new Invoice
            {
                FiscalYear = 2020,
                FiscalPeriod = 1,
                StoreNumber = "0001"
            });

            sw.Stop();

            var ms3 = sw.ElapsedMilliseconds;            

            Debugger.Break();
        }
    }
}
