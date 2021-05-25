using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Vera.Poland.Contracts;
using Vera.Poland.Models.Requests;
using Vera.Poland.Protocol;

namespace Vera.Poland.Commands
{
  /// <summary>
  /// See 4.10.2 Open drawer
  /// 
  /// Format:
  ///   ESC MFB X <t1> <t2> [<ns>] ESC MFE
  /// 
  /// Description:
  ///   The command opens the selected drawer.
  /// 
  /// Arguments:
  ///   - <t1>, <t2> - ignored
  ///   - <ns> - drawer number (1 or 2, default 1).
  /// </summary>
  [SuppressMessage("ReSharper", "InvalidXmlDocComment")]
  public class OpenCashDrawerCommand : IFiscalPrinterCommand<OpenCashDrawerRequest>
  {
    public void Validate(OpenCashDrawerRequest input)
    {
    }

    public void BuildRequest(OpenCashDrawerRequest input, List<byte> request)
    {
      request.Add(FiscalPrinterCommands.Esc);
      request.Add(FiscalPrinterCommands.Mfb);
      request.Add(FiscalPrinterDividers.X);

      request.Add(FiscalPrinterDividers.Zero);
      request.Add(FiscalPrinterDividers.Zero);

      request.AddRange(EncodingHelper.Encode((int)input.DrawerToOpen));

      request.Add(FiscalPrinterCommands.Esc);
      request.Add(FiscalPrinterCommands.Mfe);
    }
  }
}
