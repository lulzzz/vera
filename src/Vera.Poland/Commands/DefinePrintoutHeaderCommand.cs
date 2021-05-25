using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Vera.Poland.Contracts;
using Vera.Poland.Models.Requests;
using Vera.Poland.Protocol;

namespace Vera.Poland.Commands
{
  /// <summary>
  /// 4.7.6. Printout header defining
  ///
  /// Format:
  ///   ESC MFB h<line_1> LF<line_2>(LF<line_n>) ESC MFE
  ///   Description:
  ///   Defines the print header in battery backed up RAM.Headline can be up to 9 lines.Because the NIP number on the printouts is printed automatically, it is not advisable to define it in the header.
  ///   Notes:
  ///   In the header, it is possible to change the attributes of the print, as below.
  ///   Arguments:
  ///     <line_n>
  ///     A) Line without formatting attributes(left aligned); plain text
  ///     Example:
  ///       'Exorigo-Upos sp. Z o.o.'
  ///     B) Simplified formatting: The first character position contains a formatting byte:
  ///       • Bit 0 - double height(value 1),
  ///       • Bit 1 - double width(value 2),
  ///       • Bit 2 - Bold type(4).
  ///   The above bits can be lit together(summed), so the value of the first byte can be up to 7.
  ///   Example:s:
  ///     <Line_n>
  ///       0x02 'line 1' - double line width 'line 1'
  ///       0x03 'line 2' - line 2 text with double width and height.
  ///     C) Extended line format with ESC code in first position and formatting byte in second position, followed
  ///       by proper text.
  ///       • Bit 0 - double height(value 1),
  ///       • Bit 1 - double width(value 2),
  ///       • Bit 2 - Bold type(value 4),
  ///       • Bit 3 - underline(value 8),
  ///       • Bit 4 - negative(value 16),
  ///       • Bit 5 - tilt(value 32),
  ///       • Bit 6 - alignment(value 64),
  ///       • Bit 7 - align right(value 128).
  ///   If bits 6 and 7 are not set, then alignment is left by default. Not all types of formatting are available for each printing mechanism.
  ///   Example:s:
  ///     ESC 0x03 'line 2' - line 2 text with double width and height,
  ///     ESC 0x13 'line 3' - line 3 text with double width and height in the negative,
  ///     ESC 0x51 'line 4' - double line, line 4 text, negative, centered.
  ///   Terms of acceptance
  ///     The printer must be in a neutral state.
  /// 
  /// </summary>
  [SuppressMessage("ReSharper", "InvalidXmlDocComment")]
  public class DefinePrintoutHeaderCommand: IFiscalPrinterCommand<DefinePointOfSaleHeaderRequest>
  {
    /// <summary>
    /// Maximum line characters accepted here provided font/size are not changed.
    /// If they are, we will need to devise a clever way to calculate this.
    ///
    /// </summary>
    private const int MaximumHeaderLines = 9;

    /// <summary>
    /// 
    /// TODO: Check a reasonable value for this - currently the documentation gives us different values for different commands so this is confusing atm
    ///
    /// </summary>
    private const int MaximumHeaderLineLength = 36;

    /// <summary>
    /// Value used to tell the printer that the text is centered and bold. Used for this command only
    /// </summary>
    private const byte BoldCenter = 0x64;

    public void Validate(DefinePointOfSaleHeaderRequest input)
    {
      if (input?.Lines == null || input.Lines.Count == 0)
      {
        throw new ArgumentNullException(nameof(input.Lines));
      }

      if (input.Lines.Count > MaximumHeaderLines)
      {
        throw new ArgumentOutOfRangeException(nameof(input.Lines));
      }

      foreach (var line in input.Lines.Where(line => line.Length > MaximumHeaderLineLength))
      {
        throw new ArgumentOutOfRangeException(nameof(line));
      }
    }

    public void BuildRequest(DefinePointOfSaleHeaderRequest input, List<byte> request)
    {
      request.Add(FiscalPrinterCommands.Esc);
      request.Add(FiscalPrinterCommands.Mfb);
      request.Add(FiscalPrinterDividers.h);

      if (input.Lines.Count == 1)
      {
        AppendFormattedLine(input.Lines.First(), request);
      }
      else
      {
        for (var i = 0; i < input.Lines.Count - 1; i++)
        {
          AppendFormattedLine(input.Lines[i], request);
          request.Add(FiscalPrinterDividers.Lf);
        }

        AppendFormattedLine(input.Lines.Last(), request);
      }

      request.Add(FiscalPrinterCommands.Esc);
      request.Add(FiscalPrinterCommands.Mfe);
    }

    private static void AppendFormattedLine(string line, List<byte> request)
    {
      request.Add(FiscalPrinterCommands.Dle);

      // This is in order to center the line
      //
      request.Add(BoldCenter);
      request.AddRange(EncodingHelper.Encode(line));
    }
  }
}