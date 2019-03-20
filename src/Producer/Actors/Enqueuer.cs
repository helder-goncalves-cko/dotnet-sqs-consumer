using System;
using System.Linq;
using System.Threading.Tasks;
using Proto;
using Serilog;
using Queueing;
using Producer.Messages;
using Queueing.Models;
using AutoFixture;

namespace Producer.Actors
{
    public class Enqueuer : IActor
    {
        private readonly ISQSClient _sqsClient;
        private readonly ILogger _logger;
        private readonly IFixture _fixture;

        public Enqueuer(
            ISQSClient sqsClient,
            ILogger logger,
            IFixture fixture)
        {
            _sqsClient = sqsClient ?? throw new ArgumentNullException(nameof(sqsClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        }

        public async Task ReceiveAsync(IContext context)
        {
            switch (context.Message)
            {
                case SendCommands send:
                {
                    var commands = _fixture.CreateMany<WebhookCommand>(10);
                    var result = await _sqsClient.SendCommandBatchAsync(commands);

                    _logger.Information($"Sending {result.Successful.Count} commands");

                    context.Self.Tell(new BackOff());
                }
                break;

                case BackOff backOff:
                {
                    await Task.Delay(TimeSpan.FromSeconds(backOff.DelayInSeconds));
                    context.Self.Tell(new SendCommands());
                }
                break;
            }
        }
    }
}