using System.Collections.Generic;

namespace Vera.Poland.Models.Requests
{
  public class SetStaticTrailerRequest : PrinterRequest
  {
    /// <summary>
    /// Maximum lines count is 10
    /// </summary>
    public List<string> Lines { get; set; }
  }
}