using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Vera.Poland.Contracts;
using Vera.Poland.Models.Requests;
using Vera.Poland.Protocol;

namespace Vera.Poland.Commands
{
  /// <summary>
  ///
  /// See 4.7.4 Registration currency setup for more info
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
  public class SetCurrencyCommand : IFiscalPrinterCommand<SetCurrencyRequest>
  {
    public void Validate(SetCurrencyRequest input)
    {
    }

    public void BuildRequest(SetCurrencyRequest input, List<byte> request)
    {
      static bool TryGetDateAndTimeOutOfDateTime(DateTime? date, out (string date, string time) dateAndTimeTuple)
      {
        if (!date.HasValue)
        {
          dateAndTimeTuple = (null, null);
          return false;
        }

        var datePart = date.Value.ToString("dd-MM-yy");
        string timePart = null;

        if (date.Value.Hour > 0)
        {
          timePart = $"{date.Value.Hour:00}-{date.Value.Minute:00}";
        }

        dateAndTimeTuple = (datePart, timePart);
        return true;
      }

      // Description
      //   Changes the registration currency in the printer. The change can be carried out an infinite number of times for up to three different currencies.
      //
      //   Note:
      // The command with date and time is always in semicolons; if the command is sent only with the date,
      // semicolons do not apply. No date and time parameter results in immediate denomination: ESC MFB KE LF EUR LF -2 ESC MFE.
      //
      // The parameter: “-2” signed number of decimal places(round to 0.01); this parameter is not required.

      // Example with date and time
      // ESC MFB KE30-07-19 SP 09:31 LF EUR LF -2 ESC MFE

      // Example without date and time
      // ESC MFB KE LF EUR LF -2 ESC MFE

      var converted = TryGetDateAndTimeOutOfDateTime(input.Date, out (string datePart, string timePart) dateAndTime);

      request.Add(FiscalPrinterCommands.Esc);
      request.Add(FiscalPrinterCommands.Mfb);
      request.AddRange(FiscalPrinterDividers.Ke);

      // If we could convert the datetime (meaning one was passed)
      // we will write the date and time part
      //
      if (converted)
      {
        var encodedDatePart = EncodingHelper.Encode(dateAndTime.datePart);
        request.AddRange(encodedDatePart);

        if (dateAndTime.timePart != null)
        {
          request.Add(FiscalPrinterDividers.Sp);

          var encodedTimePart = EncodingHelper.Encode(dateAndTime.timePart);
          request.AddRange(encodedTimePart);
        }
      }

      request.Add(FiscalPrinterDividers.Lf);

      var encodedCurrencyCode = EncodingHelper.Encode(input.CurrencyCode);
      request.AddRange(encodedCurrencyCode);

      request.Add(FiscalPrinterDividers.Lf);
      // -2 parameter which is the precision or signed number of decimal places is not required
      request.Add(FiscalPrinterCommands.Esc);
      request.Add(FiscalPrinterCommands.Mfe);


      // Acceptance conditions
      // • Neutral or not initialized
      // • Post fiscalization for change with date and time parameters
    }
  }
}
