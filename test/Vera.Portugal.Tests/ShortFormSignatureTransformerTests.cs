using System;
using Xunit;

namespace Vera.Portugal.Tests
{
    public class ShortFormSignatureTransformerTests
    {
        [Fact]
        public void Should_return_correct_zipped_signature()
        {
            var transformer = new ShortFormSignatureTransformer();
            var got = transformer.Transform("R2sB3JRSFGWZJ5vM4s14R2sB3JRSFGWZ");

            Assert.Equal("RWRW", got);
        }

        [Fact]
        public void Should_throw_when_signature_is_null_or_empty()
        {
            var transformer = new ShortFormSignatureTransformer();

            Assert.Throws<NullReferenceException>(() => transformer.Transform(null));
            Assert.Throws<NullReferenceException>(() => transformer.Transform(string.Empty));
        }
    }
}