using System;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Vera.Bootstrap;
using Vera.Concurrency;
using Vera.Grpc;
using Vera.Grpc.Models;
using Vera.Invoices;
using Vera.Models;
using Vera.Stores;
using Vera.WebApi.Security;

namespace Vera.WebApi.Services
{
    [Authorize]
    public class InvoiceService : Grpc.InvoiceService.InvoiceServiceBase
    {
        private readonly IAccountStore _accountStore;
        private readonly IInvoiceStore _invoiceStore;
        private readonly ILocker _locker;
        private readonly IAccountComponentFactoryCollection _accountComponentFactoryCollection;

        public InvoiceService(
            IAccountStore accountStore,
            IInvoiceStore invoiceStore,
            ILocker locker,
            IAccountComponentFactoryCollection accountComponentFactoryCollection
        )
        {
            _accountStore = accountStore;
            _invoiceStore = invoiceStore;
            _locker = locker;
            _accountComponentFactoryCollection = accountComponentFactoryCollection;
        }

        public override async Task<CreateInvoiceReply> Create(CreateInvoiceRequest request, ServerCallContext context)
        {
            var account = await context.ResolveAccount(_accountStore, request.Invoice.Account);

            // TODO: validate invoice, very, very, very strict
            // TODO(kevin): PT - invoices > 1000 euros require a customer
            // TODO(kevin): NF525 - requires signature of original invoice on the returned line

            var factory = _accountComponentFactoryCollection.GetComponentFactory(account);

            var processor = new InvoiceProcessor(_invoiceStore, _locker, factory);

            var result = await processor.Process(request.Invoice.Unpack());

            return new CreateInvoiceReply
            {
                Number = result.Number,
                Sequence = result.Sequence,
                Signature = new Grpc.Signature
                {
                    Input = ByteString.CopyFromUtf8(result.Input),
                    Output = ByteString.CopyFrom(result.Output)
                }
            };
        }

        public override async Task<GetInvoiceReply> GetByNumber(GetInvoiceByNumberRequest request, ServerCallContext context)
        {
            var account = await context.ResolveAccount(_accountStore, request.AccountId);
            var invoice = await _invoiceStore.GetByNumber(account.Id, request.Number);

            return new GetInvoiceReply
            {
                Number = invoice.Number
            };
        }
    }
}