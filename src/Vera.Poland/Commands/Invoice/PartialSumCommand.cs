using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Vera.Poland.Contracts;
using Vera.Poland.Extensions;
using Vera.Poland.Models.Requests.Invoice;
using Vera.Poland.Protocol;

namespace Vera.Poland.Commands.Invoice
{
  /// <summary>
  /// See 4.3.5 Partial sum for more details
  ///
  /// Format
  ///   ESC MFB Q ESC MFB1<partial_sum> ESC MFE
  ///
  /// Description
  ///   Compares the current sum of the receipt or invoice with the sum sent.
  ///   If the sums are not compatible, an error is signaled.
  ///   If the comparison of the partial sum sent from the application and the sum from
  ///   the accumulator is correct then further action of the command depends on the next
  ///   instruction sent from the application.If the next command is the end of the sale transaction,
  ///   the partial sum is not printed,
  ///   only the sales phase is closed (printout of the tax report and sum of the receipt),
  ///   otherwise the partial sum line is printed and the next command is executed.
  ///
  /// Arguments
  ///   <partial_sum> - specifies the current sum of the receipt.
  /// </summary>
  /// <example>
  ///   ESC MFB Q ESC MFB1 100,00 ESC MFE
  /// </example>
  [SuppressMessage("ReSharper", "InvalidXmlDocComment")]
  public class PartialSumCommand
  : IFiscalPrinterCommand<PartialSumRequest>
  {
    public void Validate(PartialSumRequest input)
    {
      if (input.PartialSum <= 0)
      {
        throw new ArgumentOutOfRangeException(nameof(input.PartialSum),
          "Value needs to be > 0");
      }
    }

    public void BuildRequest(PartialSumRequest input, List<byte> request)
    {
      //
      // Acceptance conditions
      //   • Receipt is open
      //   • Receipt is not in the payment phase
      // Possible further operations
      //   • Sales line
      //   • Discount or uplift to the sales line
      //   • Discount or uplift to the partial sum
      //   • Footer
      //   • Settlement of the returnable packaging
      //   • Conversion or payment with foreign currency
      //   • Cancellation of the sales line
      //   • Cancellation of the transaction
      //   • Sum of the transaction

      var encodedPartialSum = EncodingHelper.Encode(input.PartialSum);

      request.Add(
        FiscalPrinterCommands.Esc,
        FiscalPrinterCommands.Mfb,
        FiscalPrinterDividers.Q,
        FiscalPrinterCommands.Esc,
        FiscalPrinterCommands.Mfb1);

      request.Add(encodedPartialSum);
      request.Add(FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfe);
    }
  }
}