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
            // TODO(kevin): map employees
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