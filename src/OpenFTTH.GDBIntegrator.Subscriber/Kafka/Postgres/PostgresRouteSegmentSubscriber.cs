using System;
using System.Threading.Tasks;
using Topos.Config;
using MediatR;
using OpenFTTH.GDBIntegrator.Config;
using OpenFTTH.GDBIntegrator.Subscriber.Kafka.Serialize;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.Integrator.Factory;
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
        private readonly IRouteSegmentCommandFactory _routeSegmentCommandFactory;

        public PostgresRouteSegmentSubscriber(
            IOptions<KafkaSetting> kafkaSetting,
            IMediator mediator,
            ILogger<PostgresRouteSegmentSubscriber> logger,
            IRouteSegmentCommandFactory routeSegmentCommandFactory
            )
        {
            _kafkaSetting = kafkaSetting.Value;
            _mediator = mediator;
            _logger = logger;
            _routeSegmentCommandFactory = routeSegmentCommandFactory;
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
                        if (message.Body is RouteSegment)
                        {
                            _logger.LogInformation($"Received {nameof(RouteSegment)}");
                            var routeSegment = (RouteSegment)message.Body;
                            await HandleSubscribedEvent(routeSegment);
                        }
                        else if (message.Body is RouteNode)
                        {
                            _logger.LogInformation($"Received {nameof(RouteNode)}");
                        }
                    }
                }).Start();
        }

        private async Task HandleSubscribedEvent(RouteSegment routeSegment)
        {
            _logger.LogInformation($"{DateTime.UtcNow.ToString("o")}: Received message {JsonConvert.SerializeObject(routeSegment, Formatting.Indented)}");

            if (!String.IsNullOrEmpty(routeSegment.Mrid.ToString()))
            {
                var command = await _routeSegmentCommandFactory.Create(routeSegment);
                await _mediator.Send(command);
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
