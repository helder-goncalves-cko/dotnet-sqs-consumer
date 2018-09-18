using Queueing.Models;

namespace Consumer.Messages
{
    public class SendRequest
    {
        public SendRequest(WebhookCommand command)
        {
            CommandId = command.CommandId;
            ReceiptHandle = command.ReceiptHandle;
            Url = command.Url;
            Data = "This is data";
        }

        public string CommandId { get; set; }
        public string ReceiptHandle { get; set; }
        public string Url { get; set; }
        public string Data { get; set; }
    }
}