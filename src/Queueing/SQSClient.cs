using Amazon.SQS;
using Amazon.SQS.Model;
using Newtonsoft.Json;
using Queueing.Configuration;
using Queueing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace Queueing
{
    public class SQSClient : ISQSClient
    {
        private readonly IAmazonSQS _client;
        private readonly QueueSettings _settings;
        private readonly ISQSCommand[] _messageTypes;
        private const string MessageType = "MessageType";
        private readonly ILogger _logger;

        public SQSClient(
            IAmazonSQS client,
            QueueSettings settings,
            ISQSCommand[] messageTypes,
            ILogger logger
        )
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _messageTypes = messageTypes ?? throw new ArgumentNullException(nameof(messageTypes));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<(IReadOnlyCollection<string> Successful, IReadOnlyCollection<string> Failed)> DeleteCommandBatchAsync(IEnumerable<string> receiptHandles, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (receiptHandles == null)
                throw new ArgumentNullException(nameof(receiptHandles));

            var entries = receiptHandles.Select(receipt => new DeleteMessageBatchRequestEntry
            {
                Id = receipt,
                ReceiptHandle = receipt
            });

            var response = await _client.DeleteMessageBatchAsync(_settings.QueueUrl, entries.ToList(), cancellationToken);
            var successful = response?.Successful.Select(x => x.Id) ?? new List<string>();
            var failed = response?.Failed.Select(x => x.Id) ?? receiptHandles;
            return (successful.ToList(), failed.ToList());
        }

        public async Task<IReadOnlyCollection<ISQSCommand>> ReceiveCommandBatchAsync(int maxNumberOfCommands = 10, CancellationToken cancellationToken = default(CancellationToken))
        {
            var request = new ReceiveMessageRequest
            {
                QueueUrl = _settings.QueueUrl,
                MaxNumberOfMessages = maxNumberOfCommands,
                VisibilityTimeout = _settings.VisibilityTimeout,
                MessageAttributeNames = new List<string> { MessageType }
            };

            var response = await _client.ReceiveMessageAsync(request, cancellationToken);
            var firstMessage = response?.Messages?.FirstOrDefault();
            if (firstMessage == null)
                return new ISQSCommand[0];

            var sqsMessages = new List<ISQSCommand>();
            foreach (var message in response.Messages)
            {
                var attribute = message.MessageAttributes.FirstOrDefault();
                var type = _messageTypes.FirstOrDefault(x => x.GetType().FullName.Equals(attribute.Value?.StringValue, StringComparison.OrdinalIgnoreCase));
                if (type == null)
                {
                    _logger.Error($"'{attribute.Value?.StringValue}' command type is not supported.");
                }
                else
                {
                    var command = JsonConvert.DeserializeObject(message.Body, type.GetType()) as ISQSCommand;
                    command.ReceiptHandle = message.ReceiptHandle;
                    sqsMessages.Add(command);
                }
            }

            return sqsMessages;
        }

        public async Task<(IReadOnlyCollection<string> Successful, IReadOnlyCollection<string> Failed)> SendCommandBatchAsync(IEnumerable<ISQSCommand> commands, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (commands == null)
                throw new ArgumentNullException(nameof(commands));

            IEnumerable<string> successful = null, failed = null;

            try
            {
                var entries = commands.Select(command => new SendMessageBatchRequestEntry
                {
                    Id = command.CommandId.ToString(),
                    MessageBody = JsonConvert.SerializeObject(command, new JsonSerializerSettings{ NullValueHandling = NullValueHandling.Ignore}),
                    MessageAttributes = new Dictionary<string, MessageAttributeValue>
                    {
                        {
                            MessageType,
                            new MessageAttributeValue
                            {
                                StringValue = command.GetType().FullName,
                                DataType = nameof(String)
                            }
                        }
                    }
                });

                var response = await _client.SendMessageBatchAsync(_settings.QueueUrl, entries.ToList(), cancellationToken);
                successful = response?.Successful.Select(x => x.Id) ?? new List<string>();
                failed = response?.Failed.Select(x => x.Id) ?? commands.Select(x => x.CommandId.ToString());
            }
            catch (Exception ex)
            {
            }

            return (successful.ToList(), failed.ToList());
        }
    }
}