using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Vera.Poland.Contracts;
using Vera.Poland.Extensions;
using Vera.Poland.Models.Requests.Settings;
using Vera.Poland.Protocol;

namespace Vera.Poland.Commands.Settings
{
  /// <summary>
  /// See 4.7.13  Configuration setup parameters for more information
  ///
  /// Format
  /// ESC MFB % s ESC MFB1<name> LF<value> ESC MFE
  /// </summary>
  [SuppressMessage("ReSharper", "InvalidXmlDocComment")]
  public class MutateSettingCommand : IFiscalPrinterCommand<MutateSettingRequest>
  {
    public void Validate(MutateSettingRequest input)
    {
      if (string.IsNullOrWhiteSpace(input.SettingName))
      {
        throw new ArgumentNullException(nameof(input.SettingName), $"{nameof(input.SettingName)} cannot be null or empty");
      }
      if (string.IsNullOrWhiteSpace(input.SettingValue))
      {
        throw new ArgumentNullException(nameof(input.SettingValue), $"{nameof(input.SettingValue)} cannot be null or empty");
      }
    }


    public void BuildRequest(MutateSettingRequest input, List<byte> request)
    {
      var encodedName = EncodingHelper.Encode(input.SettingName);
      var encodedValue = EncodingHelper.Encode(input.SettingValue);

      request.Add(FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb);
      request.Add(FiscalPrinterCommands.SetSettingCommand);
      request.Add(FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb1);

      request.Add(encodedName);

      request.Add(FiscalPrinterDividers.Lf);

      request.Add(encodedValue);

      request.Add(FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfe);
    }
  }
}
