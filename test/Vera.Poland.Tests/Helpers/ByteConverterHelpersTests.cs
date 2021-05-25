using System;
using Xunit;

namespace Vera.Poland.Tests.Helpers
{
  public class ByteConverterHelpersTests
  {
    [Theory]
    [InlineData("dd-MM-yy")]
    [InlineData("MM-yy")]
    public void Will_Format_Date(string format)
    {
      var dateTime = DateTime.Now;

      var convertedDateBytes = EncodingHelper.ConvertDateToBytes(dateTime, format);

      var testDateString = DateTime.Now.ToString(format);
      var testDateBytes = EncodingHelper.Encode(testDateString);
      for (var i = 0; i < convertedDateBytes.Length; i++)
      {
        Assert.Equal(convertedDateBytes[i], testDateBytes[i]);
      }
      Assert.Equal(EncodingHelper.Decode(testDateBytes), EncodingHelper.Decode(convertedDateBytes));
    }
  }
}