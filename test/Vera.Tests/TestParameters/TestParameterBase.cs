using System.Collections.Generic;
using System.Collections;

namespace Vera.Tests.TestParameters
{
    public abstract class TestParameterBase : IEnumerable<object[]>
    {
        public abstract IEnumerator<object[]> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
