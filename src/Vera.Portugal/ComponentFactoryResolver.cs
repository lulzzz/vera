using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using Vera.Concurrency;
using Vera.Dependencies;

namespace Vera.Portugal
{
    public class ComponentFactoryResolver : IComponentFactoryResolver
    {
        private readonly ILocker _locker;

        public ComponentFactoryResolver(ILocker locker)
        {
            _locker = locker;
        }

        public IComponentFactory Resolve(Account account)
        {
            RSA rsa;

            var config = account.GetConfiguration<Configuration>();
            var privateKey = config.PrivateKey;

            using (var sr = new StringReader(Encoding.ASCII.GetString(privateKey)))
            {
                var reader = new PemReader(sr);
                var keyPair = (AsymmetricCipherKeyPair) reader.ReadObject();

                var rsaParameters = DotNetUtilities.ToRSAParameters(keyPair.Private as RsaPrivateCrtKeyParameters);

                rsa = RSA.Create(rsaParameters);
            }

            return new ComponentFactory(_locker, rsa);
        }

        public string Name => "PT";
    }
}