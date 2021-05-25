using System;
using System.Diagnostics.CodeAnalysis;

namespace Vera.Poland.Models.Requests
{
  /// <summary>
  ///
  /// See 1.7.4 Registration currency setup for more info
  /// 
  /// Format
  ///   ESC MFB K E ‘[<date>] [‘ ‘<hour>]’ LF<currency>[LF - 2] ESC MFE
  ///
  /// Arguments
  ///
  ///   • <date> - format dd-mm-yy.No paper changes the currency instantly.If the date has been given,
  ///   the change will be made automatically, as soon as the given day conditions are met: closed sales period, all printouts closed.
  ///   • <hour> - format hh-mm.Sets the time of change.No parameter is 00:00 as default.
  ///   • <currency> - three-letter currency code: PLN, EUR, e.t.c.
  /// </summary>
  [SuppressMessage("ReSharper", "InvalidXmlDocComment")]
  public class SetCurrencyRequest : PrinterRequest
  {
    public string CurrencyCode { get; set; }

    public DateTime? Date { get; set; }
  }
}