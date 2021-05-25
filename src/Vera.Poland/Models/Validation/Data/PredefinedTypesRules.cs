using System.Collections.Generic;
using Vera.Poland.Models.Enums;

namespace Vera.Poland.Models.Validation.Data
{
  /// <summary>
  /// See 1.3.12 Payments and footers of receipts and invoices
  ///   A list of predefined types of payments
  /// </summary>
  public class PredefinedTypesRules
  {
    public SupportedPaymentAndFooterTypes Index { get; set; }
    public string PredefinedText { get; set; }
    public int NumberOfParameters { get; set; }
    public List<ParameterTypeEnum> ParameterTypes { get; set; }
    public string Notes { get; set; }

    public static readonly List<PredefinedTypesRules> SupportedTypes = new()
    {
      // Payment Types
      new PredefinedTypesRules
      {
        Index = SupportedPaymentAndFooterTypes.A,
        ParameterTypes = new List<ParameterTypeEnum> { ParameterTypeEnum.Type2 },
        NumberOfParameters = 1,
        Notes = "",
        PredefinedText = "Paid"
      },
      new PredefinedTypesRules
      {
        Index = SupportedPaymentAndFooterTypes.B,
        ParameterTypes = new List<ParameterTypeEnum> { ParameterTypeEnum.Type3 },
        NumberOfParameters = 1,
        Notes = "",
        PredefinedText = "Rest"
      },
      new PredefinedTypesRules
      {
        Index = SupportedPaymentAndFooterTypes.E,
        ParameterTypes = new List<ParameterTypeEnum> { ParameterTypeEnum.Type2 },
        NumberOfParameters = 1,
        Notes = "",
        PredefinedText = "Cash"
      },
      new PredefinedTypesRules
      {
        Index = SupportedPaymentAndFooterTypes.F,
        ParameterTypes = new List<ParameterTypeEnum> { ParameterTypeEnum.Type2 },
        NumberOfParameters = 1,
        Notes = "",
        PredefinedText = "Check"
      },
      new PredefinedTypesRules
      {
        Index = SupportedPaymentAndFooterTypes.G,
        ParameterTypes = new List<ParameterTypeEnum> { ParameterTypeEnum.Type2 },
        NumberOfParameters = 1,
        Notes = "",
        PredefinedText = "Credit card"
      },
      new PredefinedTypesRules
      {
        Index = SupportedPaymentAndFooterTypes.H,
        ParameterTypes = new List<ParameterTypeEnum> { ParameterTypeEnum.Type2 },
        NumberOfParameters = 1,
        Notes = "",
        PredefinedText = "Voucher"
      },
      new PredefinedTypesRules
      {
        Index = SupportedPaymentAndFooterTypes.K,
        ParameterTypes = new List<ParameterTypeEnum> { ParameterTypeEnum.Type2 },
        NumberOfParameters = 1,
        Notes = "",
        PredefinedText = "Local credit"
      },
      new PredefinedTypesRules
      {
        Index = SupportedPaymentAndFooterTypes.a,
        ParameterTypes = new List<ParameterTypeEnum> { ParameterTypeEnum.Type3 },
        NumberOfParameters = 1,
        Notes = "",
        PredefinedText = "Payment"
      },
      new PredefinedTypesRules
      {
        Index = SupportedPaymentAndFooterTypes.t,
        ParameterTypes = new List<ParameterTypeEnum> { ParameterTypeEnum.Type2 },
        NumberOfParameters = 1,
        Notes = "",
        PredefinedText = "Card"
      },
      new PredefinedTypesRules
      {
        Index = SupportedPaymentAndFooterTypes.PlusB,
        ParameterTypes = new List<ParameterTypeEnum> { ParameterTypeEnum.Type2 },
        NumberOfParameters = 1,
        Notes = "",
        PredefinedText = "Credit"
      },
      new PredefinedTypesRules
      {
        Index = SupportedPaymentAndFooterTypes.PlusO,
        ParameterTypes = new List<ParameterTypeEnum> { ParameterTypeEnum.Type2 },
        NumberOfParameters = 1,
        Notes = "",
        PredefinedText = "Mobile"
      },
      new PredefinedTypesRules
      {
        Index = SupportedPaymentAndFooterTypes.Q,
        ParameterTypes = new List<ParameterTypeEnum> { ParameterTypeEnum.Type5Q, ParameterTypeEnum.Type6Q },
        NumberOfParameters = 2,
        Notes = "",
        PredefinedText = "PaymentTypesDefinedByThePCommand"
      },
      // Footers Types
      new PredefinedTypesRules
      {
        Index = SupportedPaymentAndFooterTypes.Two_2,
        ParameterTypes = new List<ParameterTypeEnum> { ParameterTypeEnum.Ignored },
        NumberOfParameters = 1,
        Notes = "",
        PredefinedText = "Bar code at the end footer section"
      },
      // Handled by PrintFooterPaymentDetailsCommand
      new PredefinedTypesRules
      {
        Index = SupportedPaymentAndFooterTypes.h,
        ParameterTypes = new List<ParameterTypeEnum> { ParameterTypeEnum.Ignored },
        NumberOfParameters = 1,
        Notes = "",
        PredefinedText = "Bar code at the end footer section"
      },
      new PredefinedTypesRules
      { 
        Index = SupportedPaymentAndFooterTypes.R,
        ParameterTypes = new List<ParameterTypeEnum> { ParameterTypeEnum.Type1 },
        NumberOfParameters = 1,
        Notes = "",
        PredefinedText = "Nr transakcji (Transaction number)"
      },
      new PredefinedTypesRules
      {
        Index = SupportedPaymentAndFooterTypes.N,
        ParameterTypes = new List<ParameterTypeEnum> { ParameterTypeEnum.Type1 },
        NumberOfParameters = 1,
        Notes = "",
        PredefinedText = "Registration numer (Nume rejestracyjny)"
      }
    };
  }
}