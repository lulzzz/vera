using System.Collections.Generic;
using System.Linq;
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

            if (customer == null || _customers.Any(c => customer.SystemID == c.SystemID))
            {
                return;
            }

            _customers.Add(new Customer
            {
                Contact = new Contact
                {
                    Email = customer.Email,
                    Person = new PersonName
                    {
                        FirstName = customer.FirstName,
                        LastName = customer.LastName
                    }
                },
                RegistrationNumber = customer.RegistrationNumber,
                TaxRegistration = new TaxRegistration
                {
                    Number = customer.TaxRegistrationNumber
                }
            });
        }

        public void Apply(StandardAuditFileTaxation.Audit audit)

        {
            foreach (var customer in _customers)
            {
                audit.MasterFiles.Customers.Add(customer);
            }
        }
    }
}