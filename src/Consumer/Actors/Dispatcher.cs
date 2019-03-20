using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Proto;
using Serilog;
using SerilogTimings.Extensions;
using Consumer.Messages;
using Consumer.Factories;

namespace Consumer.Actors
{
    public class Dispatcher : IActor
    {
        private readonly ILogger _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ISQSCommandActorFactory _actorFactory;

        public Dispatcher(ILogger logger, IHttpClientFactory httpClientFactory, ISQSCommandActorFactory actorFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _actorFactory = actorFactory ?? throw new ArgumentNullException(nameof(actorFactory));
        }

        public async Task ReceiveAsync(IContext context)
        {
            switch (context.Message)
            {
                case Started _:
                {
                    context.SetReceiveTimeout(TimeSpan.FromSeconds(5));
                }
                break;

                case SendRequest sendRequest:
                {
                    using (_logger.TimeOperation("{ActorId} sending request to {Url}", context.Self.Id, sendRequest.Url))
                    using (var client = _httpClientFactory.CreateClient(nameof(Dispatcher)))
                    {
                        var content = new StringContent(sendRequest.Data, Encoding.UTF8, "application/json");
                        await client.PostAsync(sendRequest.Url, content);
                    }

                    var deleter = _actorFactory.GetActor<Deleter>();

                    deleter.Tell(
                        new DeleteCommand
                        {
                            CommandId = sendRequest.CommandId,
                            ReceiptHandle = sendRequest.ReceiptHandle
                        });
                }
                break;

                case ReceiveTimeout _:
                {
                    context.Self.Stop();
                }
                break;
            }
        }
    }
}