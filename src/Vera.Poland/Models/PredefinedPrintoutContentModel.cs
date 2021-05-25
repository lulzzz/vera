namespace Vera.Poland.Models
{
  /// <summary>
  /// See 1.6.5 Formats of predefined printouts
  /// Parameter is handled by the command (see PrintReturnReceiptCommand)
  /// </summary>
  public class PredefinedPrintoutContentModel
  {
    public string NumericParameter { get; set; } // Numeric parameter - digits + up to 3 letters
    public string TextParameter { get; set; } // Text parameter - letters + Polish diacritics + up to 3 digits
  }
}