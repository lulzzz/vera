using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using Vera.StandardAuditFileTaxation;
using Invoice = Vera.Models.Invoice;

namespace Vera.Audit.Extract
{
    public class CustomerAuditDataExtractor : IAuditDataExtractor
    {
        private readonly IList<Customer> _customers;

        public CustomerAuditDataExtractor()
        {
            _customers = new List<Customer>();
        }

        public void Extract(Invoice invoice)
        {
            var customer = invoice.Customer;

            if (customer == null || _customers.Any(c => customer.SystemId == c.SystemID))
            {
                return;
            }

            var addresses = new List<Address>();

            if (customer.BillingAddress != null)
            {
                addresses.Add(ExtractAddress(customer.BillingAddress, AddressType.Billing));
            }

            if (customer.ShippingAddress != null)
            {
                addresses.Add(ExtractAddress(customer.ShippingAddress, AddressType.ShipTo));
            }

            _customers.Add(new Customer
            {
                SystemID = customer.SystemId,
                Name = customer.CompanyName,
                RegistrationNumber = customer.RegistrationNumber,
                TaxRegistration = new TaxRegistration
                {
                    Number = customer.TaxRegistrationNumber
                },
                BankAccount = new BankAccount
                {
                    AccountNumber = customer.BankAccount.Number
                },
                Contact = new Contact
                {
                    Email = customer.Email,
                    Person = new PersonName
                    {
                        FirstName = customer.FirstName,
                        LastName = customer.LastName
                    }
                },
                Addresses = addresses
            });
        }

        public void Apply(StandardAuditFileTaxation.Audit audit)
        {
            foreach (var customer in _customers)
            {
                audit.MasterFiles.Customers.Add(customer);
            }
        }
        
        // TODO(kevin): extract to helper method
        private Address ExtractAddress(Models.Address address, AddressType type)
        {
            return new()
            {
                City = address.City,
                Country = address.Country,
                Number = address.Number,
                Region = address.Region,
                Street = address.Street,
                PostalCode = address.PostalCode,
                Type = type
            };
        }
    }
}