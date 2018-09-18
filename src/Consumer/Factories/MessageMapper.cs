using System;
using Consumer.Messages;
using Queueing.Models;

namespace Consumer.Factories
{
    public class MessageMapper : IMessageMapper
    {
        public object Map(ISQSCommand command)
        {
            switch (command)
            {
                case WebhookCommand webhook:
                    return new SendRequest(webhook);
                default:
                    throw new NotSupportedException($"{command.GetType()} is not supported");
            }
        }
    }
}