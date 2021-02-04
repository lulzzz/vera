using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Vera.Bootstrap;
using Vera.Concurrency;
using Vera.Grpc;
using Vera.Grpc.Models;
using Vera.Host.Security;
using Vera.Invoices;
using Vera.Stores;

namespace Vera.Host.Services
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
            var invoice = request.Invoice.Unpack();

            var factory = _accountComponentFactoryCollection.GetComponentFactory(account);

            var processor = new InvoiceProcessor(_invoiceStore, _locker, factory);
            await processor.Process(invoice);

            return new CreateInvoiceReply
            {
                Number = invoice.Number,
                Sequence = invoice.Sequence,
                Signature = new Grpc.Signature
                {
                    Input = ByteString.CopyFromUtf8(invoice.Signature.Input),
                    Output = ByteString.CopyFrom(invoice.Signature.Output)
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