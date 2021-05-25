using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Vera.Poland.Contracts;
using Vera.Poland.Extensions;
using Vera.Poland.Models.Enums;
using Vera.Poland.Models.Responses;
using Vera.Poland.Protocol;

namespace Vera.Poland.Commands
{
  /// <summary>
  /// See 4.13.3 Long status for more info
  ///
  /// Format:
  ///
  ///   ESC + 0X0A
  ///
  /// Answer
  ///   ESC r MSB LSB <data>
  /// where:
  /// ·	<data> -  <status_byte> <configuration_byte> <date, time> 0x30 <receipt_counter> <last_daily_record_date> <last_daily_report_number> <48_chars_coded_ 0x20>
  /// </summary>
  [SuppressMessage("ReSharper", "InvalidXmlDocComment")]
  public class ReadLongStatusQuery : IFiscalPrinterQuery<LongStatusResponse>
  {
    /// <summary>
    /// There are 78 bytes of response we're receiving
    /// </summary>
    private const int ResponseLength = 78;

    private const string PrinterDateAndTimeFormat = "ssmmHHddMMyy";
    private const string DailyReportDateAndTimeFormat = "ddMMyy";

    public void BuildRequest(List<byte> request)
    {
      request.Add(FiscalPrinterCommands.Esc);
      request.Add(FiscalPrinterArguments.LongStatusArgument);
    }

    public LongStatusResponse ReadResponse(byte[] printerRawResponse)
    {
      // We expect a printer response of the following form
      // Example

      // Passing the following request
      // ESC + 0x0A
      // will return the following response
      // ESC 'r' NUL 'J' 0x04 '71424122105210000052105210232'
      // SP  SP  SP  SP  SP  SP  SP  SP  SP  SP  SP  SP  SP
      // SP  SP  SP  SP  SP  SP  SP  SP  SP  SP  SP  SP  SP
      // SP  SP  SP  SP  SP  SP  SP  SP  SP  SP  SP  SP  SP
      // SP  SP  SP  SP  SP
      // the first two bytes that we need to verify are: ESC r
      // we will ignore MSB LSB for now as we only care about the next bytes

      if (printerRawResponse.Length != ResponseLength)
      {
        throw new InvalidOperationException("Expecting response of form: ESC r MSB LSB <status_byte> <configuration_byte> <date, time> 0x30 <receipt_counter> <last_daily_record_date> <last_daily_report_number> <48_chars_coded_ 0x20>");
      }

      // check the first two bytes
      var hasMsb = printerRawResponse[0] == FiscalPrinterCommands.Esc;
      var hasResponse = printerRawResponse[1] == FiscalPrinterResponses.ResponseArgument;
      var responseIsValid = hasMsb && hasResponse;

      if (!responseIsValid)
      {
        return new LongStatusResponse
        {
          Success = false,
          ResponseMalformed = true
        };
      }

      var statusBit = printerRawResponse[4];
      var configurationBit = printerRawResponse[5];

      var statusBitArray = BitConverter.GetBytes(statusBit).ToArray();
      var configurationBitArray = BitConverter.GetBytes(configurationBit).ToArray();

      var statusBits = new BitArray(statusBitArray);
      var configurationBits = new BitArray(configurationBitArray);

      var status = statusBits.GetEnum<LongStatus>();
      var configuration = configurationBits.GetEnum<ConfigurationStatus>();


      var dateAndTime = EncodingHelper.Decode(printerRawResponse[6..18]);
      var dateTime = DateTime.ParseExact(dateAndTime, PrinterDateAndTimeFormat, null);
      var receiptCounter = EncodingHelper.Decode(printerRawResponse[19..24]);
      var lastDailyReportDateString = EncodingHelper.Decode(printerRawResponse[24..30]);
      var lastDailyReportDateTime = DateTime.ParseExact(lastDailyReportDateString, DailyReportDateAndTimeFormat, null);

      var lastDailyReportNumber = EncodingHelper.Decode(printerRawResponse[30..34]);

      return new LongStatusResponse
      {
        Success = true,
        IsFiscalDeviceInReadonlyMode = status.HasFlag(LongStatus.FiscalDeviceInReadonlyMode),
        IsPreFiscal = status.HasFlag(LongStatus.FiscalLevel),
        IsFiscalDayOpen = status.HasFlag(LongStatus.SalePeriod),
        IsReceiptOpen = status.HasFlag(LongStatus.Receipt),
        IsReceiptSummarized = status.HasFlag(LongStatus.ReceiptSummarized),
        IsNonFiscalDocumentPrintoutOngoing = status.HasFlag(LongStatus.NonFiscalDocumentPrintoutOngoing),
        IsDisplayTypeAlphanumeric = configuration.HasFlag(ConfigurationStatus.DisplayType),
        IsXonXoffProtocolOn = configuration.HasFlag(ConfigurationStatus.XonXOff),
        PrinterTime = dateTime,
        ReceiptCounter = receiptCounter,
        LastDailyReportDate = lastDailyReportDateTime,
        LastDailyReportNumber = lastDailyReportNumber
      };
    }
  }
}