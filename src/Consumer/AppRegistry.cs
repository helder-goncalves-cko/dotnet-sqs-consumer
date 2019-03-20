using Consumer.Factories;
using Microsoft.Extensions.Configuration;
using Serilog;
using Shared;

namespace Consumer.Dependencies
{
    public class AppRegistry : BaseAppRegistry
    {
        public AppRegistry(IConfiguration configuration, ILogger logger)
            : base(configuration, logger)
        {
            ForSingletonOf<ISQSCommandActorFactory>()
                .Use<SQSCommandActorFactory>();

            ForSingletonOf<IMessageMapper>()
                .Use<MessageMapper>();
        }
    }
}