using System;
using Consumer.Actors;
using Consumer.Dependencies;
using Proto;
using Queueing.Models;
using StructureMap;

namespace Consumer.Factories
{
    /// <summary>
    /// Modified implementation on https://github.com/AsynkronIT/protoactor-dotnet/blob/dev/src/Proto.Actor.Extensions/ActorFactory.cs to support StructureMap
    /// </summary>
    public class ActorFactory : IActorFactory
    {
        private readonly IContainer _container;
        private readonly ActorPropsRegistry _registry;

        public ActorFactory(IContainer container, ActorPropsRegistry registry)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        }

        public PID RegisterActor<T>(T actor, string id = null, string address = null, Proto.IContext parent = null)
            where T : IActor
        {
            id = id ?? typeof(T).FullName;
            return GetActor(id, address, parent, () => CreateActor<T>(id, parent, () => new Props().WithProducer(() => actor)));
        }

        public PID GetActor(string id, string address = null, Proto.IContext parent = null)
        {
            return GetActor(id, address, parent, () => throw new InvalidOperationException($"Actor not created {id}"));
        }

        public PID GetActor<T>(string id = null, string address = null, Proto.IContext parent = null)
            where T : IActor
        {
            id = id ?? typeof(T).FullName;
            return GetActor(id, address, parent, () => CreateActor<T>(id, parent, () => new Props().WithProducer(() => _container.GetInstance<T>())));
        }

        public PID GetActor(string id, string address, Proto.IContext parent, Func<PID> create)
        {
            address = address ?? "nonhost";

            var pidId = id;
            if (parent != null)
            {
                pidId = $"{parent.Self.Id}/{id}";
            }

            var pid = new PID(address, pidId);
            var reff = ProcessRegistry.Instance.Get(pid);
            if (reff is DeadLetterProcess)
            {
                pid = create();
            }
            return pid;
        }

        private PID CreateActor<T>(string id, Proto.IContext parent, Func<Props> producer)
            where T : IActor
        {
            if (!_registry.RegisteredProps.TryGetValue(typeof(T), out var props))
            {
                props = x => x;
            }

            var props2 = props(producer());
            if (parent == null)
            {
                return Actor.SpawnNamed(props2, id);
            }

            return parent.SpawnNamed(props2, id);
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