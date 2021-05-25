using Vera.Poland.Models.Enums.Settings;

namespace Vera.Poland.Models.Requests.Settings
{
  public class MutateGraphicSettingsRequest : PrinterRequest
  {
    public FiscalGraphic? Header { get; set; }
    public FiscalGraphic? Footer { get; set; }
    public FiscalGraphic? NonFiscalHeader { get; set; }
  }
}