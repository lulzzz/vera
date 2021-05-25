namespace Vera.Poland.Models.Requests.Settings
{
  public class MutateAudioSettingsRequest : PrinterRequest
  {
    public int? AudioVolume { get; set; }
    public int? AudioKeyVolume { get; set; }
  }
}