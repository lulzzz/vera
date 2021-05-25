using System.Collections.Generic;
using Vera.Poland.Models.Enums;

namespace Vera.Poland.Models
{
  public class VatAmounts
  {
    public List<AmountInRate> AmountsInRates { get; set; }

    public VatAmounts()
    {
      AmountsInRates = new List<AmountInRate>
      {
        new AmountInRate
        {
          Vat = VatClass.A,
          Amount = 0.00m
        },

        new AmountInRate
        {
          Vat = VatClass.B,
          Amount = 0.00m
        },

        new AmountInRate
        {
          Vat = VatClass.C,
          Amount = 0.00m
        },

        new AmountInRate
        {
          Vat = VatClass.D,
          Amount = 0.00m
        },

        new AmountInRate
        {
          Vat = VatClass.E,
          Amount = 0.00m
        },

        new AmountInRate
        {
          Vat = VatClass.F,
          Amount = 0.00m
        },

        new AmountInRate
        {
          Vat = VatClass.G,
          Amount = 0.00m
        },
      };
    }
  }
}