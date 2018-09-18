using System.Net.Http;
using Consumer.Factories;
using Microsoft.Extensions.Configuration;
using Serilog;
using StructureMap;

namespace Consumer.Dependencies
{
    public class AppRegistry : Registry
    {
        public AppRegistry(IConfiguration configuration, ILogger logger)
        {
            ForSingletonOf<IConfiguration>()
                .Use(configuration);

            ForSingletonOf<ILogger>()
                .Use(ctx => logger.ForContext(ctx.ParentType ?? ctx.RootType));

            ForSingletonOf<IActorFactory>()
                .Use<ActorFactory>();

            ForSingletonOf<ActorPropsRegistry>()
                .Use(new ActorPropsRegistry());

            ForSingletonOf<IMessageMapper>()
                .Use<MessageMapper>();
        }
    }
}