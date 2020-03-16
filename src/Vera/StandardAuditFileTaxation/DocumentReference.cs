namespace Vera.StandardAuditFileTaxation
{
  public sealed class DocumentReference
  {
    /// <summary>
    /// Type of the document.
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// Reference number of the document.
    /// </summary>
    public string Number { get; set; }

    /// <summary>
    /// Line number of the document.
    /// </summary>
    public string Line { get; set; }
  }
}