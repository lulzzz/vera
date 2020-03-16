using System;

namespace Vera.StandardAuditFileTaxation
{
  public sealed class TaxRegistration
  {
    /// <summary>
    /// Used to identify to which tax regime is used. Used when more than one tax regime is used.
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// VAT number.
    /// </summary>
    public string Number { get; set; }

    /// <summary>
    /// Identification of the revenue body to which this type refers.
    /// </summary>
    public string Authority { get; set; }

    /// <summary>
    /// Date that the tax information was last checked.
    /// </summary>
    public DateTime VerificationDate { get; set; }
  }
}