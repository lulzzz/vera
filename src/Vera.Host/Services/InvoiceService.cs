using System;
using System.Diagnostics;
using System.Linq;
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
        private readonly IInvoiceProcessor _invoiceProcessor;
        private readonly IAccountComponentFactoryCollection _accountComponentFactoryCollection;

        public InvoiceService(
            IAccountStore accountStore,
            IInvoiceStore invoiceStore,
            IInvoiceProcessor invoiceProcessor,
            IAccountComponentFactoryCollection accountComponentFactoryCollection
        )
        {
            _accountStore = accountStore;
            _invoiceStore = invoiceStore;
            _invoiceProcessor = invoiceProcessor;
            _accountComponentFactoryCollection = accountComponentFactoryCollection;
        }

        public override async Task<CreateInvoiceReply> Create(CreateInvoiceRequest request, ServerCallContext context)
        {
            var account = await context.ResolveAccount(_accountStore, request.Invoice.Account);

            // TODO: validate invoice, very, very, very strict
            // TODO(kevin): NF525 - requires signature of original invoice on the returned line
            
            var factory = _accountComponentFactoryCollection.GetComponentFactory(account);
            var invoice = request.Invoice.Unpack();
            
            await _invoiceProcessor.Process(factory, invoice);

            return new CreateInvoiceReply
            {
                Number = invoice.Number,
                Sequence = invoice.Sequence,
                Signature = new Signature
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

        public override async Task<ValidateInvoiceReply> Validate(ValidateInvoiceRequest request, ServerCallContext context)
        {
            var account = await context.ResolveAccount(_accountStore, request.AccountId);
            var factory = _accountComponentFactoryCollection.GetComponentFactory(account);
            var invoice = request.Invoice.Unpack();

            var results = factory.CreateInvoiceValidator().Validate(invoice);

            var reply = new ValidateInvoiceReply();
            foreach (var result in results)
            {
                reply.Results[result.MemberNames.First()] = result.ErrorMessage;
            }

            return reply;
        }
    }
}