using AutoFixture;
using Microsoft.Extensions.Configuration;
using Queueing.Models;
using Serilog;
using Shared;

namespace Producer
{
    public class AppRegistry : BaseAppRegistry
    {
        public AppRegistry(IConfiguration configuration, ILogger logger)
            : base(configuration, logger)
        {
            ForSingletonOf<IActorFactory>()
                .Use<ActorFactory>();

            var fixture = new Fixture();

            fixture.Customize<WebhookCommand>(w =>
                w.With(x => x.Url, "http://requestbin.fullcontact.com/y1ca4ky1")
                .Without(x => x.ReceiptHandle));

            ForSingletonOf<IFixture>()
                .Use(fixture);
        }
    }
}