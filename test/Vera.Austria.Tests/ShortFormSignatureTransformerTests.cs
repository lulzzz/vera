using Xunit;

namespace Vera.Austria.Tests
{
    public class ShortFormSignatureTransformerTests
    {
        [Fact]
        public void Should_return_correct_zipped_signature()
        {
            const string jwt =
            "eyJhbGciOiJFUzI1NiJ9." +
            "X1IxLUFUMF9ERU1PLUNBU0gtQk9YNTI0XzM2NjU5Nl8yMDE1LTEyLTE3VDExOjIzOjQ0XzAsMDBfMCwwMF8zLDY0Xy0yLDYwXzEsNzlfVkZKQl80N2JlNzM3Y2IxZjZkMWYxX1p2TnhKdzZhMUE0PQ." +
            "J7YC28zquHfHzMpx02TqElbXOTSgXQu5JAA9Xu1Xzzu5p8eUYT-sgmyhzRps5nYyEp5Yh8ATIa9130zmuiACHw";

            const string expected =
              "_R1-AT0" +
              "_DEMO-CASH-BOX524" +
              "_366596" +
              "_2015-12-17T11:23:44" +
              "_0,00" +
              "_0,00" +
              "_3,64" +
              "_-2,60" +
              "_1,79" +
              "_VFJB" +
              "_47be737cb1f6d1f1" +
              "_ZvNxJw6a1A4=" +
              "_J7YC28zquHfHzMpx02TqElbXOTSgXQu5JAA9Xu1Xzzu5p8eUYT+sgmyhzRps5nYyEp5Yh8ATIa9130zmuiACHw==";


            var transformer = new ShortFormSignatureTransformer();
            var got = transformer.Transform(jwt);

            Assert.Equal(expected, got);
        }
    }
}
