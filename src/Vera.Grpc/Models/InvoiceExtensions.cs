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
                    CompanyName = invoice.Customer.CompanyName ?? string.Empty,
                    RegistrationNumber = invoice.Customer.RegistrationNumber,
                    TaxRegistrationNumber = invoice.Customer.TaxRegistrationNumber,
                    ShippingAddress = invoice.Customer.ShippingAddress.Unpack(),
                    BillingAddress = invoice.Customer.BillingAddress.Unpack()
                };
            }

            result.ShipTo = invoice.ShippingAddress.Unpack();
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
                SystemId = invoice.SystemId,
                TerminalId = invoice.TerminalId,
                Remark = invoice.Remark,
                Supplier = new Supplier
                {
                    Name = invoice.Supplier.Name,
                    RegistrationNumber = invoice.Supplier.RegistrationNumber ?? string.Empty,
                    TaxRegistrationNumber = invoice.Supplier.TaxRegistrationNumber ?? string.Empty,
                    SystemId = invoice.Supplier.SystemId,
                    Address = invoice.Supplier.Address.Pack()
                },
                Employee = new Employee
                {
                    SystemId = invoice.Employee.SystemId,
                    FirstName = invoice.Employee.FirstName,
                    LastName = invoice.Employee.LastName
                }
                // TODO: billing/shipping for this invoice from customer?
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
                Gross = line.Gross,
                Net = line.Net,
                Product = line.Product == null ? null : new Product()
                {
                    Code = line.Product.Code,
                    Description = line.Product.Description,
                    Group = line.Product.Type switch
                    {
                        ProductTypes.Service => Product.Types.Group.Other,
                        ProductTypes.Goods => Product.Types.Group.Other,
                        _ => throw new ArgumentOutOfRangeException()
                    }
                },
                Quantity = line.Quantity,
                // TODO: map
                Tax = new TaxValue(),
                // TODO: map
                Type = InvoiceLine.Types.Type.Goods,
                Unit = line.UnitOfMeasure ?? string.Empty,
                UnitPrice = line.UnitPrice
            };

            result.Settlements.AddRange(line.Settlements.Select(Pack));

            return result;
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
                        _ => throw new ArgumentOutOfRangeException(nameof(line.Tax.Category), line.Tax.Category,
                            "unknown tax category")
                    },
                    ExemptionCode = line.Tax.ExemptionCode,
                    ExemptionReason = line.Tax.ExemptionReason
                }
            };

            if (line.Product != null)
            {
                var productType = line.Product.Group switch
                {
                    Product.Types.Group.Other => ProductTypes.Goods,
                    _ => throw new ArgumentOutOfRangeException(nameof(line.Product.Group), line.Product.Group,
                        "unknown product group")
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
                result.Settlements = line.Settlements.Select(Unpack).ToList();
            }

            return result;
        }
    }
}