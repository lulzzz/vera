using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System.IO;
using System.Security.Cryptography;
using Vera.Dependencies;
using PemReader = Org.BouncyCastle.OpenSsl.PemReader;

namespace Vera.Norway
{
    public class AccountComponentFactory : AbstractAccountComponentFactory<Configuration>
    {
        protected override IComponentFactory Create(Configuration config)
        {
            RSA rsa = null;

            if (!string.IsNullOrEmpty(config.PrivateKey))
            {
                var privateKey = config.PrivateKey;

                using var sr = new StringReader(privateKey); 
                var reader = new PemReader(sr);
                var keyPair = (AsymmetricCipherKeyPair)reader.ReadObject();

                var rsaParameters = DotNetUtilities.ToRSAParameters(keyPair.Private as RsaPrivateCrtKeyParameters);

                rsa = RSA.Create(rsaParameters);
            }

            return new ComponentFactory(rsa, config);
        }

        public override string Name => "NO";
    }
}
