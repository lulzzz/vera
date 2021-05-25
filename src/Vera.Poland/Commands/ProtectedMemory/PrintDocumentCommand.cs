using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Vera.Poland.Contracts;
using Vera.Poland.Models.Requests.ProtectedMemory;
using Vera.Poland.Protocol;

namespace Vera.Poland.Commands.ProtectedMemory
{
  /// <summary>
  /// 1.10.2 Print document from Protected Memory
  ///
  /// Format:
  ///   ESC MFB HEJ<JPKID_number> ESC MFE
  ///
  /// Description:
  ///   Prints the json document with the given JPK number. According to the fiscal requirements, the printout from the
  /// protected memory does not resemble the original printout. Command can be sent many times.
  ///
  /// Arguments:
  ///   • <JPKID_number> - integer parameter
  /// </summary>
  [SuppressMessage("ReSharper", "InvalidXmlDocComment")]
  public class PrintDocumentCommand : IFiscalPrinterCommand<PrintDocumentRequest>
  {
    public void Validate(PrintDocumentRequest input)
    {

    }

    public void BuildRequest(PrintDocumentRequest input, List<byte> request)
    {
      request.Add(FiscalPrinterCommands.Esc);
      request.Add(FiscalPrinterCommands.Mfb);

      request.Add(FiscalPrinterDividers.H);
      request.Add(FiscalPrinterDividers.E);
      request.Add(FiscalPrinterDividers.J);
      request.AddRange(EncodingHelper.Encode(input.JPKID));

      request.Add(FiscalPrinterCommands.Esc);
      request.Add(FiscalPrinterCommands.Mfe);
    }
  }
}