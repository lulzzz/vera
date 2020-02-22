using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Vera.Audit;
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
            var factory = new Mock<IComponentFactory>();

            var store = new Mock<IInvoiceStore>();

            var facade = new InvoiceFacade(
                store.Object,
                factory.Object.CreateLocker(),
                factory.Object.CreateInvoiceSequenceGenerator(),
                factory.Object.CreateInvoiceNumberGenerator(),
                await factory.Object.CreatePackageSigner()
            );

            // await facade.Process(new Invoice());

            // var locker = new AzureBlobInvoiceLocker("DefaultEndpointsProtocol=https;AccountName=dev525;AccountKey=EuZbqMh6tNFHet6bMzjS6W9NK6VgoZ6MNFfe4EtyWaepypdx/8cVUtULI923Qqa85VAHnhq9+noy3nx/GBQupw==;EndpointSuffix=core.windows.net");
            //
            // var tasks = new List<Task>();
            //
            // Func<Task> DoStuff(int id)
            // {
            //     return async () =>
            //     {
            //         _testOutputHelper.WriteLine("getting lock.. " + id);
            //
            //         await using (await locker.Lock("store-1003", TimeSpan.FromSeconds(1)))
            //         {
            //             _testOutputHelper.WriteLine("got lock, doing work.. " + id);
            //
            //             await Task.Delay(500);
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
