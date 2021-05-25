using System;
using Vera.Poland.Models.Enums;
using Vera.Poland.Protocol;

namespace Vera.Poland.Extensions
{
  public static class FiscalPrinterVatClassesExtensions
  {
    public static byte EncodeVatClass(this VatClass vat)
    {
      return vat switch
      {
        VatClass.A => FiscalPrinterVatClasses.A,
        VatClass.B => FiscalPrinterVatClasses.B,
        VatClass.C => FiscalPrinterVatClasses.C,
        VatClass.D => FiscalPrinterVatClasses.D,
        VatClass.E => FiscalPrinterVatClasses.E,
        VatClass.F => FiscalPrinterVatClasses.F,
        VatClass.G => FiscalPrinterVatClasses.G,
        _ => throw new ArgumentOutOfRangeException(nameof(vat), vat, null)
      };
    }
  }
}