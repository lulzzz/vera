namespace Vera.Poland.Models.Requests.Settings
{
  public class MutateDrawerSettingsRequest: PrinterRequest
  {
    public int? FirstDrawerOpeningPulse { get; set; }
    public int? SecondDrawerOpeningPulse { get; set; }
  }
}