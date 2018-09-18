namespace Queueing.Models
{
    public interface ISQSCommand
    {
        string CommandId { get; set; }
        string ReceiptHandle { get; set; }
    }
}