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
using Vera.Grpc.Shared;
using Vera.Host.Security;
using Vera.Stores;
using Vera.Thermal;

namespace Vera.Host.Services
{
    [Authorize]
    public class ReceiptService : Grpc.ReceiptService.ReceiptServiceBase
    {
        private readonly IAccountStore _accountStore;
        private readonly IInvoiceStore _invoiceStore;
        private readonly IAccountComponentFactoryCollection _accountComponentFactoryCollection;
        private readonly IPrintAuditTrailStore _printAuditTrailStore;

        public ReceiptService(
            IAccountStore accountStore,
            IInvoiceStore invoiceStore,
            IPrintAuditTrailStore printAuditTrailStore,
            IAccountComponentFactoryCollection accountComponentFactoryCollection
        )
        {
            _accountStore = accountStore;
            _invoiceStore = invoiceStore;
            _printAuditTrailStore = printAuditTrailStore;
            _accountComponentFactoryCollection = accountComponentFactoryCollection;
        }

        public override async Task<RenderThermalReply> RenderThermal(RenderThermalRequest request, ServerCallContext context)
        {
            var account = await context.ResolveAccount(_accountStore, request.AccountId);
            var invoice = await _invoiceStore.GetByNumber(account.Id, request.InvoiceNumber);

            if (invoice == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "invoice not found"));
            }

            await using var ms = new MemoryStream(8192);
            await using var sw = new StreamWriter(ms, Encoding.UTF8);

            IThermalVisitor visitor = request.Type switch
            {
                ReceiptOutputType.Json => new JsonThermalVisitor(new JsonTextWriter(sw)),
                ReceiptOutputType.Text => new TextThermalVisitor(sw),
                ReceiptOutputType.Esc => new EscPosVisitor(ms),
                _ => throw new ArgumentOutOfRangeException(nameof(request.Type), "unknown requested output type")
            };

            var componentFactory = _accountComponentFactoryCollection.GetComponentFactory(account);

            var receiptContextFactory = new ThermalReceiptContextFactory(_printAuditTrailStore);
            var generatorContext = await receiptContextFactory.Create(account, invoice);

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

            // TODO: also store "document" on the trail so we know exactly what should've been printed?
            // Ledger fact that a render has been requested
            var trail = await _printAuditTrailStore.Create(invoice.Id);

            return new RenderThermalReply
            {
                Token = $"{invoice.Id}:{trail.Id}",
                Type = request.Type,
                Content = await ByteString.FromStreamAsync(ms)
            };
        }

        public override async Task<Empty> UpdatePrintResult(UpdatePrintResultRequest request, ServerCallContext context)
        {
            var parts = request.Token.Split(':');

            if (parts.Length != 2)
            {
                // TODO(kevin): throw error
                return new Empty();
            }

            var invoiceId = Guid.Parse(parts[0]);
            var trailId = Guid.Parse(parts[1]);

            var trail = await _printAuditTrailStore.Get(invoiceId, trailId);
            trail.Success = request.Success;

            await _printAuditTrailStore.Update(trail);

            return new Empty();
        }
    }
}