namespace Vera.StandardAuditFileTaxation
{
  public enum DebitCreditIndicator
  {
    Debit,
    Credit
  }

  public static class DebitCreditIndicatorExtensions
  {
    public static DebitCreditIndicator ToDebitCreditIndicator(this decimal d)
    {
      return d > 0 ? DebitCreditIndicator.Credit : DebitCreditIndicator.Debit;
    }
  }
}