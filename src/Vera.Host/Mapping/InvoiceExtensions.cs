using System;
using System.Linq;
using Google.Protobuf.WellKnownTypes;
using Vera.Grpc;
using Vera.Models;
using CreditReference = Vera.Grpc.CreditReference;
using Customer = Vera.Grpc.Customer;
using Employee = Vera.Grpc.Employee;
using Invoice = Vera.Grpc.Invoice;
using InvoiceLine = Vera.Grpc.InvoiceLine;
using Payment = Vera.Grpc.Payment;
using Product = Vera.Grpc.Product;
using Settlement = Vera.Grpc.Settlement;

namespace Vera.Host.Mapping
{
    public static class InvoiceExtensions
    {
        public static Vera.Models.Invoice Unpack(this Invoice invoice)
        {
            var result = new Vera.Models.Invoice
            {
                SystemId = invoice.SystemId,
                Date = invoice.Timestamp.ToDateTime(),
                Manual = invoice.Manual,
                Remark = invoice.Remark,
                AccountId = Guid.Parse(invoice.Account),
                RegisterId = invoice.RegisterId,
                Supplier = new Supplier
                {
                    SystemId  = invoice.SupplierSystemId
                },
                Employee = new Vera.Models.Employee
                {
                    SystemId = invoice.Employee.SystemId,
                    FirstName = invoice.Employee.FirstName,
                    LastName = invoice.Employee.LastName
                },
            };

            if (invoice.Customer != null)
            {
                result.Customer = new Vera.Models.Customer
                {
                    SystemId = invoice.Customer.SystemId,
                    Email = invoice.Customer.Email,
                    FirstName = invoice.Customer.FirstName,
                    LastName = invoice.Customer.LastName,
                    CompanyName = invoice.Customer.CompanyName ?? string.Empty,
                    RegistrationNumber = invoice.Customer.RegistrationNumber,
                    TaxRegistrationNumber = invoice.Customer.TaxRegistrationNumber,
                    ShippingAddress = invoice.Customer.ShippingAddress.Unpack(),
                    BillingAddress = invoice.Customer.BillingAddress.Unpack()
                };
            }

            result.Lines = invoice.Lines.Select(Unpack).ToList();
            result.Payments = invoice.Payments.Select(Unpack).ToList();
            result.Settlements = invoice.Settlements.Select(Unpack).ToList();

            return result;
        }

        public static Invoice Pack(this Vera.Models.Invoice invoice)
        {
            var result = new Invoice
            {
                Account = invoice.AccountId.ToString(),
                Manual = invoice.Manual,
                Period = invoice.FiscalPeriod,
                PeriodYear = invoice.FiscalYear,
                Timestamp = invoice.Date.ToTimestamp(),
                SystemId = invoice.SystemId ?? string.Empty,
                RegisterId = invoice.RegisterId ?? string.Empty,
                Remark = invoice.Remark ?? string.Empty,
                SupplierSystemId = invoice.Supplier.SystemId,
                Employee = new Employee
                {
                    SystemId = invoice.Employee.SystemId,
                    FirstName = invoice.Employee.FirstName,
                    LastName = invoice.Employee.LastName
                }
            };

            result.Lines.AddRange(invoice.Lines.Select(Pack));
            result.Payments.AddRange(invoice.Payments.Select(Pack));
            result.Settlements.AddRange(invoice.Settlements.Select(Pack));

            if (invoice.Customer != null)
            {
                var customer = invoice.Customer;

                result.Customer = new Customer
                {
                    Email = customer.Email,
                    CompanyName = customer.CompanyName ?? string.Empty,
                    SystemId = customer.SystemId,
                    FirstName = customer.FirstName,
                    LastName = customer.LastName,
                    TaxRegistrationNumber = customer.TaxRegistrationNumber ?? string.Empty,
                    RegistrationNumber = customer.RegistrationNumber ?? string.Empty,
                    ShippingAddress = customer.ShippingAddress.Pack(),
                    BillingAddress = customer.BillingAddress.Pack()
                };
            }

            return result;
        }

