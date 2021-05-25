using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Vera.Poland.Contracts;
using Vera.Poland.Models.Requests;
using Vera.Poland.Protocol;

namespace Vera.Poland.Commands
{
  /// <summary>
  ///
  /// 4.7.10 Invoice EURONIP printout
  /// 
  /// Format
  ///   ESC MFB g E<0 | 1> ESC MFE
  /// Description
  ///   Enables EURONIP field on invoice printout
  /// Arguments
  ///   • <0 | 1> - 1 – enable, 0 - disable
  /// Acceptance conditions
  ///   All printouts finished.
  /// </summary>
  [SuppressMessage("ReSharper", "CommentTypo")]
  public class ToggleEuroNipCommand : IFiscalPrinterCommand<ToggleRequest>
  {
    public void Validate(ToggleRequest input)
    {
    }

    public void BuildRequest(ToggleRequest input, List<byte> request)
    {
      request.Add(FiscalPrinterCommands.Esc);
      request.Add(FiscalPrinterCommands.Mfb);
      request.Add(FiscalPrinterDividers.g);
      request.Add(FiscalPrinterDividers.E);

      request.AddRange(EncodingHelper.Encode(input.FeatureEnabled ? 1 : 0));

      request.Add(FiscalPrinterCommands.Esc);
      request.Add(FiscalPrinterCommands.Mfe);
    }
  }
}