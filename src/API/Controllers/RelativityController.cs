using DelegateLearningDocs.Hangfire.QueueManagers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Dynamic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DelegateLearningDocs.Models;
using System.Net.Http.Headers;
using System.Text;

namespace DelegateLearningDocs.Controllers
{
    [ApiController]
    [Route("api/v1/relativity/")]
    public class RelativityController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public RelativityController(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        [HttpPost]
        [Route("startworkflow")]
        public async Task<IActionResult> StartDelegateWorkflow(WorkflowStart workflowStart)
        {
            //Create Json request body Delegate expects to start a workflow
            var workflowStartObject = new { workflowName = workflowStart.WorkflowName };
            var workflowStartJson = System.Text.Json.JsonSerializer.Serialize(workflowStartObject);
            var requestBody = new StringContent(workflowStartJson, Encoding.UTF8, "application/json");

            //Create an unsecure http client that ignores dev certifications
            var httpClient = _httpClientFactory.CreateClient("UnsecureRelativityClient");
            httpClient.BaseAddress = new Uri(_configuration.GetValue<string>("RelativityBaseUri"));
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Add("X-CSRF-Header", "-");

            //Get the bearer token from the Relativity token service then create authorization header
            var token = await GetBearerToken();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var url = $"Relativity/CustomPages/66e2c574-c9d9-46f5-8581-98db7c016464/api/v1/{workflowStart.WorkspaceId}/workflow/start";

            var httpResponseMessage = await httpClient.PostAsync(url, requestBody);
            var httpResponseContent = await httpResponseMessage.Content.ReadAsStringAsync();

            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                return BadRequest(httpResponseContent);
            }
            else
            {
                return Ok(httpResponseContent);
            }
        }

        private async Task<string> GetBearerToken()
        {
            var formParameters = new Dictionary<string, string>
            {
                { "client_id", _configuration.GetValue<string>("RelativityOAuthClientId") },
                { "client_secret", _configuration.GetValue<string>("RelativityOAuthClientSecret") },
                { "scope", "SystemUserInfo" },
                { "grant_type", "client_credentials" }
            };

            var url = $"Relativity/Identity/connect/token";

            var httpClient = _httpClientFactory.CreateClient("UnsecureRelativityClient");
            httpClient.BaseAddress = new Uri(_configuration.GetValue<string>("RelativityBaseUri"));

            var httpResponseMessage = await httpClient.PostAsync(url, new FormUrlEncodedContent(formParameters));

            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                var errorMessage = await httpResponseMessage.Content.ReadAsStringAsync();
                throw new Exception("Failed to retrieve token: " + errorMessage);
            }
            else
            {
                var responseContent = JsonConvert.DeserializeObject<ExpandoObject>(await httpResponseMessage.Content.ReadAsStringAsync());
                var responseJson = JToken.Parse(JsonConvert.SerializeObject(responseContent));
                var token = responseJson["access_token"].ToString().Trim(new char[] { '{', '}' });
                return token;
            }
        }
    }
}
