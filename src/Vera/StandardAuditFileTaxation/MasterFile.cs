using System.Collections.Generic;

namespace Vera.StandardAuditFileTaxation
{
  public sealed class MasterFile
  {
    public ICollection<Customer> Customers { get; } = new List<Customer>();
    public ICollection<Employee> Employees { get; } = new List<Employee>();
    public ICollection<Product> Products { get; } = new List<Product>();
    public ICollection<TaxTableEntry> TaxTable { get; } = new List<TaxTableEntry>();
    public ICollection<Period> Periods { get; } = new List<Period>();
  }
}