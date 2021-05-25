using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Vera.Poland.Contracts;
using Vera.Poland.Extensions;
using Vera.Poland.Models.Requests;
using Vera.Poland.Protocol;

namespace Vera.Poland.Commands.Invoice
{
  /// <summary>
  /// See 4.3.15 Print the VAT registration number (NIP) of the customer on the VAT invoice and receipt for more details
  /// 
  /// Format
  ///   ESC MFB j <customer’s NIP> ESC MFE
  /// 
  /// Description
  ///   Prints the VAT number (NIP) of the customer on the VAT invoice and receipt.
  /// 
  /// Arguments
  ///   • customer’s NIP - gives the tax identification number of the customer's NIP. The parameter can accept alphanumeric characters.
  ///   Special characters with the exception of the "-" sign and Polish letters return an error. The printer accepts up to 30 characters.
  /// </summary>
  /// <example>
  ///   ESC MFB j111-22-33-44 ESC MFE 
  /// </example>
  [SuppressMessage("ReSharper", "InvalidXmlDocComment")]
  public class PrintVatRegistrationNumberCommand : IFiscalPrinterCommand<PrintVatRegistrationNumberRequest>
  {
    public void Validate(PrintVatRegistrationNumberRequest input)
    {
      if (string.IsNullOrWhiteSpace(input.CustomerNip))
      {
        throw new ArgumentNullException(
          nameof(input.CustomerNip),
          $"{nameof(input.CustomerNip)} cannot be null or empty");
      }
    }

    public void BuildRequest(PrintVatRegistrationNumberRequest input, List<byte> request)
    {
      // Acceptance conditions
      //   • Sales opened
      // Possible further operations
      //   • Footer
      //   • Settlement of the returnable packaging
      //   • Conversion or payment with foreign currency
      //   • Cancellation of the transaction
      //   • Completion of transaction

      request.Add(FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb);
      request.Add(FiscalPrinterDividers.j);
      request.Add(EncodingHelper.Encode(input.CustomerNip));
      request.Add(FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfe);
    }
  }
}