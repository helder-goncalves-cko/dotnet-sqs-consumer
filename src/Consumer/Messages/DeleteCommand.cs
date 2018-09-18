namespace Consumer.Messages
{
    public class DeleteCommand
    {
        public string CommandId { get; set; }
        public string ReceiptHandle { get; set; }
    }
}