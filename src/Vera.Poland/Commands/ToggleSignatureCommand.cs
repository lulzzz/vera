using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Vera.Poland.Contracts;
using Vera.Poland.Models.Requests;
using Vera.Poland.Protocol;

namespace Vera.Poland.Commands
{
  /// <summary>
  /// See 4.7.11 Invoice customer signature field printout for more details
  /// 
  /// Format
  ///   ESC MFB g P<0 | 1> ESC MFE
  ///
  /// Arguments
  ///  • <0 | 1> - 1 – enable, 0 - disable
  /// </summary>
  [SuppressMessage("ReSharper", "InvalidXmlDocComment")]
  public class ToggleSignatureCommand : IFiscalPrinterCommand<ToggleRequest>
  {

    public void Validate(ToggleRequest input)
    {
    }

    public void BuildRequest(ToggleRequest input, List<byte> request)
    {

      // Description
      //   Enables customer signature field on invoice.

      request.Add(FiscalPrinterCommands.Esc);
      request.Add(FiscalPrinterCommands.Mfb);

      request.AddRange(FiscalPrinterDividers.Gp);

      request.AddRange(EncodingHelper.Encode(input.FeatureEnabled));

      request.Add(FiscalPrinterCommands.Esc);
      request.Add(FiscalPrinterCommands.Mfe);
      // Acceptance conditions
      //   • All printouts finished.
    }
  }
}
