using Vera.Poland.Models.Enums;

namespace Vera.Poland.Models
{
  public class VatItem
  {
    public VatClass VatClass { get; set; }

    /// <summary>
    /// This contains the percentage for the vat class
    ///
    /// e.g. 23 is 23%
    /// </summary>
    public decimal RatePercentage { get; set; }

    public bool IsExemptFromTax { get; set; }

    public bool IsNotSet { get; set; }

    public static VatItem Exempt => new()
    {
      IsExemptFromTax = true,
    };

    public static VatItem NotSet => new()
    {
      IsNotSet = true,
    };
  }

  public enum VatItemTaxCategory
  {
    A, B, C, D, E, F, G
  }
}
