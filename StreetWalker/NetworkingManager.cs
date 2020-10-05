using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace StreetWalker
{
    class NetworkingManager
    {
        private HttpClient client;

        public const string SERVER_URL = "https://overpass.kumi.systems/api/interpreter";

        public NetworkingManager()
        {
            client = new HttpClient(new WinHttpHandler());
        }

        public async Task<WalkerResponse> MakeRequest(string body)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(SERVER_URL),
                Content = new StringContent(body, Encoding.UTF8)
            };

            var response = await client.SendAsync(request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return WalkerResponse.FromJson(responseBody);
        }
    }
}
