using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Vera.Sweden.InfrasecHttpClient.Contracts;
using Vera.Sweden.Utils;

namespace Vera.Sweden.InfrasecHttpClient
{
    public class InfrasecClient : IInfrasecClient
    {
        private readonly HttpClient _httpClient;

        public InfrasecClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<HttpResponseMessage> SendInfrasecEnrollmentRequest<T>(T requestBody)
        {
            const string mediaType = "application/json";

            var requestBodyJson = JsonConvert.SerializeObject(requestBody);

            var request = CreatePostRequest(requestBodyJson, mediaType);

            var response = await _httpClient.SendAsync(request);

            return response;
        }

        public async Task<HttpResponseMessage> SendInfrasecReceiptRequest<T>(T requestBody)
        {
            const string mediaType = "application/xml";

            var requestBodyXml = InfrasecXmlSerializerHelper.SerializeXml(requestBody);

            var request = CreatePostRequest(requestBodyXml, mediaType);

            var response = await _httpClient.SendAsync(request);

            return response;
        }
        
        private HttpRequestMessage CreatePostRequest(string requestBodyJson, string mediaType)
        {
            var content = new StringContent(requestBodyJson, Encoding.UTF8, mediaType);
            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress)
            {
                Content = content
            };

            request.Content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
            request.Headers.Add("user-agent", "eva/1.0.0");
            return request;
        }
    }
}