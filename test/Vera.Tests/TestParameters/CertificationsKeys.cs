using System.Collections.Generic;

namespace Vera.Tests.TestParameters
{
    public class CertificationKeys : TestParameterBase
    {
        public override IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { "PT" };
            yield return new object[] { "NO" };
            // TODO: add Austria after ComponentFactory will be implemented
        }
    }

    public class RegisterOpenStatusCertificationKeys : TestParameterBase
    {
        public override IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { "PT" };
            yield return new object[] { "NO" };
            //TODO(andrei): FiskalyClient needs configuration to work
            //yield return new object[] { "DE" };
        }
    }
}
