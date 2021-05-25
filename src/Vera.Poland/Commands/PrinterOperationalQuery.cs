using System.Collections.Generic;
using System.Linq;
using Vera.Poland.Contracts;
using Vera.Poland.Models.Enums;
using Vera.Poland.Models.Responses;

namespace Vera.Poland.Commands
{
  /// <summary>
  /// Check if the if the fiscal printer is operational by querying the <see cref="ReadExtendedStatusQuery">extended status</see>
  /// </summary>
  public class PrinterOperationalQuery: IFiscalPrinterQuery<PrinterOperationalResponse>
  {
    private readonly ReadExtendedStatusQuery _readExtendedStatusQuery;

    public PrinterOperationalQuery()
    {
      _readExtendedStatusQuery = new ReadExtendedStatusQuery();
    }

    public PrinterOperationalResponse ReadResponse(byte[] printerRawResponse)
    {
      var extendedResponse = _readExtendedStatusQuery.ReadResponse(printerRawResponse);

      if (!extendedResponse.Success)
      {
        return new PrinterOperationalResponse
        {
          IsPrinterOperational = false,
          ResponseMalformed = extendedResponse.ResponseMalformed,
          Success = false
        };
      }

      var isPrinterOperational = IsPrinterOperational(extendedResponse);

      return new PrinterOperationalResponse
      {
        Success = true,
        IsPrinterOperational = isPrinterOperational
      };
    }

    public void BuildRequest(List<byte> request)
    {
      _readExtendedStatusQuery.BuildRequest(request);
    }

    /// <summary>
    /// We check if the printer is healthy by looking at both fiscal status and printer mechanism status
    ///
    /// Can be overriden at will
    /// </summary>
    private static bool IsPrinterOperational(ExtendedStatusResponse extendedStatus)
    {
      var fiscalStatus = extendedStatus.FiscalStatus;
      var printerMechanism = extendedStatus.PrinterMechanismStatus;

      //
      // Fiscal status is good to continue if the printer has memory left, is not busy, ram is not erased and
      // there's no periodical document printing in progress
      //

      var fiscalStatusOk = !ProvideBlackListedFiscalStatuses()
        .Any(blackListedFiscalStatus => fiscalStatus.HasFlag(blackListedFiscalStatus));

      //
      // Printer mechanism is good to continue if we don't have cutter errors, drawer open, no paper of firmware error
      //
      //

      var printerMechanismStatusOk = !ProvideBlackListedPrinterMechanismStatuses().Any(
        blackListedPrinterMechanismStatus => printerMechanism.HasFlag(blackListedPrinterMechanismStatus));

      var printerMechanismIsReady = printerMechanism.HasFlag(PrinterMechanismStatus.PrinterMechanismIsReady);

      return printerMechanismIsReady
             && fiscalStatusOk
             && printerMechanismStatusOk;
    }

    /// <summary>
    /// Provides a list of fiscal statuses considered a no-go for our current command
    ///
    /// Can override at will
    /// </summary>
    private static IEnumerable<FiscalStatus> ProvideBlackListedFiscalStatuses()
    {
      var blackList = new List<FiscalStatus>
      {
        FiscalStatus.FiscalMemoryFull,
        FiscalStatus.PrinterBusy,
        FiscalStatus.RtcError,
        FiscalStatus.RamErased,
        FiscalStatus.NonFiscalDocumentPrintingInProgress,
        FiscalStatus.PeriodicalReportPrintingInProgress
      };

      return blackList;
    }

    /// <summary>
    /// Provides a list of printer mechanism statuses considered a no-go for our current command
    ///
    /// Can override at will
    /// </summary>
    private static IEnumerable<PrinterMechanismStatus> ProvideBlackListedPrinterMechanismStatuses()
    {
      var blackList = new List<PrinterMechanismStatus>
      {
        PrinterMechanismStatus.IsPrinterCoverClosed,
        PrinterMechanismStatus.CutterError,
        PrinterMechanismStatus.PaperEndSensor,
        PrinterMechanismStatus.DrawerOpenSignal,
        PrinterMechanismStatus.PrintMechanismFirmwareCrcError
      };

      return blackList;
    }
  }
}