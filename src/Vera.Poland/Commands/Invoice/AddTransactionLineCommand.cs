using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Vera.Extensions;
using Vera.Poland.Commands.Invoice.Contract;
using Vera.Poland.Extensions;
using Vera.Poland.Models.Requests.Invoice;
using Vera.Poland.Protocol;

namespace Vera.Poland.Commands.Invoice
{
  /// <summary>
  /// Check 1.3.2 Transaction line for more information
  ///
  /// Format
  /// ESC MFB D<product_name> NUL<quantity> [<unit>] <price> LF CR [<comment1>] LF CR [<comment2>] ESC MFB1 a<value> ESC MFB2<VAT> ESC MFE
  ///   or
  /// ESC MFB D<product_name> 0x0A <quantity> [<unit>] <price> 0x0A 0x0D [<comment1>] 0x0A 0x0D [<comment2>] ESC MFB1 a<value> ESC MFB2<VAT> ESC MFE
  ///
  /// Description:
  ///   Verifies the correctness of the tax rate defined for a given product.
  ///   Updates the Control Database of Goods in RAM.
  ///   Prints the transaction line. Increases the value of receipt or invoice accumulators.
  ///   Generates and displays the appropriate message on the customer's display.
  /// Arguments
  ///
  ///   •	< product_name > - the string length must not exceed 56 characters for wide paper and small font, 42 for wide paper and bigger font, 40 for narrow paper and must be finished by a NUL character (0x00) or LF (0x0A). Permitted characters in the name are: 
  ///   • Capital letters A…Z, 
  ///   • Small letters a…z,
  ///   • Polish uppercase and lowercase letters in the selected code page(1250, CP852, MAZOVIA, ISO8859-2), 
  ///   • Digits 0..9, 
  ///   • Characters: dot(.), comma(,), forward slash(\), backward slash(/), percentage(%).
  ///   • Spaces added in the product name are ignored, while the product name containing only space characters(ASCII code 32) is not accepted.
  /// The rules according to which the entry to the commodity database is created and should be taken into account when naming the goods:
  ///   a)  characters:
  ///     space A B C D E F G H I J K L M N O P Q R S T
  ///     U V W X Y Z Ą Ć Ę Ł Ń Ó Ś Ź Ż a b c d e f
  ///     g h i j k l m n o p q r s t u v w x y z ą
  ///     ć ę ł ń ó ś ź ż. ,	\	/	%	0	1	2	3	4	5	6	7
  ///     8	9																			
  ///   are remembered.
  ///   Small and capital letters are treated as the same characters.
  ///   By default, only capital letters are printed,
  ///   it is necessary to set the appropriate parameter (po.rcpt.item_characters, see 4.7.12).  
  /// b)	characters:
  /// !	#	$	’	&	(	)	*	+	:	<	=	>	?	@		_	-	;		
  /// are accepted, but they do not affect the creation of an entry in the commodity database
  /// c)	other characters are changed to "_" or "?"

  ///   Explanation is extended - please check the manual
  /// </summary>
  [SuppressMessage("ReSharper", "InvalidXmlDocComment")]
  [SuppressMessage("ReSharper", "EntityNameCapturedOnly.Local")]
  [SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local")]
  public class AddTransactionLineCommand: IFiscalOrderLineInvoiceCommand<AddTransactionLineRequest>
  {
    private const int MaxUnitCharacters = 8;


    public void BuildRequest(AddTransactionLineRequest input, List<byte> request)
    {
      // Acceptance conditions
      //   •	Receipt is open
      //   •	Receipt is not in the payment phase
      //
      //   Possible further operations
      //   •	Sales line
      //   •	Discount or uplift to the sales line
      //   •	Discount or uplift to the partial sum
      //   •	Footer
      //   •	Settlement of the returnable packaging
      //   •	Conversion or payment with foreign currency
      //   •	Cancellation of the sales line
      //   •	Cancellation of the transaction
      //   •	Sum of the transaction

      var encodedProductName = EncodingHelper.Encode(input.ProductName);
      var encodedQuantity = EncodingHelper.EncodeQuantity(input.Quantity);
      var encodedPrice = EncodingHelper.Encode(input.Price);
      var encodedValue = EncodingHelper.Encode(input.Value);
      var encodedVatClass = input.Vat.EncodeVatClass();

      request.Add(FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb, FiscalPrinterDividers.D);
      request.Add(encodedProductName);
      request.Add(FiscalPrinterDividers.Nul);
      request.Add(encodedQuantity);

      if (!input.Unit.IsNullOrWhiteSpace())
      {
        var encodedUnit = EncodingHelper.Encode(input.Unit);
        request.Add(encodedUnit);
      }

      request.Add(FiscalPrinterDividers.Star);
      request.Add(encodedPrice);

      this.MaybeIncludeComment(request, input.Comment1);
      this.MaybeIncludeComment(request, input.Comment2);

      request.Add(FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb1, FiscalPrinterDividers.a);
      request.Add(encodedValue);
      request.Add(FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb2);
      request.Add(encodedVatClass);
      request.Add(FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfe);
    }

    public void Validate(AddTransactionLineRequest input)
    {
      this.ValidateBaseRequest(input);
      if (input.Unit?.Length > MaxUnitCharacters)
      {
        throw new ArgumentOutOfRangeException(
          nameof(AddTransactionLineRequest.Unit),
          $"Must have less than {MaxUnitCharacters} characters");
      }

      this.ValidateComment(nameof(AddTransactionLineRequest.Comment1), input.Comment1);
      this.ValidateComment(nameof(AddTransactionLineRequest.Comment2), input.Comment2);
    }
  }
}
