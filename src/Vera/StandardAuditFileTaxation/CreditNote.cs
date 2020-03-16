namespace Vera.StandardAuditFileTaxation
{
  public sealed class CreditNote
  {
    /// <summary>
    /// Reference to the original invoice.
    /// </summary>
    public string Reference { get; set; }

    /// <summary>
    /// Reason or rationale.
    /// </summary>
    public string Reason { get; set; }

    /// <summary>
    /// Signature of the original invoice.
    /// NOTE: This is not a SAF-T field.
    /// </summary>
    public string Signature { get; set; }
  }
}