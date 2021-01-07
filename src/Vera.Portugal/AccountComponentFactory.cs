using System.IO;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Vera.Concurrency;
using Vera.Configuration;
using Vera.Dependencies;
using Vera.Stores;
using PemReader = Org.BouncyCastle.OpenSsl.PemReader;

namespace Vera.Portugal
{
    public class AccountComponentFactory : AbstractAccountComponentFactory<Configuration>
    {
        private readonly IInvoiceStore _invoiceStore;
        private readonly ILocker _locker;

        public AccountComponentFactory(
            IInvoiceStore invoiceStore,
            ILocker locker
        )
        {
            _invoiceStore = invoiceStore;
            _locker = locker;
        }

        protected override IComponentFactory Create(Configuration config)
        {
            RSA rsa;

            var privateKey = config.PrivateKey;

            using (var sr = new StringReader(privateKey))
            {
                var reader = new PemReader(sr);
                var keyPair = (AsymmetricCipherKeyPair) reader.ReadObject();

                var rsaParameters = DotNetUtilities.ToRSAParameters(keyPair.Private as RsaPrivateCrtKeyParameters);

                rsa = RSA.Create(rsaParameters);
            }

            return new ComponentFactory(_invoiceStore, _locker, rsa, config);
        }

        public override string Name => "PT";
    }
}