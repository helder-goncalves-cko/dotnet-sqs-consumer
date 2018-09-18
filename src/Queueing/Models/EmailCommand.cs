namespace Queueing.Models
{
    public class EmailCommand : SQSCommand
    {
        public string Email { get; set; }
    }
}