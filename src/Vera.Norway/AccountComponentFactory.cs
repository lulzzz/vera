using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System.IO;
using System.Security.Cryptography;
using Vera.Dependencies;
using Vera.Printing;
using Vera.Stores;
using PemReader = Org.BouncyCastle.OpenSsl.PemReader;

namespace Vera.Norway
{
    public class AccountComponentFactory : AbstractAccountComponentFactory<Configuration>
    {
        private readonly IReportStore _reportStore;
        private readonly IPrintAuditTrailStore _printAuditTrailStore;
        private readonly ILoggerFactory _loggerFactory;

        public AccountComponentFactory(IReportStore reportStore, IPrintAuditTrailStore printAuditTrailStore, ILoggerFactory loggerFactory)
        {
            _reportStore = reportStore;
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
                var keyPair = (AsymmetricCipherKeyPair)reader.ReadObject();

                var rsaParameters = DotNetUtilities.ToRSAParameters(keyPair.Private as RsaPrivateCrtKeyParameters);

                rsa = RSA.Create(rsaParameters);
            }

            return new ComponentFactory(rsa, config, _reportStore, _printAuditTrailStore, _loggerFactory);
        }

        public override string Name => "NO";
    }
}
