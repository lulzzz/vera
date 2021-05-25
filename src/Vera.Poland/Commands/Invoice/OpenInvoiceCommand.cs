using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Vera.Extensions;
using Vera.Poland.Contracts;
using Vera.Poland.Extensions;
using Vera.Poland.Models.Requests.Invoice;
using Vera.Poland.Protocol;

namespace Vera.Poland.Commands.Invoice
{
  /// <summary>
  /// See 4.3.1 Opening of the receipt/invoice for more information
  ///
  /// Format
  ///
  ///   ESC MFB C[<invoice’s_number >] ESC MFE
  ///
  /// Description
  ///   If an argument[< invoice’s_number >] will be skipped
  ///   at the sign C then command determines fiscal receipt’s printout.
  /// 
  ///   Sending a number to the printer opens the printout of
  ///   the VAT invoice specified by this number.
  ///
  ///   Nothing is printed after sending this command.
  /// 
  /// Arguments
  ///   [<invoice’s_number >] – specifies the invoice number, an ASCII string higher or equal to 0x20
  ///   is accepted.The maximum invoice number corresponds to 40 characters.
  /// </summary>
  /// <example>
  ///   ESC MFB C1234567890 ESC MFE
  ///     or
  ///   ESC MFB C ESC MFE 
  /// </example>
  [SuppressMessage("ReSharper", "InvalidXmlDocComment")]
  [SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local")]
  public class OpenInvoiceCommand: IFiscalPrinterCommand<OpenInvoiceRequest>
  {
    /// <summary>
    /// Maximum number of characters accepted by the printer
    /// </summary>
    public const int MaxInvoiceCharacters = 40;

    public void Validate(OpenInvoiceRequest input)
    {
      if (input == null)
      {
        throw new ArgumentNullException(nameof(input));
      }

      if (input.InvoiceIdentifier?.Length > MaxInvoiceCharacters)
      {
        throw new ArgumentOutOfRangeException(
          nameof(OpenInvoiceRequest.InvoiceIdentifier),
          $"Must have less than {MaxInvoiceCharacters} characters");
      }
    }

    public void BuildRequest(OpenInvoiceRequest input, List<byte> request)
    {
      // Acceptance conditions
      //   • Open or closed sales period
      //   • Receipt closed
      // Possible further operations
      //   • Sales line
      //   • Footer
      //   • Cancellation of the transaction

      request.Add(FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb, FiscalPrinterDividers.C);

      if (!input.InvoiceIdentifier.IsNullOrWhiteSpace())
      {
        var encodedInvoice = EncodingHelper.Encode(input.InvoiceIdentifier);
        request.Add(encodedInvoice);
      }

      request.Add(FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfe);
    }
  }
}
