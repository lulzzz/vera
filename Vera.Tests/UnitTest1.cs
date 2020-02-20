using System;
using Xunit;

namespace Vera.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var c = new Class1();
            c.X = 100;

            Assert.Equal(100, c.X);
        }
    }
}
