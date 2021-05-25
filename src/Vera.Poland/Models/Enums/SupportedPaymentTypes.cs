using System.Diagnostics.CodeAnalysis;

namespace Vera.Poland.Models.Enums
{
  /// <summary>
  /// See 1.3.12 Payments and footers of receipts and invoices
  ///
  /// Only footer with Index 2 is handled, see PredefinedTypesOfFooterLinesChunks and PredefinedTypesOfPaymentChunks for all handled types
  ///   - For footer h - see PrintFooterPaymentDetailsCommand
  /// 
  /// Format
  ///   ESC MFB R<index> <parameter> [LF<parameter>] ESC MFE
  ///
  /// Description
  /// The command allows to print additional information on documents issued by the printer.
  /// The type of payment method used is printed in the fiscal part of the receipt or invoice,
  /// whereas the advertising and information content are below the fiscal logotype, in the so-called footer.
  /// These lines consist of predefined text and parameters that are defined by the user.On the printout,
  /// the predefined text is aligned to the left margin, and the parameter is aligned to the right margin.
  /// The number of footers in the fiscal receipt is limited to 35.
  ///
  /// Arguments
  ///    • <index> - specifies the index of one of the predefined subtitles from the basic or extended set.
  ///    • <parameter> - it is part of the receipt footer line, which can be defined by the user.The parameter’s length is the maximum number of characters in the line, except for the parameters for which the table provides otherwise.
  ///
  /// Parameters:
  ///    • Type 1 - the parameter is an alphanumeric string. The length of the string is the maximum number of characters in the line.
  ///    • Type 2 – the parameter is an alphanumeric string. The footer is treated as a payment.The length of the string is max. 12 characters, including max. 7 before comma.
  ///    • Type 3 - the parameter is an alphanumeric string. The footer is treated as a payment using the appropriate format: Payment description + space + value.
  ///    • Type 4 - the parameter requires a graphic’s number from the range 1 to 8 to be printed.
  /// </summary>
  [SuppressMessage("ReSharper", "InvalidXmlDocComment")]
  [SuppressMessage("ReSharper", "InconsistentNaming")]
  public enum SupportedPaymentAndFooterTypes
  {
    // Payments
    A,
    B,
    E,
    F,
    G,
    H,
    K,
    a,
    t,
    PlusB,
    PlusO,
    Q,

    // Footers
    Two_2,
    h,
    R,
    N
  }
}