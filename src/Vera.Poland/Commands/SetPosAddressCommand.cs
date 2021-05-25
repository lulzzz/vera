using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Vera.Poland.Contracts;
using Vera.Poland.Helpers;
using Vera.Poland.Models.Requests;
using Vera.Poland.Protocol;

namespace Vera.Poland.Commands
{
  /// <summary>
  /// See 4.7.7 Point of sale address change for more details
  ///
  /// Description
  ///   The command changes the address of the point of sale.Not all data will be printed in the document header,Format
  /// but they are required in the JPK protocol.
  /// 
  /// Arguments
  ///   •<place>, <tax_ofice>, <street> - the maximum length of the parameter is equal to the maximum number of characters in the line
  ///   • <post_code>, <house_number>, <apartament_number> - maximum length of the parameter is 15 chars. 
  ///
  /// Example
  ///   ESC MFB h ESC MFB1 A<place> LF<tax_office> LF <street> LF<post_code> LF <house_number> [LF<apartment_number>] ESC MFE
  ///
  /// Arguments
  ///  • <place>, <tax_ofice>, <street> - the maximum length of the parameter is equal to the maximum number of characters in the line
  ///  • <post_code>, <house_number>, <apartament_number> - maximum length of the parameter is 15 chars.
  /// </summary>
  /// <returns></returns>
  [SuppressMessage("ReSharper", "InvalidXmlDocComment")]
  public class SetPosAddressCommand : IFiscalPrinterCommand<SetPosAddressRequest>
  {
    /// <summary>
    /// Maximum line characters accepted here provided font/size are not changed.
    /// If they are, we will need to devise a clever way to calculate this.
    ///
    /// TODO: Check this applies
    /// </summary>
    private const int MaximumLineCharacters = 30;

    /// <summary>
    /// Maximum parameter length for postal code, house number, apartment number
    /// </summary>
    private const int MaximumParameterCharacters = 15;

    public void Validate(SetPosAddressRequest input)
    {
      if (input.Place?.Length > MaximumLineCharacters)
      {
        throw new ArgumentOutOfRangeException(nameof(input.Place),
          $"The maximum length of the parameter must be less than {MaximumLineCharacters}");
      }

      if (input.TaxOffice?.Length > MaximumLineCharacters)
      {
        throw new ArgumentOutOfRangeException(nameof(input.TaxOffice),
          $"The maximum length of the parameter must be less than {MaximumLineCharacters}");
      }

      if (input.Street?.Length > MaximumLineCharacters)
      {
        throw new ArgumentOutOfRangeException(nameof(input.Street),
          $"The maximum length of the parameter must be less than {MaximumLineCharacters}");
      }

      if (input.PostalCode?.Length > MaximumParameterCharacters)
      {
        throw new ArgumentOutOfRangeException(nameof(input.PostalCode),
          $"The maximum length of the parameter must be less than {MaximumParameterCharacters}");
      }

      if (input.HouseNumber?.Length > MaximumParameterCharacters)
      {
        throw new ArgumentOutOfRangeException(nameof(input.HouseNumber),
          $"The maximum length of the parameter must be less than {MaximumParameterCharacters}");
      }

      if (input.ApartmentNumber?.Length > MaximumParameterCharacters)
      {
        throw new ArgumentOutOfRangeException(nameof(input.ApartmentNumber),
          $"The maximum length of the parameter must be less than {MaximumParameterCharacters}");
      }
    }

    public void BuildRequest(SetPosAddressRequest input, List<byte> request)
    {
      // Acceptance conditions
      //   • Neutral

      var posAddressEncoded = PosAddressHelper.GetPosAddressEncoded(input, EncodingHelper.Encode);

      request.Add(FiscalPrinterCommands.Esc);
      request.Add(FiscalPrinterCommands.Mfb);
      request.Add(FiscalPrinterDividers.A);

      foreach (var encodedItem in posAddressEncoded)
      {
        request.AddRange(encodedItem);
        request.Add(FiscalPrinterDividers.Lf);
      }

      request.Add(FiscalPrinterCommands.Esc);
      request.Add(FiscalPrinterCommands.Mfe);

    }
  }
}
