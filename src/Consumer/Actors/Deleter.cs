using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Consumer.Messages;
using Queueing;
using Proto;
using Proto.Schedulers.SimpleScheduler;
using Serilog;
using Shared;

namespace Consumer.Actors
{
    public class Deleter : IActor
    {
        private CancellationTokenSource _timer;

        private readonly ISQSClient _sqsClient;
        private readonly ILogger _logger;
        private readonly ISimpleScheduler _scheduler = new SimpleScheduler();
        private readonly List<string> _receiptHandles = new List<string>();

        public Deleter(ISQSClient sqsClient, ILogger logger)
        {
            _sqsClient = sqsClient ?? throw new ArgumentNullException(nameof(sqsClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task ReceiveAsync(IContext context)
        {
            switch (context.Message)
            {
                case Started _:
                {
                    _scheduler.ScheduleTellRepeatedly(
                        TimeSpan.FromSeconds(1),
                        TimeSpan.FromSeconds(2),
                        context.Self,
                        new DeleteCommandBatch(),
                        out _timer);
                }
                break;

                case DeleteCommand delete:
                {
                    AddToBatch(delete);
                }
                break;

                case DeleteCommandBatch batch:
                {
                    if (_receiptHandles.Count > 0)
                    {
                        var successful = new List<string>();

                        foreach (var chunk in _receiptHandles.ChunkBy(10))
                        {
                            var response = await _sqsClient.DeleteCommandBatchAsync(chunk);
                            successful.AddRange(response.Successful);
                        }

                        RemoveFromBatch(successful);
                    }
                }
                break;
            }
        }

        private void RemoveFromBatch(List<string> successful)
        {
            successful.ForEach(x => _receiptHandles.Remove(x));
        }

        private void AddToBatch(DeleteCommand delete)
        {
            _receiptHandles.Add(delete.ReceiptHandle);
        }
    }
}