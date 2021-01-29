using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using Vera.Bootstrap;
using Vera.Documents;
using Vera.Documents.Visitors;
using Vera.Grpc;
using Vera.Models;
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
                throw new RpcException(new Status(StatusCode.Unauthenticated, "unauthorized"));
            }

            var invoice = await _invoiceStore.GetByNumber(accountId, request.Number);

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

            var factory = _accountComponentFactoryCollection.GetOrThrow(account).CreateComponentFactory(account);
            var generator = factory.CreateThermalReceiptGenerator();
            var node = generator.Generate(new ThermalReceiptContext
            {
                Account = account,
                Invoice = invoice,

                // TODO(kevin): fill this correctly
                Totals = new Totals
                {
                    Amount = 0m,
                    AmountInTax = 0m,
                    Taxes = new List<TaxTotal>()
                }

                // TODO(kevin): map other properties
            });

            node.Accept(visitor);

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