using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Vera.Extensions;
using Vera.Poland.Contracts;
using Vera.Poland.Extensions;
using Vera.Poland.Models.Enums;
using Vera.Poland.Models.Requests.Invoice;
using Vera.Poland.Protocol;

namespace Vera.Poland.Commands.Invoice
{
  /// <summary>
  /// See 1.3.12 Payments and footers of receipts and invoices
  ///   This command handles footer of type h, because it behaves differently than other payment or footer types
  ///
  /// See MutateReceiptPaymentAndFooterCommand for other payment and footers
  /// </summary>
  [SuppressMessage("ReSharper", "InvalidXmlDocComment")]
  public class PrintFooterPaymentDetailsCommand : IFiscalPrinterCommand<PrintFooterPaymentDetailsRequest>
  {
    public void Validate(PrintFooterPaymentDetailsRequest input)
    {
      if (input.TextParameter.IsNullOrWhiteSpace())
      {
        throw new ArgumentNullException(nameof(input.TextParameter), $"{nameof(input.TextParameter)} cannot be null or whitespace");
      }
    }

    public void BuildRequest(PrintFooterPaymentDetailsRequest input, List<byte> request)
    {
      request.Add(FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb);
      request.Add(FiscalPrinterDividers.R);
      request.Add(EncodingHelper.Encode(SupportedPaymentAndFooterTypes.h.ToString()));

      // no LF needed
      request.Add(EncodingHelper.Encode(input.TextParameter));

      // # - description of the extended format for the footer h:
      // ESC MFB R h<text> LF<parameter1> LF< parameter2 > ESC MFE
      request.Add(FiscalPrinterDividers.Lf, FiscalPrinterDividers.C, FiscalPrinterDividers.Lf, FiscalPrinterDividers.Zero);

      request.Add(FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfe);
    }
  }
}