using Fiskaly;
using Fiskaly.Client.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using static Vera.Germany.Constants;

namespace Vera.Germany
{
    public class FiskalyClient
    {
        private readonly string _fiskalyApiKey;
        private readonly string _fiskalyApiSecret;
        private readonly string _baseUrl;

        private readonly FiskalyHttpClient _client;

        public FiskalyClient(string fiskalyApiKey, string fiskalyApiSecret, string baseUrl)
        {
            _fiskalyApiKey = fiskalyApiKey;
            _fiskalyApiSecret = fiskalyApiSecret;
            _baseUrl = baseUrl;

            _client = new FiskalyHttpClient(_fiskalyApiKey, _fiskalyApiSecret, _baseUrl);
        }

        public string Authenticate()
        {
            var model = new
            {
                api_key = _fiskalyApiKey,
                api_secret = _fiskalyApiSecret
            };
            var requestBody = CreateRequestBody(model);
            var response = _client.Request(RequestMethod.POST, "auth", requestBody, null, null);
            var responseDictionary = ReadResponseBody(response);

            return (string) responseDictionary["access_token"];
        }

        private byte[] CreateRequestBody(object model)
        {
            var json = JsonConvert.SerializeObject(model);

            return Encoding.UTF8.GetBytes(json);
        }

        private Dictionary<string, object> ReadResponseBody(FiskalyHttpResponse response)
        {
            var decodedBody = Encoding.UTF8.GetString(response.Body);
            return JsonConvert
                .DeserializeObject<Dictionary<string, object>>(decodedBody);
        }
    }
}
