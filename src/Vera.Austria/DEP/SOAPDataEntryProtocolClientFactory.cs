using Microsoft.Extensions.Logging;

namespace Vera.Austria.DEP
{
    public sealed class SOAPDataEntryProtocolClientFactory : IDataEntryProtocolClientFactory
    {
        private readonly ILogger<SOAPDataEntryProtocolClientFactory> _log;
        private readonly Configuration _configuration;
        
        public SOAPDataEntryProtocolClientFactory(ILoggerFactory loggerFactory, Configuration configuration)
        {
            _log = loggerFactory.CreateLogger<SOAPDataEntryProtocolClientFactory>();
            _configuration = configuration;
        }

        public IDataEntryProtocolClient Create()
        {
            var host = _configuration.DEPHost;
            var username = _configuration.DEPUsername;
            var password = _configuration.DEPPassword;

            return new SOAPDataEntryProtocolClient(host, username, password, _log);
        }
    }
}