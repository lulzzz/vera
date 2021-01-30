using System;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Vera.Bootstrap;
using Vera.Grpc;
using Vera.Invoices;
using Vera.Models;
using Vera.Stores;
using Vera.WebApi.Models;
using Vera.WebApi.Security;

namespace Vera.WebApi.Services
{
    [Authorize]
    public class InvoiceService : Grpc.InvoiceService.InvoiceServiceBase
    {
        private readonly IAccountStore _accountStore;
        private readonly IInvoiceStore _invoiceStore;
        private readonly IAccountComponentFactoryCollection _accountComponentFactoryCollection;

        public InvoiceService(
            IAccountStore accountStore,
            IInvoiceStore invoiceStore,
            IAccountComponentFactoryCollection accountComponentFactoryCollection
        )
        {
            _accountStore = accountStore;
            _invoiceStore = invoiceStore;
            _accountComponentFactoryCollection = accountComponentFactoryCollection;
        }

        public override async Task<CreateInvoiceReply> Create(CreateInvoiceRequest request, ServerCallContext context)
        {
            var account = await context.ResolveAccount(_accountStore, request.Invoice.Account);

            // TODO: validate invoice, very, very, very strict
            // TODO(kevin): PT - invoices > 1000 euros require a customer
            // TODO(kevin): NF525 - requires signature of original invoice on the returned line

            var factory = _accountComponentFactoryCollection.GetComponentFactory(account);

            var processor = new InvoiceProcessor(_invoiceStore, factory);

            var result = await processor.Process(Map(request.Invoice));

            return new CreateInvoiceReply
            {
                Number = result.Number,
                Sequence = result.Sequence,
                Signature = new Grpc.Signature
                {
                    Input = ByteString.CopyFromUtf8(result.RawSignature),
                    Output = ByteString.CopyFrom(result.Signature)
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

        private static Vera.Models.Invoice Map(Grpc.Invoice invoice)
        {
            var result = new Vera.Models.Invoice
            {
                SystemId = invoice.SystemId,
                Date = invoice.Timestamp.ToDateTime(),
                Manual = invoice.Manual,
                Remark = invoice.Remark,
                AccountId = Guid.Parse(invoice.Account),
                TerminalId = invoice.TerminalId,
                Supplier = new Billable
                {
                    SystemId = invoice.Supplier.SystemId,
                    Name = invoice.Supplier.Name,
                    RegistrationNumber = invoice.Supplier.RegistrationNumber,
                    TaxRegistrationNumber = invoice.Supplier.TaxRegistrationNumber,
                    Address = invoice.Supplier.Address.Unpack(),
                },
                Employee = new Vera.Models.Employee
                {
                    SystemId = invoice.Employee.SystemId,
                    FirstName = invoice.Employee.FirstName,
                    LastName = invoice.Employee.LastName
                }
            };

            if (invoice.Customer != null)
            {
                result.Customer = new Vera.Models.Customer
                {
                    SystemId = invoice.Customer.SystemId,
                    Email = invoice.Customer.Email,
                    FirstName = invoice.Customer.FirstName,
                    LastName = invoice.Customer.LastName,
                    CompanyName = invoice.Customer.CompanyName,
                    RegistrationNumber = invoice.Customer.RegistrationNumber,
                    TaxRegistrationNumber = invoice.Customer.TaxRegistrationNumber,
                    ShippingAddress = invoice.Customer.ShippingAddress.Unpack(),
                    BillingAddress = invoice.Customer.BillingAddress.Unpack()
                };
            }

            result.ShipTo = invoice.ShippingAddress.Unpack();
            result.Lines = invoice.Lines.Select(Map).ToList();
            result.Payments = invoice.Payments.Select(Map).ToList();
            result.Settlements = invoice.Settlements.Select(Map).ToList();

            return result;
        }

        private static Vera.Models.Payment Map(Grpc.Payment p)
        {
            var category = p.Category switch
            {
                Grpc.Payment.Types.Category.Other => PaymentCategory.Other,
                Grpc.Payment.Types.Category.Debit => PaymentCategory.Debit,
                Grpc.Payment.Types.Category.Credit => PaymentCategory.Credit,
                Grpc.Payment.Types.Category.Cash => PaymentCategory.Cash,
                Grpc.Payment.Types.Category.Voucher => PaymentCategory.Voucher,
                Grpc.Payment.Types.Category.Online => PaymentCategory.Online,
                _ => throw new ArgumentOutOfRangeException(nameof(p.Category), p.Category, null)
            };

            return new()
            {
                Amount = p.Amount,
                Category =  category,
                Description = p.Description
            };
        }

        private static Vera.Models.Settlement Map(Grpc.Settlement s) => new()
        {
            Amount = s.Amount,
            Description = s.Description,
            SystemId = s.SystemId
        };

        private static Vera.Models.InvoiceLine Map(Grpc.InvoiceLine line)
        {
            // TODO(kevin): validate that when exempt is given that a reason and/or code is also available
            // TODO(kevin): check if this is a requirement or optional (may depend on certifications?)

            var result = new Vera.Models.InvoiceLine
            {
                Description = line.Description,
                Gross = line.Gross,
                Net = line.Net,
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                UnitOfMeasure = line.Unit,
                Taxes = new Taxes
                {
                    Code = line.Tax.Code,
                    Rate = line.Tax.Rate,
                    Category = line.Tax.Category switch
                    {
                        TaxValue.Types.Category.High => TaxesCategory.High,
                        TaxValue.Types.Category.Low => TaxesCategory.Low,
                        TaxValue.Types.Category.Zero => TaxesCategory.Zero,
                        TaxValue.Types.Category.Exempt => TaxesCategory.Exempt,
                        TaxValue.Types.Category.Intermediate => TaxesCategory.Intermediate,
                        _ => throw new ArgumentOutOfRangeException(nameof(line.Tax.Category), line.Tax.Category, "unknown tax category")
                    },
                    ExemptionCode = line.Tax.ExemptionCode,
                    ExemptionReason = line.Tax.ExemptionReason
                }
            };

            if (line.Product != null)
            {
                var productType = line.Product.Group switch
                {
                    Grpc.Product.Types.Group.Other => ProductTypes.Goods,
                    _ => throw new ArgumentOutOfRangeException(nameof(line.Product.Group), line.Product.Group, "unknown product group")
                };

                result.Product = new Vera.Models.Product
                {
                    Code = line.Product.Code,
                    Description = line.Product.Description,
                    Type = productType,
                };
            }

            if (line.Settlements != null)
            {
                result.Settlements = line.Settlements.Select(Map).ToList();
            }

            return result;
        }
    }
}