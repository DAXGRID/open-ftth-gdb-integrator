using System;
using System.Threading.Tasks;
using Topos.Config;
using MediatR;
using OpenFTTH.GDBIntegrator.Config;
using OpenFTTH.GDBIntegrator.Subscriber.Kafka.Serialize;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.Integrator.Notifications;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace OpenFTTH.GDBIntegrator.Subscriber.Kafka.Postgres
{
    public class PostgresRouteNodeSubscriber : IRouteNodeSubscriber
    {
        private IDisposable _consumer;
        private readonly KafkaSetting _kafkaSetting;
        private readonly IMediator _mediator;
        private readonly ILogger<PostgresRouteNodeSubscriber> _logger;

        public PostgresRouteNodeSubscriber(
            IOptions<KafkaSetting> kafkaSetting,
            IMediator mediator,
            ILogger<PostgresRouteNodeSubscriber> logger
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
                .Serialization(s => s.RouteNode())
                .Topics(t => t.Subscribe(_kafkaSetting.PostgresRouteNodeTopic))
                .Positions(p => p.StoreInFileSystem(_kafkaSetting.PositionFilePath))
                .Handle(async (messages, context, token) =>
                {
                    foreach (var message in messages)
                    {
                        if (message.Body is RouteNode)
                        {
                            _logger.LogInformation($"Received {nameof(RouteNode)}");
                            var routeNode = (RouteNode)message.Body;
                            await HandleSubscribedEvent(routeNode);
                        }
                    }
                }).Start();
        }

        private async Task HandleSubscribedEvent(RouteNode routeNode)
        {
            _logger.LogInformation($"{DateTime.UtcNow.ToString("o")}: Received message {JsonConvert.SerializeObject(routeNode, Formatting.Indented)}");

            if (!String.IsNullOrEmpty(routeNode.Mrid.ToString()))
            {
                await _mediator.Publish(new GeoDatabaseUpdated { UpdatedEntity = routeNode });
            }
            else
            {
                _logger.LogInformation($"{DateTime.UtcNow.ToString("o")}: Received message" + "RouteSegment deleted");
            }
        }

        public void Dispose()
        {
            _consumer.Dispose();
        }
    }
}
