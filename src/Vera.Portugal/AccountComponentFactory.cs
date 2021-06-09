using System.IO;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Vera.Dependencies;
using Vera.Portugal.Stores;
using Vera.Printing;
using Vera.Stores;
using PemReader = Org.BouncyCastle.OpenSsl.PemReader;

namespace Vera.Portugal
{
    public class AccountComponentFactory : AbstractAccountComponentFactory<Configuration>
    {
        private readonly IWorkingDocumentStore _wdStore;
        private readonly IPrintAuditTrailStore _printAuditTrailStore;
        private readonly ILoggerFactory _loggerFactory;

        public AccountComponentFactory(IWorkingDocumentStore wdStore, IPrintAuditTrailStore printAuditTrailStore, ILoggerFactory loggerFactory)
        {
            _wdStore = wdStore;
            _printAuditTrailStore = printAuditTrailStore;
            _loggerFactory = loggerFactory;
        }

        protected override IComponentFactory Create(Configuration config)
        {
            RSA rsa = null;

            if (!string.IsNullOrEmpty(config.PrivateKey))
            {
                var privateKey = config.PrivateKey;

                using var sr = new StringReader(privateKey);
                var reader = new PemReader(sr);
                var keyPair = (AsymmetricCipherKeyPair) reader.ReadObject();

                var rsaParameters = DotNetUtilities.ToRSAParameters(keyPair.Private as RsaPrivateCrtKeyParameters);

                rsa = RSA.Create(rsaParameters);
            }

            return new ComponentFactory(rsa, config, _wdStore, _printAuditTrailStore, _loggerFactory);
        }

        public override string Name => "PT";
    }
}
