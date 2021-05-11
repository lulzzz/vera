using Microsoft.Extensions.Logging;
using Moq;
using System.Text;
using System.Threading.Tasks;
using Vera.Models;
using Vera.Models.Portugal;
using Vera.Portugal.WorkingDocuments;
using Vera.Signing;
using Vera.Stores;
using Vera.Tests.Shared;
using Xunit;

namespace Vera.Portugal.Tests
{
    public class WorkingDocumentsHandlerTests
    {
        [Fact]
        public async Task Create_working_documents_handler()
        {
            var product = ProductFactory.CreateRandomProduct();
            product.Type = ProductType.GiftCard;

            var account = new Account();
            var supplier = new Supplier();
            var builder = new InvoiceBuilder();
            var invoice = builder
                .Reset()
                .WithAccount(account.Id)
                .WithRegister("1.1")
                .WithEmployee()
                .WithSupplier(supplier.SystemId)
                .WithProductLine(1, 1.99m, 1.23m, TaxesCategory.High, product)
                .WithPayment(PaymentCategory.Cash)
                .WithSignature(new Signature
                {
                    Input = "test",
                    Output = Encoding.ASCII.GetBytes("test"),
                    Version = 1
                })
                .Build();

            var wdStore = new Mock<IWorkingDocumentStore>();
            var chainStore = new Mock<IChainStore>();
            var last = new Mock<IChainable>();
            var signer = new Mock<IPackageSigner>();
            var logger = new Mock<ILogger<WorkingDocumentsHandler>>();

            last.SetupGet(x => x.NextSequence)
                .Returns(1);

            chainStore.Setup(x => x.Last(It.IsAny<ChainContext>()))
                .ReturnsAsync(last.Object);
                
            var handler = new WorkingDocumentsHandler(wdStore.Object, chainStore.Object, 
                    signer.Object, logger.Object);

            await handler.Handle(invoice);

            wdStore.Verify(w => w.Store(It.IsAny<WorkingDocument>()), Times.Exactly(1));
        }
    }
}
