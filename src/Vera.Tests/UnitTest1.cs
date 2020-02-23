using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Vera.Audit;
using Vera.Concurrency;
using Vera.Models;
using Vera.Signing;
using Vera.Stores;
using Xunit;
using Xunit.Abstractions;

namespace Vera.Tests
{
    public class UnitTest1
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public UnitTest1(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task Test1()
        {
            // Azure functions
            // gRPC call
            // ASP.NET MVC

            // Somehow get the factory for the country based on the configuration
            // for the current principal
            // var factory = new Mock<IComponentFactory>();

            // var store = new Mock<IInvoiceStore>();

            // var facade = new InvoiceFacade(
            //     store.Object,
            //     factory.Object.CreateLocker(),
            //     factory.Object.CreateInvoiceBucketGenerator(),
            //     factory.Object.CreateInvoiceNumberGenerator(),
            //     await factory.Object.CreatePackageSigner()
            // );

            // await facade.Process(new Invoice());

            const string cosmosConnectionString =
                "AccountEndpoint=https://cdb-dev-eva.documents.azure.com:443/;AccountKey=xb8nifoQ3WuFOzOeVkvpUENUXF9dc6oSVbH1AC5uxZ6j0OiNmRLbXv7AJjUFO44jHYWxaH5BdwKWSb3wjL2U6g==;";

            var storeTest = new CosmosStore(cosmosConnectionString, "EVA", "invoices");

            var invoice = new Invoice
            {
                StoreNumber = "1004",
                FiscalYear = 2019,
                FiscalPeriod = 2
            };

            var bucket = invoice.StoreNumber;
            
            var last = await storeTest.Last(invoice, bucket);
            await storeTest.Save(invoice, bucket);
            last = await storeTest.Last(invoice, bucket);
            
            Debugger.Break();

            // var locker = new AzureBlobLocker("DefaultEndpointsProtocol=https;AccountName=dev525;AccountKey=EuZbqMh6tNFHet6bMzjS6W9NK6VgoZ6MNFfe4EtyWaepypdx/8cVUtULI923Qqa85VAHnhq9+noy3nx/GBQupw==;EndpointSuffix=core.windows.net");
            //
            // var tasks = new List<Task>();
            //
            // Func<Task> DoStuff(int id)
            // {
            //     return async () =>
            //     {
            //         _testOutputHelper.WriteLine("getting lock.. " + id);
            //
            //         await using (await locker.Lock("store-1004", TimeSpan.FromSeconds(30)))
            //         {
            //             _testOutputHelper.WriteLine("got lock, doing work.. " + id);
            //
            //             await Task.Delay(150);
            //
            //             _testOutputHelper.WriteLine("done working, releasing.. " + id);
            //         }
            //
            //         _testOutputHelper.WriteLine("released lock " + id);
            //     };
            // }
            //
            // for (var i = 1; i <= 10; i++)
            // {
            //     tasks.Add(Task.Run(DoStuff(i)));
            // }
            //
            // await Task.WhenAll(tasks);

            // Somehow get the audit factory for the country
            // IAuditFactory<AuditPortugal> auditFactory = null;
            //
            // var transformer = await auditFactory.CreateAuditTransformer();
            // var archive = await auditFactory.CreateAuditArchive();
            //
            // var transformResult = await transformer.Transform(new Models.Audit());
            // await archive.Archive(new AuditCriteria(), transformResult);
            //
            // var principal = new GenericPrincipal(new GenericIdentity("kevin"), new[] { "admin" });
        }
    }
}
