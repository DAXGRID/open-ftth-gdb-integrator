using System;
using System.Threading.Tasks;
using Topos.Config;
using MediatR;
using OpenFTTH.GDBIntegrator.Config;
using OpenFTTH.GDBIntegrator.Subscriber.Kafka.Serialize;
using OpenFTTH.GDBIntegrator.Integrator.ConsumerMessages;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.Integrator.Commands;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace OpenFTTH.GDBIntegrator.Subscriber.Kafka.Postgres
{
    public class PostgresRouteSegmentSubscriber : IRouteSegmentSubscriber
    {
        private IDisposable _consumer;
        private readonly KafkaSetting _kafkaSetting;
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public PostgresRouteSegmentSubscriber(
            IOptions<KafkaSetting> kafkaSetting,
            IMediator mediator,
            ILogger<PostgresRouteSegmentSubscriber> logger
            )
        {
            _kafkaSetting = kafkaSetting.Value;
            _mediator = mediator;
            _logger = logger;
        }

        public void Subscribe()
        {
            _consumer = Configure
                .Consumer(_kafkaSetting.PostgresRouteSegmentConsumer, c => c.UseKafka(_kafkaSetting.Server))
                .Serialization(s => s.RouteSegment())
                .Topics(t => t.Subscribe(_kafkaSetting.PostgresRouteSegmentTopic))
                .Positions(p => p.StoreInFileSystem(_kafkaSetting.PositionFilePath))
                .Handle(async (messages, context, token) =>
                {
                    foreach (var message in messages)
                    {
                        if (message.Body is RouteSegmentMessage)
                        {
                            var routeSegmentMessage = (RouteSegmentMessage)message.Body;
                            await HandleSubscribedEvent(routeSegmentMessage);
                        }
                    }
                }).Start();
        }

        private async Task HandleSubscribedEvent(RouteSegmentMessage routeSegmentMessage)
        {
            _logger.LogDebug($"{DateTime.UtcNow.ToString("o")}: Received message {JsonConvert.SerializeObject(routeSegmentMessage, Formatting.Indented)}");
            await _mediator.Send(new GeoDatabaseUpdated { UpdateMessage = routeSegmentMessage });
        }

        public void Dispose()
        {
            _consumer.Dispose();
        }
    }
}
