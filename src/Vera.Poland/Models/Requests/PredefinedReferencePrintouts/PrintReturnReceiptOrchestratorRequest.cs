using System.Collections.Generic;

namespace Vera.Poland.Models.Requests.PredefinedReferencePrintouts
{
  public class PrintReturnReceiptOrchestratorRequest : PrinterRequest
  {
    public string Date { get; set; }// DATA / Date
    public string Number { get; set; }  // NUMER / Number
    public string Cashier { get; set; } // KASJER / Cashier
    public string TotalAmount { get; set; }  // KWOTA / Amount / Total amount paid
    public List<PredefinedPrintoutContentModel> Products { get; set; } // ZWROT TOWARU / Return / Product related information, 2 parameters
    public List<PredefinedPrintoutContentModel> PaymentTypes { get; set; }// ŚRODKI PŁATNOŚCI / Means of Payment / PaymentType
  }
}