using System;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Threading.Tasks;
using Moq;
using Vera.Audit;
using Vera.Models;
using Vera.Signing;
using Vera.Stores;
using Xunit;

namespace Vera.Tests
{
    public class UnitTest1
    {
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
            var facade = new InvoiceFacade(store.Object, factory.Object);

            await facade.Process(new Invoice());

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
