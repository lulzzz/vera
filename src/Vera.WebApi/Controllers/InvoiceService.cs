using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Vera.Bootstrap;
using Vera.Grpc;
using Vera.Invoices;
using Vera.Stores;

namespace Vera.WebApi.Controllers
{
    public class InvoiceService : Grpc.InvoiceService.InvoiceServiceBase
    {
        private readonly ICompanyStore _companyStore;
        private readonly IInvoiceStore _invoiceStore;
        private readonly IComponentFactoryCollection _componentFactoryCollection;

        public InvoiceService(
            ICompanyStore companyStore,
            IInvoiceStore invoiceStore,
            IComponentFactoryCollection componentFactoryCollection
        )
        {
            _companyStore = companyStore;
            _invoiceStore = invoiceStore;
            _componentFactoryCollection = componentFactoryCollection;
        }

        public override async Task<CreateInvoiceReply> Create(CreateInvoiceRequest request, ServerCallContext context)
        {
            var principal = context.GetHttpContext().User;
            var company = await _companyStore.GetByName(principal.FindFirstValue(Security.ClaimTypes.CompanyName));
            var account = company.Accounts.FirstOrDefault(a => a.Id == Guid.Parse(request.Invoice.Account));

            if (account == null)
            {
                // Not allowed to create an invoice for this account because it does not belong to the company
                // to which the user has rights
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Unauthorized"));
            }

            // TODO: validate invoice, very, very, very strict

            // TODO: think about this structure, does it make sense?
            // collection gets one of the resolvers for the account
            // resolver, resolves (no async support) by getting all the right stuff
            // to be able to return the components needed

            var factory = _componentFactoryCollection.Get(account);

            var facade = new InvoiceFacade(
                _invoiceStore,
                factory.CreateLocker(),
                factory.CreateInvoiceBucketGenerator(),
                factory.CreateInvoiceNumberGenerator(),
                factory.CreatePackageSigner()
            );

            var result = await facade.Process(Map(request.Invoice));

            return new CreateInvoiceReply
            {
                Number = result.Number,
                Sequence = result.Sequence,
                Signature = new Signature
                {
                    Input = ByteString.CopyFromUtf8(result.RawSignature),
                    Output = ByteString.CopyFrom(result.Signature)
                }
            };
        }

        private static Vera.Models.Invoice Map(Invoice invoice)
        {
            var result = new Vera.Models.Invoice();
            result.SystemId = invoice.SystemId;
            result.Date = invoice.Timestamp.ToDateTime();

            // TODO: map all the fields

            return result;
        }
    }
}