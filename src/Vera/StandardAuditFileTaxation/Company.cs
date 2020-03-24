using System;
using System.Collections.Generic;
using System.Linq;

namespace Vera.StandardAuditFileTaxation
{
  public class Company
  {
    /// <summary>
    /// Null if there is no entry for this company. E.g. when just the <see cref="RegistrationNumber"/> has
    /// been saved but there is no record for the rest of the data.
    /// </summary>
    public string SystemID { get; set; }

    /// <summary>
    /// Fiscal registration number of the company.
    /// </summary>
    public string RegistrationNumber { get; set; }

    public string BranchNumber { get; set; }

    public string Name { get; set; }

    public ICollection<Address> Addresses { get; set; } = new List<Address>();

    public Contact Contact { get; set; }

    public TaxRegistration TaxRegistration { get; set; }

    public BankAccount BankAccount { get; set; }

    public Address StreetAddress => Addresses.FirstOrDefault(a => a.Type == AddressType.Street);
    public Address BillingAddress => Addresses.FirstOrDefault(a => a.Type == AddressType.Billing);
    public Address ShipToAddress => Addresses.FirstOrDefault(a => a.Type == AddressType.ShipTo);
  }
}