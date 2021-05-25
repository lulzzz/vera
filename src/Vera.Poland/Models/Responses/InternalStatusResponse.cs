using Vera.Poland.Models.Enums;

namespace Vera.Poland.Models.Responses
{
  public class InternalStatusResponse : PrinterResponse
  {
    public InternalStatusWord1 Word1 { get; set; }
    public InternalStatusWord2 Word2 { get; set; }
    public InternalStatusWord3 Word3 { get; set; }
    public InternalStatusWord4 Word4 { get; set; }
    public InternalStatusWord5 Word5 { get; set; }
    public InternalStatusWord6 Word6 { get; set; }
    public InternalStatusWord7 Word7 { get; set; }
    public InternalStatusWord8 Word8 { get; set; }
    public InternalStatusWord9 Word9 { get; set; }
  }
}