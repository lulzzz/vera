using System;
using System.Collections.Generic;
using Vera.Poland.Contracts;
using Vera.Poland.Extensions;
using Vera.Poland.Models.Requests.Invoice;
using Vera.Poland.Protocol;

namespace Vera.Poland.Commands.Invoice
{
  /// <summary>
  /// See 4.3.17 The end of the sale transaction
  /// 
  /// Format
  ///   ESC MFB E ESC MFE
  /// 
  /// Description
  ///   Ends the receipt or invoice.Updates daily accumulators.Prints the lines ending the receipt and then the footer lines.Closes the settlement of returnable packaging.
  ///
  /// Note:
  ///   In the case with invoice, it is possible to enter: „Sale date”, for example:
  ///   ESC MFB E09-09-2019 ESC MFE
  /// </summary>
  public class EndSaleTransactionCommand : IFiscalPrinterCommand<EndSaleTransactionRequest>
  {
    private const string DateFormat = "yyyy-MM-dd"; // TODO(SEBI): In the document it's written as dd-MM-yyyy, but printed as yyyy-MM-dd, test

    public void Validate(EndSaleTransactionRequest input)
    {
      if (!input.IsInvoice && input.SaleDate != null)
      {
        throw new InvalidOperationException($"Only an invoice can have a {nameof(input.SaleDate)}!");
      }
    }

    public void BuildRequest(EndSaleTransactionRequest input, List<byte> request)
    {
      // Acceptance conditions
      //  • Receipt in the payment phase

      request.Add(FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb);
      request.Add(FiscalPrinterDividers.E);

      if (input.SaleDate.HasValue)
      {
        request.Add(EncodingHelper.ConvertDateToBytes(input.SaleDate.Value, DateFormat));
      }

      request.Add(FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfe);
    }
  }
}