using Vera.Sweden.Utils;
using Xunit;

namespace Vera.Sweden.Tests.Utils
{
  public class MiscUtilsTests
  {
    [Theory]
    [InlineData(null, "0,00")]
    [InlineData(1, "1,00")]
    [InlineData(1.2, "1,20")]
    [InlineData(12.34, "12,34")]
    [InlineData(12.344, "12,34")]
    [InlineData(12.345, "12,35")]
    [InlineData(12456789.1234, "12456789,12")]
    [InlineData(-1.3, "-1,30")]
    [InlineData(-1239.124, "-1239,12")]
    public void Will_Format_As_Expected(double? inputNumber, string expectedOutput)
    {
      var result = MiscUtils.FormatDecimalWithTwoPlaces((decimal?) inputNumber);

      Assert.Equal(expectedOutput, result);
    }
  }
}