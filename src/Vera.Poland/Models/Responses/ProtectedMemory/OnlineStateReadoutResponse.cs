namespace Vera.Poland.Models.Responses.ProtectedMemory
{
  public class OnlineStateReadoutResponse : PrinterResponse
  {
    public string JPKID { get; set; }

    public uint? ParsedKpkID { get; set; }
  }
}