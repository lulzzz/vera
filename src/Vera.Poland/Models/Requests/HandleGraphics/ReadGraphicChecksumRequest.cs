using Vera.Poland.Models.Enums;

namespace Vera.Poland.Models.Requests.HandleGraphics
{
  public class ReadGraphicChecksumRequest: PrinterRequest
  {
    public GraphicNumber? Graphic { get; set; }
  }
}