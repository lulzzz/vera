using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Vera.Poland.Commands.Invoice.Contract;
using Vera.Poland.Extensions;
using Vera.Poland.Models.Requests.Invoice;
using Vera.Poland.Protocol;

namespace Vera.Poland.Commands.Invoice
{
  /// <summary>
  /// See 4.3.3  Cancellation of the sales/advance line (storno) for more details
  ///
  /// Description
  ///   Prints a transaction line with a negative value.
  ///   Decreases the value of the corresponding receipt accumulator.
  /// 
  /// Format
  ///   ESC MFB D<product_name> NUL<quantity> * <unit_price> LF CR [<comment1>] LF CR [<comment2>] ESC MFB1 c<-value> ESC MFB2<VAT> ESC MFE
  /// </summary>
  [SuppressMessage("ReSharper", "EntityNameCapturedOnly.Local")]
  [SuppressMessage("ReSharper", "InvalidXmlDocComment")]
  public class CancelTransactionLineCommand: IFiscalOrderLineInvoiceCommand<CancelTransactionLineRequest>
  { 
    public void Validate(CancelTransactionLineRequest input)
    {
      this.ValidateBaseRequest(input);

      this.ValidateComment(nameof(CancelTransactionLineRequest.Comment1), input.Comment1);
      this.ValidateComment(nameof(CancelTransactionLineRequest.Comment2), input.Comment2);
    }

    public void BuildRequest(CancelTransactionLineRequest input, List<byte> request)
    {

      // Acceptance conditions
      //   • Receipt is open
      //   • Receipt is not in the payment phase
      //   • Sales phase
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

      var encodedProductName = EncodingHelper.Encode(input.ProductName);
      var encodedQuantity = EncodingHelper.EncodeQuantity(input.Quantity);
      var encodedPrice = EncodingHelper.Encode(input.Price);
      var encodedValue = EncodingHelper.Encode(input.Value);
      var encodedVatClass = input.Vat.EncodeVatClass();

      request.Add(FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb, FiscalPrinterDividers.D);
      request.Add(encodedProductName);
      request.Add(FiscalPrinterDividers.Nul);
      request.Add(encodedQuantity);
      request.Add(FiscalPrinterDividers.Star);
      request.Add(encodedPrice);

      this.MaybeIncludeComment(request, input.Comment1);
      this.MaybeIncludeComment(request, input.Comment2);

      request.Add(FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb1, FiscalPrinterDividers.c);
      request.Add(FiscalPrinterDividers.Minus);
      request.Add(encodedValue);
      request.Add(FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb2);
      request.Add(encodedVatClass);
      request.Add(FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfe);
    }
  }
}