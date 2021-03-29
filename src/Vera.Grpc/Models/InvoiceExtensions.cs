using System;
using System.Linq;
using Google.Protobuf.WellKnownTypes;
using Vera.Models;

namespace Vera.Grpc.Models
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
                TerminalId = invoice.TerminalId,
                PeriodId = invoice.PeriodId,
                Supplier = invoice.Supplier.Unpack(),
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
                PeriodId = invoice.PeriodId,
                SystemId = invoice.SystemId ?? string.Empty,
                TerminalId = invoice.TerminalId ?? string.Empty,
                Remark = invoice.Remark ?? string.Empty,
                Supplier = invoice.Supplier.Pack(),
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
                    Code = line.Product.Code,
                    Description = line.Product.Description
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
                Category = taxes.Category switch
                {
                    TaxesCategory.High => TaxValue.Types.Category.High,
                    TaxesCategory.Low => TaxValue.Types.Category.Low,
                    TaxesCategory.Zero => TaxValue.Types.Category.Zero,
                    TaxesCategory.Exempt => TaxValue.Types.Category.Exempt,
                    TaxesCategory.Intermediate => TaxValue.Types.Category.Intermediate,
                    _ => throw new ArgumentOutOfRangeException()
                },
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
                Category = payment.Category switch
                {
                    PaymentCategory.Other => Payment.Types.Category.Other,
                    PaymentCategory.Debit => Payment.Types.Category.Debit,
                    PaymentCategory.Credit => Payment.Types.Category.Credit,
                    PaymentCategory.Cash => Payment.Types.Category.Cash,
                    PaymentCategory.Change => Payment.Types.Category.Change,
                    PaymentCategory.Voucher => Payment.Types.Category.Voucher,
                    PaymentCategory.Online => Payment.Types.Category.Online,
                    _ => throw new ArgumentOutOfRangeException()
                },
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
                Payment.Types.Category.Other => PaymentCategory.Other,
                Payment.Types.Category.Debit => PaymentCategory.Debit,
                Payment.Types.Category.Credit => PaymentCategory.Credit,
                Payment.Types.Category.Cash => PaymentCategory.Cash,
                Payment.Types.Category.Voucher => PaymentCategory.Voucher,
                Payment.Types.Category.Online => PaymentCategory.Online,
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
                        TaxValue.Types.Category.High => TaxesCategory.High,
                        TaxValue.Types.Category.Low => TaxesCategory.Low,
                        TaxValue.Types.Category.Zero => TaxesCategory.Zero,
                        TaxValue.Types.Category.Exempt => TaxesCategory.Exempt,
                        TaxValue.Types.Category.Intermediate => TaxesCategory.Intermediate,
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
                    Description = product.Description
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