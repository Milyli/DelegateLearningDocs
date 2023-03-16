using DelegateLearningDocs.Hangfire.Tasks;
using Hangfire;

namespace DelegateLearningDocs.Hangfire.QueueManagers
{
    public class SlackMessageQueueManager : ISlackMessageQueueManager
    {
        public void EnqueueSlackMessage(string slackEndpoint, string message, HttpClient httpClient)
        {
            BackgroundJob.Enqueue<ISlackMessageTask>(x => x.SendDelegateWebhookSlackMessage(slackEndpoint, message, httpClient));
        }
    }
}
