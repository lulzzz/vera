using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Vera.Extensions;
using Vera.Poland.Contracts;
using Vera.Poland.Models.Requests;
using Vera.Poland.Protocol;

namespace Vera.Poland.Commands
{
  /// <summary>
  /// See 4.7.6 Taxpayer name setup for more information
  ///
  /// Format
  ///   ESC MFB h[DLE] <line_1>… [LF<line_n>] ESC MFE
  ///
  ///Arguments
  ///   • <line_n> - text parameter, which can be formatted by adding the byte at the beginning of the appropriate value:
  ///   o Normal text – no text format(allign to left)
  ///     Example: „ESC MFE h Exorigo-Upos sp.z o.o.ESC MFE”
  ///     Spację należy wpisać jako ‘ ‘ lub w kodzie ASCII: 0x20
  ///   o Simplified formatting: in the first position of the character there is a formatted byte:
  ///     ▪ Bit 0 – double height (value 1)
  ///     ▪ Bit 1 – double width(value 2)
  ///     ▪ Bit 2 – bold(value 4)
  ///     The above bits can be used together (summed), so the value of the first byte can be the maximum of 7.
  ///     Example: <line_n>:
  ///       0x02 <line_01> - double width,
  ///       0x03 <line_02> - double height and double width.
  ///   o The extended line format with the DLE code in the first position and the formatted byte in the second position, after that, text is located:
  ///     ▪ Bit 0 - no formatting
  ///     ▪ Bit 1 - double width (value 2)
  ///     ▪ Bit 2 - bold(value 4)
  ///     ▪ Bit 3 - underlining(value 8)
  ///     ▪ Bit 4 - negative(value 16)
  ///     ▪ Bit 5 - tilt(value 32)
  ///     ▪ Bit 6 – centered(value 64)
  ///     ▪ Bit 7 – to right(wartość 128)
  ///   The simultaneous setting of bits 6 and 7 (center + to right) causes an error.Not all types of formatting are available for every printing mechanism.
  ///   Example::
  ///     ESC 0x03 <line_01> - double height and double width,
  ///     ESC 0x13 <line_03> - double height and double width in negative
  ///     ESC 0x51 <line_04> - double height in negative, centered
  ///
  /// </summary>
  [SuppressMessage("ReSharper", "InvalidXmlDocComment")]
  public class SetTaxpayerCommand : IFiscalPrinterCommand<SetTaxpayerNameRequest>
  {
    /// <summary>
    /// Maximum number of accepted tax payer lines
    /// </summary>
    private const int MaximumTaxpayerLines = 6;

    public void Validate(SetTaxpayerNameRequest input)
    {
      if (input.TaxpayerLines == null || !input.TaxpayerLines.Any())
      {
        throw new ArgumentOutOfRangeException(
          nameof(SetTaxpayerNameRequest.TaxpayerLines),
          $"Taxpayer lines empty");
      }

      if (input.TaxpayerLines.Count > MaximumTaxpayerLines)
      {
        throw new ArgumentOutOfRangeException(
          nameof(SetTaxpayerNameRequest.TaxpayerLines),
          $"Taxpayer lines must be at most {MaximumTaxpayerLines}");
      }
    }

    public void BuildRequest(SetTaxpayerNameRequest input, List<byte> request)
    {
      IEnumerable<byte[]> GetTaxPayerLines()
      {
        var encodedTaxPayerLines = input.TaxpayerLines
          .Where(line => !line.IsNullOrWhiteSpace())
          .Select(EncodingHelper.Encode).ToList();

        return encodedTaxPayerLines;
      }


      request.Add(FiscalPrinterCommands.Esc);
      request.Add(FiscalPrinterCommands.Mfb);

      request.Add(FiscalPrinterDividers.h);

      foreach (var encodedLine in GetTaxPayerLines())
      {
        request.AddRange(encodedLine);

        // we divide each line by linefeed
        //
        request.Add(FiscalPrinterDividers.Lf);
      }

      request.Add(FiscalPrinterCommands.Esc);
      request.Add(FiscalPrinterCommands.Mfe);

      // Acceptance conditions
      //   • Neutral
    }
  }
}
