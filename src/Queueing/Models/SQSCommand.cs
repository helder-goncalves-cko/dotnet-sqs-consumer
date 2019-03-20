using System;

namespace Queueing.Models
{
    public class SQSCommand : ISQSCommand
    {
        public string CommandId { get; set; }
        public string ReceiptHandle { get; set; }
    }
}