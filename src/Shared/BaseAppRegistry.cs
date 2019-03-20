using Microsoft.Extensions.Configuration;
using Serilog;
using StructureMap;

namespace Shared
{
    public class BaseAppRegistry : Registry
    {
        public BaseAppRegistry(IConfiguration configuration, ILogger logger)
        {
            ForSingletonOf<IConfiguration>()
                .Use(configuration);

            ForSingletonOf<ILogger>()
                .Use(ctx => logger.ForContext(ctx.ParentType ?? ctx.RootType));

            ForSingletonOf<ActorPropsRegistry>()
                .Use(new ActorPropsRegistry());
        }
    }
}