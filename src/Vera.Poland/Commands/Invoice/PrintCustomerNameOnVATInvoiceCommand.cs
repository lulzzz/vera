using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Vera.Extensions;
using Vera.Poland.Contracts;
using Vera.Poland.Extensions;
using Vera.Poland.Models.Requests;
using Vera.Poland.Protocol;

namespace Vera.Poland.Commands.Invoice
{
  /// <summary>
  /// See 4.3.14 Print the name of the customer on the VAT invoice for more details
  /// 
  /// Description
  ///   Prints the customer's name on the VAT invoice.
  /// 
  /// Arguments
  ///   • < customer’s name_line1> .. < customer’s name_line6> - The customer's name can consist of 6 lines. Each line can not contain more
  ///   than the maximum character length in the line. The buyer's lines must be separated by the LF mark.
  /// </summary>  
  [SuppressMessage("ReSharper", "InvalidXmlDocComment")]
  public class PrintCustomerNameOnVatInvoiceCommand : IFiscalPrinterCommand<PrintCustomerNameOnVatInvoiceRequest>
  {
    public const int MaxCharactersInLine = 36;
    public const int MaxNumberOfCustomers = 6;
   
    public void Validate(PrintCustomerNameOnVatInvoiceRequest input)
    {
      if (input.CustomerNameLines == null || input.CustomerNameLines.Count == 0)
      {
        throw new ArgumentOutOfRangeException(nameof(input.CustomerNameLines), "No customer name lines are defined");
      }

      if (input.CustomerNameLines == null || input.CustomerNameLines.Count > MaxNumberOfCustomers)
      {
        throw new ArgumentOutOfRangeException(nameof(input.CustomerNameLines), "Too many customers are defined");
      }

      var characterOverflowCount = input.CustomerNameLines.Count(x => x != null && x.Length > MaxCharactersInLine); // TODO: clarify value for constant = 36 ?
      if (characterOverflowCount > 0)
      {
        throw new ArgumentOutOfRangeException(nameof(input.CustomerNameLines),
          $"Customer name line cannot contain more than {MaxCharactersInLine} characters," + // TODO: clarify value for constant = 20 ?
          $"but found {characterOverflowCount} entries that do");
      }
    }

    public void BuildRequest(PrintCustomerNameOnVatInvoiceRequest input, List<byte> request)
    {
      // Acceptance conditions
      //   • Invoice in the payment phase
      //   • Sales or summary phase of invoice
      // Possible further operations
      //   • Footer
      //   • Customer’s NIP
      //   • Cancellation of the transaction
      //   • Completion of transaction

      request.Add(FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb);
      request.Add(FiscalPrinterDividers.i);

      for (var i = 0; i < input.CustomerNameLines.Count; i++)
      {
        if (input.CustomerNameLines[i].IsNullOrWhiteSpace())
        {
          continue;
        }

        request.Add(EncodingHelper.Encode(input.CustomerNameLines[i]));
        // last customer name line does not need LF separator added afterwards
        if (i + 1 != input.CustomerNameLines.Count)
        {
          request.Add(FiscalPrinterDividers.Lf);
        }
      }

      request.Add(FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfe);
    }
  }
}