using System.Diagnostics.CodeAnalysis;
using Vera.Poland.Models.Responses;

namespace Vera.Poland.Models
{
  /// <summary>
  /// See 1.11.7 Internal error readout for more information
  ///
  /// Format
  ///   ESC + e
  ///
  /// Answer
  ///   ESC r MSB LSB<internal_error>
  /// where:
  /// • <internal_error> - internal error code(2 bytes).
  /// </summary>
  [SuppressMessage("ReSharper", "InvalidXmlDocComment")]
  public class PrinterError: PrinterResponse
  {
    /// <summary>
    /// Internal error code returned by internal error readout
    /// </summary>
    public int Code { get; set; }

    /// <summary>
    /// Error description returned by internal error readout
    /// </summary>
    public string ErrorDescription { get; set; }

    /// <summary>
    /// The raw error description kept for debugging/logging purposes
    /// </summary>
    public string? RawErrorDescription { get; set; }
  }
}