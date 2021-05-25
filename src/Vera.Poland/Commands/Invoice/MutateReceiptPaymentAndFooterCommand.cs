using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Vera.Extensions;
using Vera.Poland.Contracts;
using Vera.Poland.Extensions;
using Vera.Poland.Models.Enums;
using Vera.Poland.Models.Requests.Invoice;
using Vera.Poland.Models.Validation.Data;
using Vera.Poland.Protocol;

namespace Vera.Poland.Commands.Invoice
{
  /// <summary>
  /// See 1.3.12 Payments and footers of receipts and invoices for more details
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
  ///
  /// Edge cases treated as extra parameters:
  ///    • Type 5Q - required for Payment Type Q:
  ///    • Type 6Q - required for Payment Type Q:
  /// </summary>
  [SuppressMessage("ReSharper", "InvalidXmlDocComment")]
  public class MutateReceiptPaymentAndFooterCommand : IFiscalPrinterCommand<MutateReceiptPaymentAndFooterRequest>
  {
    private const int ParameterType1MaxLength = 35;
    private const int ParameterType2MaxLength = 12;
    private const int ParameterType2MaxLengthBeforeComma = 7;

    private const int ParameterType5QExactLength = 18;
    private const int ParameterType6QMaxLength = 22; // programmers manual

    public void Validate(MutateReceiptPaymentAndFooterRequest input)
    {
      // Handled by PrintFooterPaymentDetailsCommand
      if (input.Index == SupportedPaymentAndFooterTypes.h)
      {
        throw new InvalidOperationException(
          $"Index {SupportedPaymentAndFooterTypes.h} is not supported, use {nameof(PrintFooterPaymentDetailsCommand)} instead");
      }

      var predefinedTypeOfPaymentRules =
        PredefinedTypesRules.SupportedTypes.Single(x => x.Index == input.Index);

      if (input.Parameters.Count != predefinedTypeOfPaymentRules.NumberOfParameters)
      {
        throw new ArgumentOutOfRangeException(nameof(input.Index),
          $"Invalid number of parameters for type {input.Index}, expected: {predefinedTypeOfPaymentRules.NumberOfParameters}, but was {input.Parameters.Count}");
      }

      for (var i = 0; i < input.Parameters.Count; i++)
      {
        if (input.Parameters[i].Name.IsNullOrWhiteSpace())
        {
          throw new ArgumentNullException(nameof(ParameterModel.Name), $"{nameof(ParameterModel.Name)} cannot be null or whitespace");
        }

        if (input.Parameters[i].Type != predefinedTypeOfPaymentRules.ParameterTypes[i])
        {
          throw new InvalidOperationException($"For {nameof(input.Index)} - {predefinedTypeOfPaymentRules.PredefinedText}: Expected parameter of type {predefinedTypeOfPaymentRules.ParameterTypes[i]} " +
                                              $"but was {input.Parameters[i].Type}");
        }

        switch (input.Parameters[i].Type)
        {
          case ParameterTypeEnum.Type1:
            {
              if (input.Parameters[i].Name.Length > ParameterType1MaxLength)
              {
                throw new ArgumentOutOfRangeException(nameof(ParameterModel.Name), $"Parameter of type {nameof(ParameterTypeEnum.Type2)}: {input.Parameters[i].Name}, cannot have" +
                                                                                   $"more than {ParameterType1MaxLength} characters");
              }
              break;
            }
          case ParameterTypeEnum.Type2:
            {
              if (input.Parameters[i].Name.Length > ParameterType2MaxLength)
              {
                throw new ArgumentOutOfRangeException(nameof(ParameterModel.Name), $"Parameter of type {nameof(ParameterTypeEnum.Type2)}: {input.Parameters[i].Name}, cannot have" +
                                                                                   $"more than {ParameterType2MaxLength} characters");
              }

              if (input.Parameters[i].Name.IndexOf(",", StringComparison.InvariantCulture) > ParameterType2MaxLengthBeforeComma)
              {
                throw new ArgumentOutOfRangeException(nameof(ParameterModel.Name), $"Parameter of type {nameof(ParameterTypeEnum.Type2)} cannot have" +
                                                                                   $"a word with length higher than {ParameterType2MaxLengthBeforeComma}" +
                                                                                   $"characters before the first comma");
              }
              break;
            }
          case ParameterTypeEnum.Type3:
            {
              // Hard to validate
              // Type 3 - the parameter is an alphanumeric string.
              // The footer is treated as a payment using the appropriate format: Payment description + space + value.
              break;
            }
          case ParameterTypeEnum.Type5Q:
            {
              if (input.Parameters[i].Name.Length != ParameterType5QExactLength)
              {
                throw new ArgumentOutOfRangeException(nameof(ParameterModel.Name), $"For {nameof(input.Index)} - {predefinedTypeOfPaymentRules.PredefinedText}: " +
                                                                                   $"Parameter of type {nameof(ParameterTypeEnum.Type5Q)} must have exactly {ParameterType5QExactLength} characters");
              }
              break;
            }
          case ParameterTypeEnum.Type6Q:
            {
              if (input.Parameters[i].Name.Length > ParameterType6QMaxLength)
              {
                throw new ArgumentOutOfRangeException(nameof(ParameterModel.Name), $"For {nameof(input.Index)} - {predefinedTypeOfPaymentRules.PredefinedText}: " +
                                                                                   $"Parameter of type {nameof(ParameterTypeEnum.Type6Q)}: {input.Parameters[i].Name}, cannot have more than {ParameterType6QMaxLength} characters");
              }
              break;
            }
          case ParameterTypeEnum.Ignored:
            {
              break;
            }
          default:
            throw new ArgumentOutOfRangeException(nameof(ParameterModel.Type), $"Unknown parameter type");
        }
      }
    }

    public void BuildRequest(MutateReceiptPaymentAndFooterRequest input, List<byte> request)
    {

      // Acceptance conditions
      //   •	Receipt is open
      //   •	Receipt in payment phase
      //   •	Summary phase or packaging settlement phase
      //
      // Possible further operations
      //   •	Footer
      //   •	Settlement of the returnable packaging
      //   •	Conversion or payment with foreign currency
      //   •	Cancellation of the transaction
      //   •	Completion of transaction

      request.Add(FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb);
      request.Add(FiscalPrinterDividers.R);
      request.Add(EncodingHelper.Encode(input.Index.ToString()));

      for (var i = 0; i < input.Parameters.Count; i++)
      {
        if (i != 0)
        {
          request.Add(FiscalPrinterDividers.Lf);
        }
        request.Add(EncodingHelper.Encode(input.Parameters[i].Name));
      }

      request.Add(FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfe);
    }
  }
}