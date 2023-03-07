using DelegateLearningDocs.Hangfire.QueueManagers;
using DelegateLearningDocs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace DelegateLearningDocs.Controllers
{
    [ApiController]
    [Route("api/v1/slack/")]
    public class SlackController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ISlackMessageQueueManager _slackMessageQueueManager;
        private readonly IHttpClientFactory _httpClientFactory;

        public SlackController(IConfiguration configuration, ISlackMessageQueueManager slackMessageQueueManager, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _slackMessageQueueManager = slackMessageQueueManager;
            _httpClientFactory = httpClientFactory;
        }

        [HttpPost]
        [Route("hangfire/slackmessage")]
        public IActionResult HangfireSlackMessageWebhook(WebhookStep step)
        {
            var message = $"Hey there! I'm a background processed message using Hangfire! This request was from the {step.WorkflowName} workflow located on {step.RelativityInstanceURL}. Robots are cool! Beep boop beep :robot_face:";

            try
            {
                _slackMessageQueueManager.EnqueueSlackMessage(_configuration.GetValue<string>("SlackEndpoint"), message, _httpClientFactory);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok("Slack message sent successfully!");
        }

        [HttpPost]
        [Route("slackmessage")]
        public async Task<IActionResult> SlackMessageWebhook(WebhookStep step)
        {
            var message = $"Hey there! I'm a basic Slack Endpoint webhook. I was sent for testing purposes from {step.RelativityInstanceURL} in the {step.WorkflowName} workflow. :pandadance:";
            var contentObject = new { text = message };
            var contentJson = System.Text.Json.JsonSerializer.Serialize(contentObject);
            var content = new StringContent(contentJson, Encoding.UTF8, "application/json");

            var httpClient = _httpClientFactory.CreateClient("");
            var httpResponseMessage = await httpClient.PostAsync(_configuration.GetValue<string>("SlackEndpoint"), content);
            var httpResponseContent = await httpResponseMessage.Content.ReadAsStringAsync();

            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                return BadRequest(httpResponseContent);
                //Add logging
            }
            else
            {
                return Ok(httpResponseContent);
            }
        }
    }
}
