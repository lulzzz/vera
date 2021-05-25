namespace Vera.Poland.Models.Requests.Settings
{
  public class MutateSettingRequest: PrinterRequest
  {
    public string SettingName { get; set; }
    public string SettingValue { get; set; }
  }
}
