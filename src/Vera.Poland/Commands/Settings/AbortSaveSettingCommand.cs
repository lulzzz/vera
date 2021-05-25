using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Vera.Poland.Contracts;
using Vera.Poland.Extensions;
using Vera.Poland.Protocol;

namespace Vera.Poland.Commands.Settings
{
  /// <summary>
  /// See 4.7.14  Configuration save for more details
  ///
  /// Format
  ///   ESC MFB % c ESC MFB1<parameter> ESC MFE
  /// </summary>
  [SuppressMessage("ReSharper", "InvalidXmlDocComment")]
  public class AbortSaveSettingCommand: IFiscalPrinterCommand
  {
    public void BuildRequest(List<byte> request)
    {
      request.Add(FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb);
      request.Add(FiscalPrinterCommands.SaveSettingCommand);
      request.Add(FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb1);
      request.Add(FiscalPrinterParameters.AbortSaveSetting);
      request.Add(FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfe);
    }
  }
}