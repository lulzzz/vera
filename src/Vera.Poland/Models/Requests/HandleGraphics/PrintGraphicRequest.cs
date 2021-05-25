using Vera.Poland.Models.Enums;

namespace Vera.Poland.Models.Requests.HandleGraphics
{
  public class PrintGraphicRequest: PrinterRequest
  {
    public GraphicNumber? Graphic { get; set; } 
  }
}