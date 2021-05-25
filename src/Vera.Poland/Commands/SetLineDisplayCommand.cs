using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Vera.Poland.Contracts;
using Vera.Poland.Models.Enums;
using Vera.Poland.Models.Requests;
using Vera.Poland.Protocol;

namespace Vera.Poland.Commands
{
  /// <summary>
  /// See 4.10.8 Line display for mode detailed information
  ///
  /// Format
  ///   ESC MFB G ESC MFB1 <line_number> <text> ESC MFE
  ///
  /// Arguments
  ///   •	<line_number>
  ///   •	0 (ASCII code 0x30) first line,
  ///   •	1 (ASCII code 0x31) second line,
  ///   •	3 (ASCII code 0x33) – the first 7 out of 20 characters are replaced by the text RESZTA and are displayed in the second line
  ///      of the alphanumeric display.
  ///   •	<text> - string max [20] chars
  ///   •	D (ASCII code 0x44) date and time display,
  ///   •	T (ASCII code 0x54) time display.
  /// </summary>
  [SuppressMessage("ReSharper", "InvalidXmlDocComment")]
  public class SetLineDisplayCommand : IFiscalPrinterCommand<SetLineDisplayRequest>
  {
    private const int MaxLength = 20;

    private readonly string _empty = Enumerable.Repeat(" ", MaxLength).Aggregate(string.Concat);

    public void Validate(SetLineDisplayRequest input)
    {
      if (input.Type == default)
      {
        throw new ArgumentOutOfRangeException(nameof(SetLineDisplayRequest.Type), "Choose line one or two");
      }

      if (input.Text?.Length > MaxLength)
      {
        throw new ArgumentOutOfRangeException(nameof(SetLineDisplayRequest.Text));
      }
    }

    public void BuildRequest(SetLineDisplayRequest input, List<byte> request)
    {
      // Notes
      //   The text line sent to the display must be formatted by the software application.
      // If the length of the string written to the alphanumeric display exceeds 20 characters, the string is truncated.
      // In order to delete a given display line, send a sequence of 20 space characters to the appropriate line (ASCII code 0x20).

      byte lineIdentifier = input.Type switch
      {
        LineDisplayType.FirstLine => 0x30,
        LineDisplayType.SecondLine => 0x31,
        _ => throw new ArgumentOutOfRangeException(nameof(input.Type))
      };

      request.Add(FiscalPrinterCommands.Esc);
      request.Add(FiscalPrinterCommands.Mfb);
      request.Add(FiscalPrinterDividers.G);

      request.Add(FiscalPrinterCommands.Esc);
      request.Add(FiscalPrinterCommands.Mfb1);

      request.Add(lineIdentifier);
      request.AddRange(EncodingHelper.Encode(input.Text ?? _empty));

      request.Add(FiscalPrinterCommands.Esc);
      request.Add(FiscalPrinterCommands.Mfe);

      // Acceptance conditions
      //  Neutral state
      //  Sale period opened.
    }
  }
}