namespace DelegateLearningDocs.Hangfire.Tasks
{
    public interface ISlackMessageTask
    {
        Task<string> SendDelegateWebhookSlackMessage(string slackEndpoint, string message, HttpClient httpClient);
    }
}
