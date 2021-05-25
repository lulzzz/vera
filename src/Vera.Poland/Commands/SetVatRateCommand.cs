using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Vera.Poland.Contracts;
using Vera.Poland.Helpers;
using Vera.Poland.Models;
using Vera.Poland.Models.Requests;
using Vera.Poland.Protocol;

namespace Vera.Poland.Commands
{
  /// <summary>
  /// See 1.7.3 Sets VAT tax rates for more detailed information
  ///
  /// Format
  ///  ESC MFB K D<VAT_rates> ESC MFE
  ///
  /// Arguments
  ///  • <VAT_rates> - gives VAT rates in the format 'aaaabbbbccccddddeeeeffffgggg',
  ///   where: aaaa, ..., gggg these are VAT rates in categories A..G.VAT rates must be expressed as a percentage,
  ///   multiplied by 100 (eg: 23.00% must be sent as '2300'). The n scralled rate is passed as '????'. The string '====' defines the exempt category.
  /// </summary>
  [SuppressMessage("ReSharper", "InvalidXmlDocComment")]
  public class SetVatRateCommand: IFiscalPrinterCommand<SetVatRequest>
  {
    public void Validate(SetVatRequest input)
    {
      GuardNotNull(input.A, nameof(input.A));
      GuardNotNull(input.B, nameof(input.B));
      GuardNotNull(input.C, nameof(input.C));
      GuardNotNull(input.D, nameof(input.D));
      GuardNotNull(input.E, nameof(input.E));
      GuardNotNull(input.F, nameof(input.F));
      GuardNotNull(input.G, nameof(input.G));
    }

    public void BuildRequest(SetVatRequest input, List<byte> request)
    {
      byte[] GetVatEncoded()
      {
        var vatRateBuilder = new StringBuilder();

        var rates = new[]
        {
          input.A,
          input.B,
          input.C,
          input.D,
          input.E,
          input.F,
          input.G
        };

        foreach (var vatRate in rates)
        {
          vatRateBuilder.Append(VatRateHelper.GetVatValue(vatRate));
        }

        var vatData = vatRateBuilder.ToString();
        return EncodingHelper.Encode(vatData);
      }

      request.Add(FiscalPrinterCommands.Esc);
      request.Add(FiscalPrinterCommands.Mfb);
      request.AddRange(FiscalPrinterDividers.Kd);
      request.AddRange(GetVatEncoded());

      request.Add(FiscalPrinterCommands.Esc);
      request.Add(FiscalPrinterCommands.Mfe);

      // Acceptance conditions
      //   • Neutral
      //   • Not initialized
    }

    public void GuardNotNull(VatItem item, string propertyName)
    {
      var propertyIsInitialized = item != default;

      if (!propertyIsInitialized)
      {
        throw new ArgumentNullException(propertyName);
      }
    }
  }
}
