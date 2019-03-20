using Proto;
using Queueing.Models;
using Shared;

namespace Consumer.Factories
{
    public interface ISQSCommandActorFactory : IActorFactory
    {
        /// <summary>
        /// Get the Actor responsible of processing the specific command
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        PID GetActor(ISQSCommand command);
    }
}