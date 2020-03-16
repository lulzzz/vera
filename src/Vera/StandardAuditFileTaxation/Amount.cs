namespace Vera.StandardAuditFileTaxation
{
  public sealed class Amount
  {
    public Amount() { }

    public Amount(decimal value)
    {
      Value = decimal.Round(value, 4);
    }

    public Amount(string foreignCurrencyCode, decimal foreignCurrencyAmount, decimal foreignExchangeRate)
    {
      ForeignCurrencyCode = foreignCurrencyCode;
      ForeignCurrencyAmount = decimal.Round(foreignCurrencyAmount, 4);
      ForeignExchangeRate = decimal.Round(foreignExchangeRate, 4);
    }

    /// <summary>
    /// Value of the amount in the currency of the header.
    /// <see cref="Header.DefaultCurrencyCode"/>.
    /// </summary>
    public decimal Value { get; set; }

    /// <summary>
    /// ISO 4217.
    /// </summary>
    public string ForeignCurrencyCode { get; set; }
    public decimal ForeignCurrencyAmount { get; set; }
    public decimal ForeignExchangeRate { get; set; }

    public override string ToString()
    {
      return $"{nameof(Value)}: {Value:F}";
    }
  }
}