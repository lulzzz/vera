using Vera.Poland.Models.Enums;

namespace Vera.Poland.Models.Requests
{
  public class SetLineDisplayRequest : PrinterRequest
  {
    public LineDisplayType Type { get; set; }

    public string? Text { get; set; }
  }
}