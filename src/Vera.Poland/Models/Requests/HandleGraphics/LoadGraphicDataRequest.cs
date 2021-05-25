using System.Collections.Generic;

namespace Vera.Poland.Models.Requests.HandleGraphics
{
  public class LoadGraphicDataRequest: PrinterRequest
  {
    public List<byte> ImageDataBytes { get; set; }
  }
}