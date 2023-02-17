using System.Text;

namespace DelegateLearningDocs.Hangfire.Tasks
{
    public class SlackMessageTask : ISlackMessageTask
    {
        public async Task<string> SendDelegateWebhookSlackMessage(string slackEndpoint, string message)
        {
            var contentObject = new { text = message };
            var contentJson = System.Text.Json.JsonSerializer.Serialize(contentObject);
            var content = new StringContent(contentJson, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                var result = await client.PostAsync(slackEndpoint, content);
                return await result.Content.ReadAsStringAsync();
            }
        }
    }
}
