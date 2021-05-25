using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Vera.Poland.Contracts;
using Vera.Poland.Models.Requests;
using Vera.Poland.Protocol;

namespace Vera.Poland.Commands
{
  /// <summary>
  /// See 4.7.1 Sets date and time for more detailed information
  /// Format
  ///  ESC MFB I<time_date> ESC MFE
  ///
  /// Arguments
  /// <time_date> - format: ss, mm, gg; DD, MM, RR
  /// </summary>
  [SuppressMessage("ReSharper", "InvalidXmlDocComment")]
  public class SetDateCommand: IFiscalPrinterCommand<SetDateRequest>
  {
    /// <summary>
    /// Format according to section 1.7.1
    /// </summary>
    private const string DateFormat = "ss,mm,HH;dd,MM,yy";


    public void Validate(SetDateRequest input)
    {
      // Nothing to validate for now
    }

    public void BuildRequest(SetDateRequest input, List<byte> request)
    {
      var encodedDate = EncodingHelper.ConvertDateToBytes(input.Date, DateFormat);

      request.Add(FiscalPrinterCommands.Esc);
      request.Add(FiscalPrinterCommands.Mfb);
      request.Add(FiscalPrinterDividers.I);

      request.AddRange(encodedDate);

      request.Add(FiscalPrinterCommands.Esc);
      request.Add(FiscalPrinterCommands.Mfe);

      // Acceptance conditions
      //   • Neutral or not initialized
      //   • The lower limit of setting the date and time is the time stamp for last record saved to the fiscal memory.
      //   • The time can be set in total by  2 hours in the neutral state
      //   • Date and time can be set any later than the last entry in the fiscal memory of the printer if the printer is in uninitialized state
      //   • It is possible to change the date to an earlier date than the last closed fiscal day and the printer will allow the registration of sales to start,
      //     but will not allow closing the fiscal day and printing the periodic report to a time that is late than the last daily report.
      //   • Sending a command without a parameter will call the current time check with the NTP time servers - it requires a connection to the Internet.
    }
  }
}
