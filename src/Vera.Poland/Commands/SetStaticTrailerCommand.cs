using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Vera.Extensions;
using Vera.Poland.Contracts;
using Vera.Poland.Extensions;
using Vera.Poland.Models.Requests;
using Vera.Poland.Protocol;

namespace Vera.Poland.Commands
{
  /// <summary>
  /// See 4.7.18 Static trailer for more details
  ///
  /// Format
  ///   ESC MFB t S<line_1>[LF<line_2>…LF<line_10>] ESC MFE
  ///
  /// Description
  ///   Defines text, displayed in fiscal document footer.
  ///
  /// Arguments
  ///   • <line_01> … <line _10>
  ///     lines automatically printed in footers, saved in RAM,
  ///     each line consists of 3 parameters: <position><font_format><text>:
  ///
  ///     <position = L | R | C>
  ///     L – to left
  ///     R – to right
  ///     C – centered
  ///     <font_format> = <0 | 1 | 2 | 3 | 4>
  ///     0 – normal
  ///     1 – double height
  ///     2 – double width
  ///     3 – double height and width
  ///     4 – bold
  ///     <text> - text, empty string disables static trailer.
  ///     <#> - prints barcode, according to command in chapter 1.3.22 <bc_string>
  ///     <#+L> - prints graphics according to chapter 1.5.1
  /// </summary>
  [SuppressMessage("ReSharper", "InvalidXmlDocComment")]
  public class SetStaticTrailerCommand : IFiscalPrinterCommand<SetStaticTrailerRequest>
  {
    private const int StaticTrailerLinesMaxCount = 10;

    public void Validate(SetStaticTrailerRequest input)
    {
      if (input == null)
        throw new ArgumentNullException(nameof(input));

      if (input.Lines == null)
        throw new ArgumentNullException(nameof(SetStaticTrailerRequest.Lines));

      if (input.Lines.Count >= StaticTrailerLinesMaxCount)
      {
        throw new ArgumentOutOfRangeException(nameof(SetStaticTrailerRequest.Lines));
      }
    }

    public void BuildRequest(SetStaticTrailerRequest input, List<byte> request)
    {

      // Acceptance conditions
      //  Closed fiscal day


      List<string> PaddedList()
      {
        var result = new List<string>();
        result.AddRange(input.Lines);

        if (result.Count < StaticTrailerLinesMaxCount)
        {
          result.AddRange(Enumerable.Repeat("", StaticTrailerLinesMaxCount - result.Count));
        }

        return result;
      }

      void AddLine(string line)
      {
        var encodedLine = EncodingHelper.Encode(line);

        request.Add(!line.IsNullOrWhiteSpace() ? FiscalPrinterDividers.C : FiscalPrinterDividers.L, Convert.ToByte('0'));

        if (!line.IsNullOrWhiteSpace())
        {
          request.Add(encodedLine);
        }
        
        request.Add(FiscalPrinterDividers.Lf);
      }

      request.Add(FiscalPrinterCommands.Esc);
      request.Add(FiscalPrinterCommands.Mfb);
      request.AddRange(FiscalPrinterDividers.tS);
      
      foreach (var line in PaddedList())
      {
        AddLine(line);
      }

      request.Add(FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfe);
    }
  }
}