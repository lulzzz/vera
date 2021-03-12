using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Vera.Austria.DEP
{
    public sealed class SOAPDataEntryProtocolClient : IDataEntryProtocolClient
    {
        private readonly ILogger<SOAPDataEntryProtocolClientFactory> _log;

        private readonly rksv_depSoapClient _client;
        private readonly NetworkCredential _credentials;

        public SOAPDataEntryProtocolClient(string host, string username, string password, ILogger<SOAPDataEntryProtocolClientFactory> log)
        {
            _log = log;
            var uri = new Uri(host);

            Binding binding = uri.Scheme == "http" ? new BasicHttpBinding() : new BasicHttpsBinding();

            _client = new rksv_depSoapClient(binding, new EndpointAddress(host));
            
            _credentials = new NetworkCredential
            {
                UserName = username,
                Password = password
            };
        }

        public async Task Push(string entry, string signedEntry)
        {
            _log.LogInformation("Pushing receipt to the DEP..");
            
            var response = await _client.AddAsync(_credentials, entry, signedEntry);
            var result = response.Body.AddResult;

            _log.LogInformation($"Response from DEP state={result.ResultState}, description={result.Description}");

            if (result.ResultState != State.Success)
            {
                throw new DEPException(response.Body.AddResult.ResultState, response.Body.AddResult.Description);
            }
        }
    }
}