using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using ProxiCall.Web.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ProxiCall.Web.Services.MsGraph
{
    public class MsGraphClient
    {
        private readonly MsGraphConfig msGraphConfig;

        public MsGraphClient(IOptions<MsGraphConfig> msGraphConfig)
        {
            this.msGraphConfig = msGraphConfig.Value;
        }

        public async Task<string> CreateAndSendRequestAsync(HttpMethod httpMethod, string query, object body)
        {
            ConfidentialClientApplication daemonClient = new ConfidentialClientApplication(msGraphConfig.ClientId, String.Format(msGraphConfig.AuthorityFormat, msGraphConfig.TenantId), msGraphConfig.RedirectUri, new ClientCredential(msGraphConfig.ClientSecret), null, new TokenCache());
            AuthenticationResult authResult = await daemonClient.AcquireTokenForClientAsync(new string[] { msGraphConfig.Scope });

            using (HttpClient client = new HttpClient())
            {
                HttpRequestMessage request = new HttpRequestMessage(httpMethod, query);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);

                if (body != null)
                    request.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.SendAsync(request);
                string responseBody = await response.Content.ReadAsStringAsync();

                return responseBody;
            }
        }

        public async Task<string> GetEvents()
        {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:65115/")
            };

            ConfidentialClientApplication daemonClient = new ConfidentialClientApplication(msGraphConfig.ClientId, String.Format(msGraphConfig.AuthorityFormat, msGraphConfig.TenantId), msGraphConfig.RedirectUri, new ClientCredential(msGraphConfig.ClientSecret), null, new TokenCache());
            AuthenticationResult authResult = await daemonClient.AcquireTokenForClientAsync(new string[] { msGraphConfig.Scope });

            var request = new HttpRequestMessage(HttpMethod.Get, $"https://graph.microsoft.com/v1.0/users/melissa.fontesse@mic-belgique.be/calendar/events/");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
            var response = await httpClient.SendAsync(request);

            //var path = $"https://graph.microsoft.com/v1.0/users/melissa.fontesse@mic-belgique.be/calendar/events/";
            //httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
            //var response = await httpClient.GetAsync(path);

            //parsing du json
            var responseBody = await new StringContent(JsonConvert.SerializeObject(response.Content.ReadAsStringAsync()), Encoding.UTF8, "application/json").ReadAsStringAsync(); //await response.Content.ReadAsStringAsync();
            
            OutlookCalendarResult test = await response.Content.ReadAsAsync<OutlookCalendarResult>();
            string[] startTab = responseBody.Split("\"start\":{\"dateTime\":\"");
            string[] endTab = responseBody.Split("end\":{\"dateTime\":\"");
            string[] idTab = responseBody.Split("\"id\":\"");
            return responseBody;
        }
    }
}
