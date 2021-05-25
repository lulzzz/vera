using System;

namespace Vera.Poland.Models.Enums
{
  /// <summary>
  /// 1.11.5 Fiscal Module internal status readout
  /// </summary>
  [Flags]
  public enum InternalStatusWord1 : long
  {
    None = 0,

    //
    // Word 1
    //

    FiscalMemoryDamaged = 1 << 0,
    RtcDefective = 1 << 1,
    ManufacturerJumperInActivePosition = 1 << 2,
    ServiceJumperOn = 1 << 3,

    /// <summary>
    /// Documentation not really clear
    /// </summary>
    DrawerOneOpened = 1 << 5,
    
    DrawerTwoOpened = 1 << 7,
    RamContentsAreInvalidated = 1 << 8,
    RtcDateTimeNotSet = 1 << 9,
    TotalizersDataDeleted = 1 << 10,
    CustomerDisplayDisconnected = 1 << 11,
    BatteryPowerSupply = 1 << 13,
    BatteryVoltageLow = 1 << 14,
    ExternalPowerSupplyVoltage = 1 << 15
  }
}