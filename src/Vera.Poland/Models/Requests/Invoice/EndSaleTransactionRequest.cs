using System;

namespace Vera.Poland.Models.Requests.Invoice
{
  public class EndSaleTransactionRequest : PrinterRequest
  {
    public bool IsInvoice { get; set; }
    public DateTime? SaleDate { get; set; } // In the case with invoice, it is possible to enter: „Sale date”: dd-mm-yyyy
  }
}