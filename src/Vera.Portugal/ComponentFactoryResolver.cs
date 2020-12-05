using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.IO.Pem;
using Vera.Concurrency;
using Vera.Configuration;
using Vera.Dependencies;
using Vera.Models;
using Vera.Stores;
using PemReader = Org.BouncyCastle.OpenSsl.PemReader;
using PemWriter = Org.BouncyCastle.OpenSsl.PemWriter;

namespace Vera.Portugal
{
    public class ComponentFactoryResolver : AbstractComponentFactoryResolver<Configuration>
    {
        private readonly IInvoiceStore _invoiceStore;
        private readonly ILocker _locker;

        public ComponentFactoryResolver(
            IInvoiceStore invoiceStore,
            ILocker locker,
            IAccountConfigurationProvider accountConfigurationProvider
        ) : base(accountConfigurationProvider)
        {
            _invoiceStore = invoiceStore;
            _locker = locker;
        }

        protected override IComponentFactory Build(Account account, Configuration config)
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