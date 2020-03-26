using System.Collections.Generic;
using System.Linq;
using Vera.StandardAuditFileTaxation;
using Invoice = Vera.Models.Invoice;

namespace Vera.Audit.Extract
{
    public class EmployeeAuditDataExtractor : IAuditDataExtractor
    {
        private readonly ICollection<Employee> _employees;

        public EmployeeAuditDataExtractor()
        {
            _employees = new List<Employee>();
        }

        public void Extract(Models.Invoice invoice)
        {
            var employee = invoice.Employee;

            if (employee == null || _employees.Any(e => employee.SystemID == e.SystemID))
            {
                return;
            }
            
            // TODO(kevin): fill in all of the data
            _employees.Add(new Employee
            {
                ID = invoice.Employee.SystemID,
                Contact = new Contact
                {
                    Person = new PersonName
                    {
                    }
                },
                RegistrationNumber = invoice.Employee.RegistrationNumber,
                TaxRegistration = new TaxRegistration
                {
                    Number = invoice.Employee.TaxRegistrationNumber
                }
            });
        }

        public void Apply(StandardAuditFileTaxation.Audit audit)
        {
            foreach (var employee in _employees)
            {
                audit.MasterFiles.Employees.Add(employee);
            }
        }
    }
}