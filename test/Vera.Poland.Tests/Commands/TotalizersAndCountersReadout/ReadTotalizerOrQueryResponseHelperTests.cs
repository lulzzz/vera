using System;
using Vera.Poland.Commands.TotalizersAndCountersReadout;
using Vera.Poland.Models.Enums;
using Xunit;

namespace Vera.Poland.Tests.Commands.TotalizersAndCountersReadout
{
  public class ReadTotalizerOrQueryResponseHelperTests
  {
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Totalizer_When_PrinterDataResponse_Is_Null_Or_Whitespace_Will_Throw(string printerDataResponse)
    {
      Assert.Throws<ArgumentNullException>(() => { ReadTotalizerOrQueryResponseHelper.ReadTotalizer(printerDataResponse); });
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Counter_When_PrinterDataResponse_Is_Null_Or_Whitespace_Will_Throw(string printerDataResponse)
    {
      Assert.Throws<ArgumentNullException>(() => { ReadTotalizerOrQueryResponseHelper.ReadCounter(printerDataResponse); });
    }

    /// <summary>
    /// -type--sign-15
    /// </summary>
    [Theory]
    [InlineData("1+000000000012345,10", TotalizerOrCounterType.NonIndex, 12345.10)]
    [InlineData("\u0001+000000000012345,10", TotalizerOrCounterType.NonIndex, 12345.10)]
    [InlineData("2-000000000012345,20", TotalizerOrCounterType.IndexedByTaxCategory, -12345.20)]
    [InlineData("\u0002-000000000012345,20", TotalizerOrCounterType.IndexedByTaxCategory, -12345.20)]
    [InlineData("3+999999999999999,30", TotalizerOrCounterType.NotUsed, 999999999999999.30)]
    [InlineData("\u0003+999999999999999,30", TotalizerOrCounterType.NotUsed, 999999999999999.30)]
    [InlineData("4-999999999999999,40", TotalizerOrCounterType.NotUsedTwo, -999999999999999.40)]
    [InlineData("\u0004-999999999999999,40", TotalizerOrCounterType.NotUsedTwo, -999999999999999.40)]
    [InlineData("5+000000000012345,50", TotalizerOrCounterType.IndexedByPaymentMethod, 12345.50)]
    [InlineData("\u0005+000000000012345,50", TotalizerOrCounterType.IndexedByPaymentMethod, 12345.50)]
    public void Totalizer_Will_Handle_Printer_Response(string printerDataResponse, TotalizerOrCounterType expectedType, double expectedValue)
    {
      var result = ReadTotalizerOrQueryResponseHelper.ReadTotalizer(printerDataResponse);

      Assert.Equal(expectedType, result.Item1);
      Assert.Equal(expectedValue, result.Item2);
    }

    /// <summary>
    /// -type--sign-15
    /// </summary>
    [Theory]
    [InlineData("1+0000012345", TotalizerOrCounterType.NonIndex, 12345)]
    [InlineData("\u0001+0000012345", TotalizerOrCounterType.NonIndex, 12345)]
    [InlineData("2+0000012345", TotalizerOrCounterType.IndexedByTaxCategory, 12345)]
    [InlineData("\u0002+0000012345", TotalizerOrCounterType.IndexedByTaxCategory, 12345)]
    [InlineData("3+9999999999", TotalizerOrCounterType.NotUsed, 9999999999)]
    [InlineData("\u0003+9999999999", TotalizerOrCounterType.NotUsed, 9999999999)]
    [InlineData("4-9999999999", TotalizerOrCounterType.NotUsedTwo, -9999999999)]
    [InlineData("\u0004-9999999999", TotalizerOrCounterType.NotUsedTwo, -9999999999)]
    [InlineData("5+0000012345", TotalizerOrCounterType.IndexedByPaymentMethod, 12345)]
    [InlineData("\u0005+0000012345", TotalizerOrCounterType.IndexedByPaymentMethod, 12345)]
    public void Counter_Will_Handle_Printer_Response(string printerDataResponse, TotalizerOrCounterType expectedType, long expectedValue)
    {
      var result = ReadTotalizerOrQueryResponseHelper.ReadCounter(printerDataResponse);

      Assert.Equal(expectedType, result.Item1);
      Assert.Equal(expectedValue, result.Item2);
    }
  }
}