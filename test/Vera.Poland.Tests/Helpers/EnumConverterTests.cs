using System.Collections;
using System.Linq;
using Vera.Poland.Extensions;
using Vera.Poland.Models.Enums;
using Vera.Poland.Utils;
using Xunit;
using static Assertive.DSL;

namespace Vera.Poland.Tests.Helpers
{
  public class EnumConverterTests
  {
    [Fact]
    public void Sanity_Test()
    {
      var byteArray = new byte[]
      {
        0x00, 0x1F, 0x21, 0x1A
      };

      var bitArray = new BitArray(byteArray);
      var fiscalStatus = bitArray.GetEnum<FiscalStatus>();
      var byteResult = EnumConverter<FiscalStatus>.GetByteArrayRepresentation(fiscalStatus);

      var arrayEqual = byteArray.SequenceEqual(byteResult);

      Assert(() => arrayEqual);
    }
  }
}