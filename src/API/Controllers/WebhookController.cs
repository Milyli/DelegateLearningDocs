using DelegateLearningDocs.Hangfire.QueueManagers;
using DelegateLearningDocs.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace DelegateLearningDocs.Controllers
{
    [ApiController]
    [Route("api/v1/webhook/")]
    public class WebhookController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ISlackMessageQueueManager _slackMessageQueueManager;

        public WebhookController(IConfiguration configuration, ISlackMessageQueueManager slackMessageQueueManager)
        {
            _configuration = configuration;
            _slackMessageQueueManager = slackMessageQueueManager;
        }

        [HttpPost]
        [Route("hangfire/slackmessage")]
        public IActionResult HangfireSlackMessageWebhook(WebhookStep step)
        {
            var message = $"Hey there! I'm a background processed message using Hangfire! This request was from the {step.WorkflowName} located on {step.RelativityInstanceURL}. Robots are cool! Beep boop beep :robot_face:";

            try
            {
                _slackMessageQueueManager.EnqueueSlackMessage(_configuration.GetValue<string>("SlackEndpoint"), message);
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

            using (var client = new HttpClient())
            {
                var result = await client.PostAsync(_configuration.GetValue<string>("SlackEndpoint"), content);
                var resultContent = await result.Content.ReadAsStringAsync();

                if (!result.IsSuccessStatusCode)
                {
                    return BadRequest(resultContent);
                    //Add logging
                }
                else
                {
                    return Ok(resultContent);
                }
            }
        }

        [HttpPost]
        [Route("200")]
        public IActionResult Delegate200Webhook(WebhookStep step)
        {
            return Ok($"This is a 200 response. The request body values were: {step.WorkflowName}, {step.WorkspaceId}, {step.RelativityInstanceURL}, {step.InstanceId}");
        }

        [HttpPost]
        [Route("400")]
        public IActionResult Delegate400Webhook(WebhookStep step)
        {
            return BadRequest("This is a 400 response.");
        }

        [HttpPost]
        [Route("404")]
        public IActionResult Delegate404Webhook(WebhookStep step)
        {
            return NotFound("This is a 404 response.");
        }

        [HttpPost]
        [Route("500")]
        public IActionResult Delegate500Webhook(WebhookStep step)
        {
            return StatusCode(500, "This is a 500 response.");
        }

        [HttpPost]
        [Route("timeout")]
        public IActionResult DelegateTimeoutWebhook(WebhookStep step)
        {
            Thread.Sleep(60000);
            return Ok("I have finished sleeping for 60 seconds!");
        }
    }
}
