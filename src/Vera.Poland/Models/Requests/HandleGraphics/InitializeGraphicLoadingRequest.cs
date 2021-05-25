using Vera.Poland.Models.Enums;

namespace Vera.Poland.Models.Requests.HandleGraphics
{
  public class InitializeGraphicLoadingRequest : PrinterRequest
  {
    public uint Width { get; set; } // (pixels) – max 360
    public uint Height { get; set; } // (pixels) - max 256
    public GraphicNumber? Graphic { get; set; } // the number of the defined image (from 1 to 8)
    /// <summary>
    /// from 1 to 99 where:
    ///   1 – black and white image printed from inverted bitmap data
    ///   2...99 – reserved for future use
    /// </summary>
    public uint Colour => 1;

    public string Name { get; set; }
  }
}