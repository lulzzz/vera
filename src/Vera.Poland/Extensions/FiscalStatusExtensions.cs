using Vera.Poland.Models.Enums;

namespace Vera.Poland.Extensions
{
  public static class FiscalStatusExtensions
  {
    public static bool IsFiscalMemoryFull(this FiscalStatus status)
    {
      return status.HasFlag(FiscalStatus.FiscalMemoryFull);
    }

    public static bool IsFiscalMemoryAlmostFull(this FiscalStatus status)
    {
      return status.HasFlag(FiscalStatus.FiscalMemoryAlmostFull);
    }
  }
}
