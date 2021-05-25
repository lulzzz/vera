namespace Vera.Poland.Models.Printer
{
  /// <summary>
  /// See 1.8.13. Properties readout
  /// 
  /// Format:
  ///  ESC MFB L c <query> ESC MFE
  ///
  /// Arguments
  ///  query - properties from below
  /// </summary>
  public static class PropertyReadoutQueryConstants
  {
    // Is this a test version; 1 - yes, 0 - no
    public const string SoftwareDebug = "software.debug";
    // Firmware version of the fiscal module (eg 1.05.5222)
    public const string SoftwareVersion = "software.version";
  }
}