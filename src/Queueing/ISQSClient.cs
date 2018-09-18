using Queueing.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Queueing
{
    public interface ISQSClient
    {
        /// <summary>
        /// Long polls a predefined number of commands from the queue
        /// </summary>
        /// <param name="maxNumberOfCommands"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IReadOnlyCollection<ISQSCommand>> ReceiveCommandBatchAsync(int maxNumberOfCommands = 10, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// A batch queue insert can result in a combination of successful and unsuccessful commands
        /// </summary>
        /// <param name="commands"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<(IReadOnlyCollection<string> Successful, IReadOnlyCollection<string> Failed)> SendCommandBatchAsync(IEnumerable<ISQSCommand> commands, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// A batch queue delete can result in a combination of successful and unsuccessful commands
        /// </summary>
        /// <param name="receiptHandles"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<(IReadOnlyCollection<string> Successful, IReadOnlyCollection<string> Failed)> DeleteCommandBatchAsync(IEnumerable<string> receiptHandles, CancellationToken cancellationToken = default(CancellationToken));
    }
}