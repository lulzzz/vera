using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using Vera.Bootstrap;
using Vera.Documents;
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
            var companyId = context.GetCompanyId();
            var accountId = Guid.Parse(request.Account);

            var account = await _accountStore.Get(companyId, accountId);

            if (account == null)
            {
                // Not allowed to create an invoice for this account because it does not belong to the company
                // to which the user has rights
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Unauthorized"));
            }

            await using var ms = new MemoryStream(8192);
            await using var sw = new StreamWriter(ms, Encoding.UTF8);

            var visitor = request.Type switch
            {
                ReceiptOutputType.Json => new JsonThermalVisitor(new JsonTextWriter(sw)),
                _ => throw new ArgumentOutOfRangeException(nameof(request.Type), "unknown requested output type")
            };

            var factory = _accountComponentFactoryCollection.GetOrThrow(account).CreateComponentFactory(account);
            var generator = factory.CreateThermalReceiptGenerator();
            var node = generator.Generate(new ThermalReceiptContext());

            node.Accept(visitor);

            await sw.FlushAsync();

            // TODO(kevin): mark as "printed" at this point? or separate endpoint to confirm printing?

            return new RenderThermalReply
            {
                Type = request.Type,
                Content = await ByteString.FromStreamAsync(ms)
            };
        }
    }
}