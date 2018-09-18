namespace Queueing.Models
{
    public class WebhookCommand : SQSCommand
    {
        public string Url { get; set; }
    }
}