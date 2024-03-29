﻿using System.Text;

namespace DelegateLearningDocs.Hangfire.Tasks
{
    public class SlackMessageTask : ISlackMessageTask
    {
        public async Task<string> SendDelegateWebhookSlackMessage(string slackEndpoint, string message, HttpClient httpClient)
        {
            var contentObject = new { text = message };
            var contentJson = System.Text.Json.JsonSerializer.Serialize(contentObject);
            var content = new StringContent(contentJson, Encoding.UTF8, "application/json");

            var httpResponseMessage = await httpClient.PostAsync(slackEndpoint, content);
            return await httpResponseMessage.Content.ReadAsStringAsync();
        }
    }
}
