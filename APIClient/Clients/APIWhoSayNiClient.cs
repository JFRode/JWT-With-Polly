using API.Commons;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Polly;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace APIClient.Clients
{
    public class APIWhoSayNiClient : IAPIWhoSayNiClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private static string AuthenticationToken = "";

        public APIWhoSayNiClient(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string> Get(CancellationToken cancellationToken)
        {
            var policy = CreateTokenRefreshPolicy(cancellationToken);

            var response = await policy.ExecuteAsync(context =>
            {
                var client = _httpClientFactory.CreateClient("APIWhoSayNi");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthenticationToken);
                return client.GetAsync("api");
            }, cancellationToken);

            string apiResponse = await response.Content.ReadAsStringAsync();

            return apiResponse;
        }

        public async Task<string> GetAuthenticationToken(CancellationToken cancellationToken)
        {
            var client = _httpClientFactory.CreateClient("APIWhoSayNi");

            var content = new StringContent(JsonConvert.SerializeObject(CommonConstants.SecurityKey), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("token", content);

            string apiResponse = await response.Content.ReadAsStringAsync();

            var token = JObject.Parse(apiResponse)
                .GetValue("token")
                .ToString();

            return token;
        }

        public async Task<string> RefreshAuthenticationToken(CancellationToken cancellationToken)
        {
            var token = await GetAuthenticationToken(cancellationToken);
            AuthenticationToken = token;
            return token;
        }

        private IAsyncPolicy<HttpResponseMessage> CreateTokenRefreshPolicy(CancellationToken cancellationToken)
        {
            var policy = Policy
                .HandleResult<HttpResponseMessage>(message => message.StatusCode == HttpStatusCode.Unauthorized)
                .RetryAsync(1, async (result, retryCount, context) =>
                    await RefreshAuthenticationToken(cancellationToken));

            return policy;
        }
    }
}