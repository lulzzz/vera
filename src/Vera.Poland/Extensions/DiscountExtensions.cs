using System;
using Vera.Poland.Models.Enums;
using Vera.Poland.Protocol;

namespace Vera.Poland.Extensions
{
  public static class DiscountExtensions
  {
    public static byte Encode(this DiscountType discountType)
    {
      return discountType switch
      {
        DiscountType.Amount => FiscalPrinterDividers.k,
        DiscountType.Percentage => FiscalPrinterDividers.p,
        _ => throw new ArgumentOutOfRangeException(nameof(discountType), discountType, null)
      };
    }

    public static byte Encode(this DiscountAction discountAction)
    {
      return discountAction switch
      {
        DiscountAction.Discount => FiscalPrinterDividers.A,
        DiscountAction.Uplift => FiscalPrinterDividers.U,
        DiscountAction.Reduce => FiscalPrinterDividers.P,
        _ => throw new ArgumentOutOfRangeException(nameof(discountAction), discountAction, null)
      };
    }

    public static byte Encode(this SimpleSumDiscountAction discountAction, string paramName)
    {
      return discountAction switch
      {
        SimpleSumDiscountAction.Discount => FiscalPrinterDividers.A,
        SimpleSumDiscountAction.Uplift => FiscalPrinterDividers.U,
        _ => throw new ArgumentOutOfRangeException(paramName, paramName, null)
      };
    }

    public static byte Encode(this DiscountActionType discountActionType)
    {
      return discountActionType switch
      {
        DiscountActionType.Give => FiscalPrinterDividers.a,
        DiscountActionType.Withdraw => FiscalPrinterDividers.c,
        _ => throw new ArgumentOutOfRangeException(nameof(discountActionType), discountActionType, null)
      };
    }
  }
}