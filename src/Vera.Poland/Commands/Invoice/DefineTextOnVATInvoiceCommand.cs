using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Vera.Extensions;
using Vera.Poland.Contracts;
using Vera.Poland.Extensions;
using Vera.Poland.Models.Requests.Invoice;
using Vera.Poland.Protocol;

namespace Vera.Poland.Commands.Invoice
{
  /// <summary>
  /// See 4.3.16 Defining any text on a VAT invoice
  /// Format
  ///   ESC MFB i ESC<description> [LF<text_line1>…LF<text_line5>] ESC MFE
  ///
  /// Description
  ///   It allows you to print any text on a VAT invoice.You can define up to 5 lines of text,
  ///   with length max 13 characters in the one line.An order can be sent 3 times in one transaction.
  ///   We can use this order to print the receiver's name on a VAT invoice.
  /// </summary>
  [SuppressMessage("ReSharper", "InvalidXmlDocComment")]
  public class DefineTextOnVatInvoiceCommand : IFiscalPrinterCommand<DefineTextOnVatInvoiceRequest>
  {
    private const int MaxTextLines = 5;
    private const int MaxCharactersPerTextLine = 13;


    public void Validate(DefineTextOnVatInvoiceRequest input)
    {
      if (input.Description.IsNullOrWhiteSpace())
      {
        throw new ArgumentOutOfRangeException(nameof(input.Description), $"{nameof(input.Description)} cannot be null or empty");
      }
      if (input.TextLines == null)
      {
        throw new ArgumentNullException(nameof(input.TextLines), $"{nameof(input.TextLines)} TextLines are null");
      }
      if (input.TextLines.Count > MaxTextLines)
      {
        throw new ArgumentOutOfRangeException(nameof(input.TextLines), $"{nameof(input.TextLines)} cannot exceed MaxTextLines");
      }
      if (input.TextLines.Any(x => x.Length > MaxCharactersPerTextLine))
      {
        throw new ArgumentOutOfRangeException(nameof(input.TextLines),
          $"{nameof(input.TextLines)} cannot have lines that contain more than {MaxCharactersPerTextLine} per line");
      }
    }

    public void BuildRequest(DefineTextOnVatInvoiceRequest input, List<byte> request)
    {
      request.Add(FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb);
      request.Add(FiscalPrinterDividers.i);
      request.Add(FiscalPrinterCommands.Esc);
      request.Add(EncodingHelper.Encode(input.Description));

      if (input.TextLines.Any())
      {
        foreach (var textLine in input.TextLines
          .Where(textLine => !textLine.IsNullOrWhiteSpace()))
        {
          request.Add(FiscalPrinterDividers.Lf);
          request.Add(EncodingHelper.Encode(textLine));
        }
      }

      request.Add(FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfe);
    }
  }
}