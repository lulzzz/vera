using System;

namespace Vera.Poland.Models.Requests
{
  public class SetDateRequest : PrinterRequest
  {
    public DateTime Date { get; set; }
  }
}