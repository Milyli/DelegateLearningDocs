namespace DelegateLearningDocs.Hangfire.QueueManagers
{
    public interface ISlackMessageQueueManager
    {
        void EnqueueSlackMessage(string slackEndpoint, string message, IHttpClientFactory httpClientFactory);
    }
}
