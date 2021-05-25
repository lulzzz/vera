using System;
using System.Collections.Generic;
using Vera.Poland.Contracts;
using Vera.Poland.Extensions;
using Vera.Poland.Models.Requests.Invoice;
using Vera.Poland.Protocol;

namespace Vera.Poland.Commands.Invoice
{
  /// <summary>
  /// See 4.3.13 Cancel the receipt or invoice for more details
  /// 
  /// Format
  ///   ESC MFB T ESC MFB1 c[total] ESC MFE
  /// 
  /// Description
  ///   Cancels a receipt or invoice.
  ///   Prints the line "TRANSACTION CANCELED".
  ///   It deletes the receipt accumulators.
  ///   The number of cancelled receipts and the amount of
  ///   the sum counted are specified in the Daily Report.
  /// </summary>
  /// <example>
  ///   ESC MFB T ESC MFB1 c [total] ESC MFE
  /// </example>
  public class CancelReceiptOrInvoiceCommand: IFiscalPrinterCommand<CancelReceiptOrInvoiceRequest>
  {

    public void Validate(CancelReceiptOrInvoiceRequest input)
    {
      if (input.Total < 0)
      {
        throw new ArgumentOutOfRangeException(nameof(input.Total), $"{nameof(input.Total)} cannot be lower than 0");
      }
    }

    public void BuildRequest(CancelReceiptOrInvoiceRequest input, List<byte> request)
    {
      // Acceptance conditions
      //  • Receipt is open
      //  • Receipt in the payment phase

      request.Add(FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb);
      request.Add(FiscalPrinterDividers.T);
      request.Add(FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb1);
      request.Add(FiscalPrinterDividers.c);
      request.Add(EncodingHelper.Encode(input.Total));
      request.Add(FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfe);
    }
  }
}