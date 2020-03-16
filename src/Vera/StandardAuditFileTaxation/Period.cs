using System;

namespace Vera.StandardAuditFileTaxation
{
  public sealed class Period
  {
    public string SystemID { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
  }
}