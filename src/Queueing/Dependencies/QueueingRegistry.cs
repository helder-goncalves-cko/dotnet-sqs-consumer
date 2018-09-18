using Amazon.SQS;
using Microsoft.Extensions.Configuration;
using Queueing.Configuration;
using Queueing.Models;
using Shared;
using StructureMap;

namespace Queueing.Dependencies
{
    public class QueueingRegistry : Registry
    {
        public QueueingRegistry(IConfiguration configuration)
        {
            var settings = configuration.BindTo<QueueSettings>("Queueing");

            ForSingletonOf<QueueSettings>()
                .Use(settings);

            ForSingletonOf<IAmazonSQS>()
                .Use(CreateAmazonSQSClient(settings));

            ForSingletonOf<ISQSClient>()
                .Use<SQSClient>();

            Scan(s =>
            {
                s.AssemblyContainingType<ISQSCommand>();
                s.AddAllTypesOf<ISQSCommand>();
            });
        }

        private IAmazonSQS CreateAmazonSQSClient(QueueSettings settings)
        {
            if (string.IsNullOrWhiteSpace(settings.Endpoint))
                return new AmazonSQSClient();

            // local usage
            var config = new AmazonSQSConfig { ServiceURL = settings.Endpoint };
            return new AmazonSQSClient(config);
        }
    }
}