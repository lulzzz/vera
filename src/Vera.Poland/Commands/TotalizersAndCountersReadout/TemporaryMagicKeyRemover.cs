namespace Vera.Poland.Commands.TotalizersAndCountersReadout
{
  /// <summary>
  /// Temporary solution until we know all the counters and totalizers that
  /// we need, until then a simple enum is used
  /// </summary>
  public static class TemporaryMagicKeyRemover
  {
    private const string TemporaryMagicKey = "DOTKEY";

    public static string RemoveMagicKey(string enumName)
    {
      return enumName.Replace(TemporaryMagicKey, ".");
    }
  }
}