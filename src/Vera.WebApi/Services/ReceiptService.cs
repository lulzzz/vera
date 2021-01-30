using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using Vera.Bootstrap;
using Vera.Documents.Visitors;
using Vera.Grpc;
using Vera.Stores;
using Vera.Thermal;
using Vera.WebApi.Security;

namespace Vera.WebApi.Services
{
    [Authorize]
    public class ReceiptService : Grpc.ReceiptService.ReceiptServiceBase
    {
        private readonly IAccountStore _accountStore;
        private readonly IInvoiceStore _invoiceStore;
        private readonly IAccountComponentFactoryCollection _accountComponentFactoryCollection;

        public ReceiptService(
            IAccountStore accountStore,
            IInvoiceStore invoiceStore,
            IAccountComponentFactoryCollection accountComponentFactoryCollection
        )
        {
            _accountStore = accountStore;
            _invoiceStore = invoiceStore;
            _accountComponentFactoryCollection = accountComponentFactoryCollection;
        }

        public override async Task<RenderThermalReply> RenderThermal(RenderThermalRequest request, ServerCallContext context)
        {
            var account = await context.ResolveAccount(_accountStore, request.Account);
            var invoice = await _invoiceStore.GetByNumber(account.Id, request.Number);

            if (invoice == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "invoice not found"));
            }

            await using var ms = new MemoryStream(8192);
            await using var sw = new StreamWriter(ms, Encoding.UTF8);

            IThermalVisitor visitor = request.Type switch
            {
                ReceiptOutputType.Json => new JsonThermalVisitor(new JsonTextWriter(sw)),
                ReceiptOutputType.Text => new StringThermalVisitor(sw),
                ReceiptOutputType.Esc => new EscPosVisitor(ms),
                _ => throw new ArgumentOutOfRangeException(nameof(request.Type), "unknown requested output type")
            };

            var componentFactory = _accountComponentFactoryCollection.GetComponentFactory(account);

            var receiptContextFactory = new ThermalReceiptContextFactory();
            var generatorContext = receiptContextFactory.Create(account, invoice);

            var generator = componentFactory.CreateThermalReceiptGenerator();
            var node = generator.Generate(generatorContext);

            node.Accept(visitor);

            // TODO(kevin): find a nicer way to flush
            switch (request.Type)
            {
                case ReceiptOutputType.Json:
                case ReceiptOutputType.Text:
                    await sw.FlushAsync();
                    break;
                case ReceiptOutputType.Esc:
                    await ms.FlushAsync();
                    break;
            }

            ms.Position = 0;

            // TODO(kevin): mark as "printed" at this point? or separate endpoint to confirm printing?

            return new RenderThermalReply
            {
                Type = request.Type,
                Content = await ByteString.FromStreamAsync(ms)
            };
        }
    }
}