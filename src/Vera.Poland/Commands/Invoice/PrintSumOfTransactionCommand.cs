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
  /// See 4.3.9 Sum of transaction
  /// 
  /// Format
  ///   ESC MFB T ESC MFB1 a<total> ESC MFE
  /// 
  /// Description
  ///   The end of the sales phase.Compares the current sum of the receipt with the <total> value sent by the sales application.
  ///   If the values match, print the tax statement and the summary line.
  /// 
  /// Arguments
  ///   • <total> - determines the current sum of the receipt.
  /// </summary>
  /// <example>
  ///   ESC MFB T ESC MFB1 a128,00 ESC MFE 
  /// </example>
  [SuppressMessage("ReSharper", "InvalidXmlDocComment")]
  public class PrintSumOfTransactionCommand : IFiscalPrinterCommand<PrintSumOfTransactionRequest>
  {
    public void Validate(PrintSumOfTransactionRequest input)
    {
      if (input.Total <= 0)
      {
        throw new ArgumentOutOfRangeException(nameof(input.Total), $"{nameof(input.Total)} cannot be 0 or lower than 0");
      }
    }

    public void BuildRequest(PrintSumOfTransactionRequest input, List<byte> request)
    {
      //Acceptance conditions
      //  • Receipt is open
      //  • Receipt is not in the payment phase
      //  • Sales phase
      //Possible further operations
      //  • Footer
      //  • Settlement of the returnable packaging
      //  • Conversion or payment with foreign currency
      //  • Cancellation of the transaction
      //  • Completion of transaction

      request.Add(FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb);
      request.Add(FiscalPrinterDividers.T);
      request.Add(FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb1);
      request.Add(FiscalPrinterDividers.a);
      request.Add(EncodingHelper.Encode(input.Total));
      request.Add(FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfe);
    }
  }
}