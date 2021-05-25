using System.Collections.Generic;

namespace Vera.Poland.Models.Requests.HandleGraphics
{
  public class LoadGraphicChunkDataRequest : PrinterRequest
  {
    public List<byte> ImageChunkBytes { get; set; }
  }
}