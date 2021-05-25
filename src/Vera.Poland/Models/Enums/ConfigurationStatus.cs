using System;

namespace Vera.Poland.Models.Enums
{
  [Flags]
  public enum ConfigurationStatus
  {
    DisplayType = 1 << 1,
    XonXOff = 1 << 3
  }
}