        public static InvoiceLine Pack(this Vera.Models.InvoiceLine line)
        {
            var result = new InvoiceLine
            {
                Description = line.Description,
                Product = line.Product == null ? null : new Product
                {
                    // map the product type
                    Code = line.Product.Code,
                    Description = line.Product.Description,
                    Type = line.Product.Type.Map(),
                    SystemId = line.Product.SystemId
                },
                Quantity = line.Quantity,
                Tax = line.Taxes.Pack(),
                Unit = line.UnitOfMeasure ?? string.Empty,
                UnitPrice = line.UnitPrice
            };

            result.Settlements.AddRange(line.Settlements.Select(Pack));

            result.CreditReference = line.CreditReference == null ? null : new CreditReference
            {
                Number = line.CreditReference.Number,
                Reason = line.CreditReference.Reason
            };
            
            return result;
        }
        
        public static TaxValue Pack(this Taxes taxes)
        {
            return new()
            {
                Code = taxes.Code,
                Rate = taxes.Rate,
                Category = taxes.Category.Map(),
                ExemptionCode = taxes.ExemptionCode ?? string.Empty,
                ExemptionReason = taxes.ExemptionReason ?? string.Empty
            };
        }

        public static Payment Pack(this Vera.Models.Payment payment)
        {
            return new()
            {
                SystemId = payment.SystemId,
                Amount = payment.Amount,
                Category = payment.Category.Map(),
                Description = payment.Description,
                Timestamp = payment.Date.ToTimestamp()
            };
        }

        public static Settlement Pack(this Vera.Models.Settlement settlement)
        {
            return new()
            {
                Amount = settlement.Amount,
                Description = settlement.Description,
                SystemId = settlement.SystemId
            };
        }

        private static Vera.Models.Payment Unpack(Payment p)
        {
            var category = p.Category switch
            {
                Grpc.Shared.PaymentCategory.Other => PaymentCategory.Other,
                Grpc.Shared.PaymentCategory.Debit => PaymentCategory.Debit,
                Grpc.Shared.PaymentCategory.Credit => PaymentCategory.Credit,
                Grpc.Shared.PaymentCategory.Cash => PaymentCategory.Cash,
                Grpc.Shared.PaymentCategory.Voucher => PaymentCategory.Voucher,
                Grpc.Shared.PaymentCategory.Online => PaymentCategory.Online,
                _ => throw new ArgumentOutOfRangeException(nameof(p.Category), p.Category, null)
            };

            return new()
            {
                Amount = p.Amount,
                Category = category,
                Description = p.Description
            };
        }

        private static Vera.Models.Settlement Unpack(Settlement s) => new()
        {
            Amount = s.Amount,
            Description = s.Description,
            SystemId = s.SystemId
        };

        private static Vera.Models.InvoiceLine Unpack(InvoiceLine line)
        {
            var result = new Vera.Models.InvoiceLine
            {
                Description = line.Description,
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                UnitOfMeasure = line.Unit,
                Taxes = new Taxes
                {
                    Code = line.Tax.Code,
                    Rate = line.Tax.Rate,
                    Category = line.Tax.Category switch
                    {
                        Grpc.Shared.TaxCategory.High => TaxesCategory.High,
                        Grpc.Shared.TaxCategory.Low => TaxesCategory.Low,
                        Grpc.Shared.TaxCategory.Zero => TaxesCategory.Zero,
                        Grpc.Shared.TaxCategory.Exempt => TaxesCategory.Exempt,
                        Grpc.Shared.TaxCategory.Intermediate => TaxesCategory.Intermediate,
                        _ => throw new ArgumentOutOfRangeException(nameof(line.Tax.Category), line.Tax.Category,
                            "unknown tax category")
                    },
                    ExemptionCode = line.Tax.ExemptionCode,
                    ExemptionReason = line.Tax.ExemptionReason
                }
            };

            if (line.Product != null)
            {
                var product = line.Product;
                
                result.Product = new Vera.Models.Product
                {
                    SystemId = product.SystemId,
                    Code = product.Code,
                    Barcode = product.Barcode,
                    Description = product.Description,
                    Type = product.Type switch
                    {
                        Grpc.ProductType.ProductType.GiftCard => ProductType.GiftCard,
                        Grpc.ProductType.ProductType.Goods => ProductType.Goods,
                        Grpc.ProductType.ProductType.Other => ProductType.Other,
                        _ => throw new ArgumentOutOfRangeException(nameof(product.Type), product.Type,
                            "unknown product type")
                    }
                };
            }

            if (line.Settlements != null)
            {
                result.Settlements = line.Settlements.Select(Unpack).ToList();
            }

            result.CreditReference = line.CreditReference == null ? null : new Vera.Models.CreditReference
            {
                Number = line.CreditReference.Number,
                Reason = line.CreditReference.Reason
            };

            return result;
        }
    }
}