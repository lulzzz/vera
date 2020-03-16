namespace Vera.StandardAuditFileTaxation
{
  /// <summary>
  /// Contains bank account information. Either use the IBAN or all the other properties.
  /// </summary>
  public sealed class BankAccount
  {
    /// <summary>
    /// International back account number according to: ISO 13616.
    /// </summary>
    public string IBAN { get; set; }

    public string AccountNumber { get; set; }
    public string AccountName { get; set; }

    /// <summary>
    /// Identifier for the bank branch where the account is being hold. May be needed to uniquely identify the account.
    /// </summary>
    public string SortCode { get; set; }
  }
}