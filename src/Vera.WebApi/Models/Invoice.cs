using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vera.Models;

namespace Vera.WebApi.Models
{
    // https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations?view=netframework-4.8
    // https://docs.microsoft.com/en-us/aspnet/core/mvc/models/validation?view=aspnetcore-3.1#ivalidatableobject

    public class Invoice
    {
        [Required]
        public Guid Account { get; set; }

        [Required]
        public string SystemId { get; set; }

        [Required]
        public Store Store { get; set; }

        public Customer Customer { get; set; }

        [Required]
        public Employee Employee { get; set; }

        public DateTime Timestamp { get; set; }

        [Required]
        public string TerminalId { get; set; }

        public bool Manual { get; set; }

        [Required]
        public ICollection<InvoiceLine> Lines { get; set; }

        [Required]
        public ICollection<Payment> Payments { get; set; }

        public ICollection<Settlement> Discounts { get; set; }

        public int Period { get; set; }
        public int PeriodYear { get; set; }

        public Vera.Models.Invoice ToModel()
        {
            return new Vera.Models.Invoice
            {
                SystemId = SystemId,
                Store = new Vera.Models.Store
                {
                    SystemId = Store.SystemId,
                    Name = Store.Name,
                    Number = Store.Number,
                    Address = new Vera.Models.Address()
                },
                Customer = new Vera.Models.Customer
                {

                },
                Employee = new Vera.Models.Employee
                {

                },
                Payments = Payments.Select(p => new Vera.Models.Payment
                {
                    Reference = p.Reference,
                    Description = p.Description,
                    Amount = p.Amount
                }).ToList(),
                Lines = Lines.Select(l => new Vera.Models.InvoiceLine
                {
                    Description = l.Description,
                    Quantity = l.Quantity,
                    Product = l.Product == null ? null : new Vera.Models.Product
                    {
                        Description = l.Product.Description,
                        ArticleCode = l.Product.ArticleCode
                    },
                    Tax = new TaxInformation
                    {

                    },
                    Settlements = l.Discounts?.Select(s => new Vera.Models.Settlement
                    {

                    }).ToList(),
                    Gross = l.Gross,
                    Net = l.Gross
                }).ToList(),
                Manual = Manual,
                Timestamp = Timestamp,
                TerminalId = TerminalId,
                FiscalPeriod = Period,
                FiscalYear = PeriodYear
            };
        }
    }

    public class Store
    {
        [Required]
        public string SystemId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Number { get; set; }

        [Required]
        public Address Address { get; set; }
    }

    public class Employee
    {
        [Required]
        public string SystemId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class Customer
    {
        [Required]
        public string SystemId { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string Email { get; set; }

        public string CompanyName { get; set; }
        public string RegistrationNumber { get; set; }
        public string TaxRegistrationNumber { get; set; }

        public Address BillingAddress { get; set; }
        public Address ShippingAddress { get; set; }
    }

    public class Product
    {
        [Required]
        public string ArticleCode { get; set; }

        [Required]
        public string Description { get; set; }

        public ProductGroup Group { get; set; }
    }

    public class InvoiceLine
    {
        public Product Product { get; set; }

        [Required]
        public string Description { get; set; }

        public int Quantity { get; set; }

        /// <summary>
        /// Unit of measure.
        /// </summary>
        public string Unit { get; set; }

        public InvoiceLineType Type { get; set; }

        [Required]
        public decimal Gross { get; set; }

        [Required]
        public decimal Net { get; set; }

        public ICollection<Settlement> Discounts { get; set; }

        // taxes
    }

    public class Payment
    {
        [Required]
        public string Reference { get; set; }

        public DateTime Timestamp { get; set; }

        [Required]
        public string Code { get; set; }

        [Required]
        public string Description { get; set; }

        public decimal Amount { get; set; }

        public PaymentCategory Category { get; set; }
    }

    public class Settlement
    {
        public string SystemId { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
    }

    public class Address
    {
        public string Street { get; set; }

        public string Number { get; set; }

        public string City { get; set; }

        public string PostalCode { get; set; }

        /// <summary>
        /// ISO 3166-1
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// ISO 3166-2
        /// </summary>
        public string Region { get; set; }
    }

    public enum InvoiceLineType
    {
        Goods = 0,
        Services = 1
    }

    public enum ProductGroup
    {
        Other = 1
    }

    public enum PaymentCategory
    {
        Other = 0,
        Debit = 1,
        Credit = 2,
        Cash = 3,
        Voucher = 4,
        Online = 5
    }
}