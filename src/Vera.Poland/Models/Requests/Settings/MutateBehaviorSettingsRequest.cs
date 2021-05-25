using Vera.Poland.Models.Enums.Settings;

namespace Vera.Poland.Models.Requests.Settings
{
  public class MutateBehaviorSettingsRequest : PrinterRequest
  {
    public PrinterLanguage? Language { get; set; }

    public PrinterPaper? Paper { get; set; }

    /// <summary>
    /// Specifies how many days before the obligatory service review,
    ///   the printer will start printing information about it
    ///   (after printing the daily report)
    ///
    ///   0 - no information
    ///   1 - 31 - number of days
    /// </summary>
    public int? ServicePeriodWarning { get; set; }

    public TurnOnOrOff? CancelPrintoutsAfterCableDisconnected { get; set; }

    public TurnOnOrOff? CancelPrintoutsAfterPowerFailure { get; set; }

    public TurnOnOrOff? BeepOnError { get; set; }
  }
}