using DelegateLearningDocs.Hangfire.QueueManagers;
using DelegateLearningDocs.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace DelegateLearningDocs.Controllers
{
    [ApiController]
    [Route("api/v1/webhook/")]
    public class WebhookController : ControllerBase
    {
        [HttpPost]
        [Route("200")]
        public IActionResult Delegate200Webhook(WebhookStep step)
        {
            return Ok($"This is a 200 response. The request body values were: {step.WorkflowName}, {step.WorkspaceId}, {step.RelativityInstanceURL}, {step.InstanceId}, {step.FriendlyInstanceName}");
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
