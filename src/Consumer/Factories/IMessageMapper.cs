using Queueing.Models;

namespace Consumer.Factories
{
    public interface IMessageMapper
    {
        /// <summary>
        /// Maps an ISQSCommand to a Message object to be handled by an Actor
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        object Map(ISQSCommand command);
    }
}