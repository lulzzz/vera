using System;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Vera.Bootstrap;
using Vera.Grpc;
using Vera.Host.Mapping;
using Vera.Host.Security;
using Vera.Printing;
using Vera.Stores;
using ClientAction = Vera.Printing.ClientAction;

namespace Vera.Host.Services
{
    [Authorize]
    public class PrinterService: Grpc.PrinterService.PrinterServiceBase
    {
        private readonly IAccountStore _accountStore;
        private readonly IInvoiceStore _invoiceStore;
        private readonly IAccountComponentFactoryCollection _accountComponentFactoryCollection;

        public PrinterService(
            IAccountStore accountStore,
            IInvoiceStore invoiceStore,
            IAccountComponentFactoryCollection accountComponentFactoryCollection)
        {
            _accountStore = accountStore;
            _invoiceStore = invoiceStore;
            _accountComponentFactoryCollection = accountComponentFactoryCollection;
        }
        
        public override async Task Print(IAsyncStreamReader<PrintRequest> requestStream, IServerStreamWriter<PrintResponse> responseStream, ServerCallContext context)
        {
            var request = await GetNextClientReply(requestStream);
            var account = await context.ResolveAccount(_accountStore);
            var invoice = await GetInvoice(account.Id, request.InvoiceNumber);
            
            var thermalInvoicePrintActionFactory = _accountComponentFactoryCollection
                .GetComponentFactory(account)
                .CreateThermalInvoicePrintActionFactory();
           
            var action = thermalInvoicePrintActionFactory.Create(account, invoice);
            
            await Execute(requestStream, responseStream, action);
        }
        
        private static async Task Execute(IAsyncStreamReader<PrintRequest> requestStream, IAsyncStreamWriter<PrintResponse> responseStream, IPrintAction action)
        {
            while (action != null)
            {
                var response = await action.Generate();
                
                if (response.Action == ClientAction.Done)
                {
                    await responseStream.WriteAsync(new PrintResponse
                    {
                        Action = Grpc.ClientAction.Done
                    });

                    break;
                }
                
                await responseStream.WriteAsync(new PrintResponse
                {
                    Payload = ByteString.CopyFrom(response.Payload),
                    Action = response.Action.Pack()
                });

                if (response.Action == ClientAction.Read)
                {
                    // Told the client we expect a read back, wait for that reply
                    var reply = await GetNextClientReply(requestStream);
                    
                    response = await action.Process(reply.Payload.Span);
                }
                
                if (response.Action == ClientAction.Done)
                {
                    await responseStream.WriteAsync(new PrintResponse
                    {
                        Action = Grpc.ClientAction.Done
                    });

                    break;
                }

                // Still more to do
                action = response.NextAction;
            }
        }

        private async Task<Models.Invoice> GetInvoice(Guid accountId, string invoiceNumber)
        {
            var invoice = await _invoiceStore.GetByNumber(accountId, invoiceNumber);
            
            if (invoice == null)
            {
                throw new RpcException(new Status(StatusCode.FailedPrecondition, $"invoice {invoiceNumber} does not exist"));
            }

            return invoice;
        }

        private static async Task<PrintRequest?> GetNextClientReply(IAsyncStreamReader<PrintRequest> requestStream)
        {
            if (await requestStream.MoveNext())
            {
                return requestStream.Current;    
            }

            return null;
        }
    }
}