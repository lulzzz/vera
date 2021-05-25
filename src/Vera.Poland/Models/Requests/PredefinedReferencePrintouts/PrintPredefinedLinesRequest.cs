namespace Vera.Poland.Models.Requests.PredefinedReferencePrintouts
{
  public class PrintPredefinedLinesRequest : PrinterRequest
  {
    public string PatternNumber { get; set; }
    public string Line { get; set; }
    public string ParameterValue { get; set; }
  }
}