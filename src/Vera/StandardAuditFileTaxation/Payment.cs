using System;

namespace Vera.StandardAuditFileTaxation
{
  public sealed class Payment : SourceDocument
  {
    /// <summary>
    /// Unique reference for the payment.
    /// </summary>
    public string Reference { get; set; }

    public DateTime TransactionDate { get; set; }

    /// <summary>
    /// Cheque, bank, Giro, Cash, etc.
    /// <see cref="PaymentTransaction.Type"/>
    /// </summary>
    public string Method { get; set; }

    public string MethodName { get; set; }

    public string Description { get; set; }

    public PaymentLine[] Lines { get; set; }
  }
}