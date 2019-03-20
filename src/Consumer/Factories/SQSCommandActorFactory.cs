using System;
using Consumer.Actors;
using Proto;
using Queueing.Models;
using Shared;

namespace Consumer.Factories
{
    public class SQSCommandActorFactory : ActorFactory, ISQSCommandActorFactory
    {
        public SQSCommandActorFactory(StructureMap.IContainer container, ActorPropsRegistry registry)
            : base(container, registry)
        {
        }

        public PID GetActor(ISQSCommand command)
        {
            switch (command)
            {
                case WebhookCommand webhook:
                    return GetActor<Dispatcher>($"Dispatcher_{webhook.CommandId}");
                default:
                    throw new NotSupportedException($"{command.GetType()} is not supported");
            }
        }
    }
}