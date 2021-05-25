using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Vera.Extensions;
using Vera.Poland.Contracts;
using Vera.Poland.Models.Requests.PredefinedReferencePrintouts;
using Vera.Poland.Protocol;

namespace Vera.Poland.Commands.PredefinedReferencePrintouts
{
  /// <summary>
  /// See:
  ///   1.6 Pre-defined reference printouts
  ///
  /// Format:
  ///   ESC MFB Z<parameter> ESC MFE
  ///
  /// Description:
  ///   Prints a non-fiscal document based on a predefined output format.
  ///
  /// Arguments
  ///   • <parameter> - specifies number of the predefined format, line number and its data.
  ///
  /// Notes:
  ///   The text line must be sent to the printer in the order specified by the output format.
  ///   The printout can be adapted to current needs through the appropriate selection of lines.
   /// </summary>
  [SuppressMessage("ReSharper", "InvalidXmlDocComment")]
  public class PrintPredefinedLinesCommand : IFiscalPrinterCommand<PrintPredefinedLinesRequest>
  {
    public void Validate(PrintPredefinedLinesRequest input)
    {
      // length validation => the printer does not throw any errors, no recovery scenario really
      if (input.Line == null)
      {
        throw new ArgumentNullException(nameof(input.Line), $"{nameof(input.Line)} cannot be null");
      }
      if (input.PatternNumber.IsNullOrWhiteSpace())
      {
        throw new ArgumentNullException(nameof(input.PatternNumber), $"{nameof(input.PatternNumber)} cannot be null or whitespace");
      }
    }

    public void BuildRequest(PrintPredefinedLinesRequest input, List<byte> request)
    {
      // Acceptance conditions
      //  • Neutral state
      //  • open sales period.
      request.Add(FiscalPrinterCommands.Esc);
      request.Add(FiscalPrinterCommands.Mfb);
      request.Add(FiscalPrinterDividers.Z);
      request.AddRange(EncodingHelper.Encode(input.PatternNumber));
      request.AddRange(EncodingHelper.Encode(input.Line));
      if (!string.IsNullOrWhiteSpace(input.ParameterValue))
      {
        request.AddRange(EncodingHelper.Encode(input.ParameterValue));
      }
      request.Add(FiscalPrinterCommands.Esc);
      request.Add(FiscalPrinterCommands.Mfe);
    }
  }

  //Pattern number: 2F
  //01
  //02 POKWITOWANIE ZWROTU
  //10 DATA: ################ <par#>
  //Version 1.2.2 FP210 protocol for online cash register
  //72 Exorigo-Upos S.A.
  //11
  //18 NUMER: ################################# <par#>
  //19 KASJER: @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ <par@>
  //20
  //30 ZWROT TOWARU: @@@@@@@@@@@@@@@@@@@@@@@@@@ <par@>
  //30 ################################# <par#>
  //30
  //40 KWOTA: ################################# <par#>
  //50
  //51 ŚRODKI PŁATNOŚCI:
  //52 @@@@@@@@@@@@@@@@@@@@@@@@@@@@ ######### <par@>, <par#>
  //E0
  //......................................
  //PODPIS
  //E1
  //......................................
  //PODPIS KASJERA
  //E2
  //......................................
  //PODPIS KLIENTA
  //OPIS:
  //- line 10 centered
  //- line 30 can be repeated any number of times
  //- line 52 can be repeated any number of times
}