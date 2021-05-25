namespace Vera.Poland.Protocol
{
  public class FiscalPrinterSettings
  {
    // SMALL FONT IS DEFAULT and cannot be set by means of user authorization (only SERVICE)

    public static class Invoices
    {
      public const string CopyCount = "invoice.copy_count";
    }


    public static class Communication
    {
      public const string UsbAProtocol = "usb.a.prot";
      public const string CodePage = "code_page";
      public const string Fp210ProtocolEnabled = "net.prot.fp210a.enabled";
      public const string NetIp = "net.ip";
      public const string NetGateway = "net.gateway";
      public const string Fp210ProtocolPort = "net.prot.fp210a.port";
    }

    public static class Behavior
    {
      public const string CancelPrintoutsAfterCableDisconnected = "po.canc_after.cable_discon";
      public const string CancelPrintoutsAfterPowerFailure = "po.canc_after.pwr";
      public const string ShellLanguage = "langen.shell";
      public const string PrinterLanguage = "langen.prn";
      public const string PrinterPaper = "printer.paper";

      public const string BeepOnError = "cmd.beep.on.error";
      public const string ServicePeriodWarning = "service.period.warning";
    }

    public static class Drawers
    {
      /// <summary>
      /// Setting the duration of the opening pulse of the first drawer. Unit = 1ms (50-500ms)
      /// </summary>
      public const string FirstDrawerOpeningPulse = "cd.1.pulse";

      /// <summary>
      /// Setting the duration of the opening pulse of the Second drawer. Unit = 1ms (50-500ms)
      /// </summary>
      public const string SecondDrawerOpeningPulse = "cd.2.pulse";
    }

    public static class Audio
    {
      /// <summary>
      /// Volume - only in modules with a loudspeaker. Unit = 0 - 100
      /// </summary>
      public const string AudioVolume = "audio.volume";

      /// <summary>
      /// Volume - only in modules with a loudspeaker. Unit = 0 - 100
      /// </summary>
      public const string AudioKeyVolume = "audio.key_volume";
    }

    public static class Graphics
    {
      public const string FiscalHeader = "graph.bhead_fiscal";
      public const string FiscalFooter = "graph.foot_fiscal";
      public const string NonFiscalHeader = "graph.bhead_nonfiscal";
    }
  }
}