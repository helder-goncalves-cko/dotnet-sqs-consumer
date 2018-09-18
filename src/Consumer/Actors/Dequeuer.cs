using System;
using System.Linq;
using System.Threading.Tasks;
using Proto;
using Serilog;
using Consumer.Messages;
using Queueing;
using Consumer.Factories;

namespace Consumer.Actors
{
    public class Dequeuer : IActor
    {
        private readonly ISQSClient _sqsClient;
        private readonly ILogger _logger;
        private readonly IActorFactory _factory;
        private readonly IMessageMapper _mapper;

        public Dequeuer(
            ISQSClient sqsClient,
            ILogger logger,
            IActorFactory factory,
            IMessageMapper mapper)
        {
            _sqsClient = sqsClient ?? throw new ArgumentNullException(nameof(sqsClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task ReceiveAsync(IContext context)
        {
            switch (context.Message)
            {
                case ReceiveCommands receive:
                {
                    var commands = await _sqsClient.ReceiveCommandBatchAsync(receive.NumberOfCommands);
                    if (commands.Count() > 0)
                    {
                        foreach (var command in commands)
                        {
                            var handler = _factory.GetActor(command);
                            var message = _mapper.Map(command);
                            handler.Tell(message);
                        }

                        context.Self.Tell(new ReceiveCommands());
                    }
                    else
                    {
                        context.Self.Tell(new BackOff());
                    }
                }
                break;

                case BackOff backOff:
                {
                    await Task.Delay(TimeSpan.FromSeconds(backOff.DelayInSeconds));
                    context.Self.Tell(new ReceiveCommands());
                }
                break;
            }
        }
    }
}