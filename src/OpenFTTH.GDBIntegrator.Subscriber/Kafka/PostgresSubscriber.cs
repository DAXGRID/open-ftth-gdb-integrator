using System;
using Topos.Config;
using MediatR;
using OpenFTTH.GDBIntegrator.Config;
using OpenFTTH.GDBIntegrator.Subscriber.Kafka.Serialize;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.Integrator.Queries;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace OpenFTTH.GDBIntegrator.Subscriber.Kafka
{
    public class PostgresSubscriber : ISubscriber
    {
        private IDisposable _consumer;
        private readonly KafkaSetting _kafkaSetting;
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public PostgresSubscriber(IOptions<KafkaSetting> kafkaSetting, IMediator mediator, ILogger<PostgresSubscriber> logger)
        {
            _kafkaSetting = kafkaSetting.Value;
            _mediator = mediator;
            _logger = logger;
        }

        public void Subscribe()
        {
            _consumer = Configure
                .Consumer(_kafkaSetting.Consumer, c => c.UseKafka(_kafkaSetting.Server))
                .Serialization(s => s.RouteSegment())
                .Topics(t => t.Subscribe(_kafkaSetting.Topic))
                .Positions(p => p.StoreInFileSystem(_kafkaSetting.PositionFilePath))
                .Handle(async (messages, context, token) =>
                {
                    foreach (var message in messages)
                    {
                        var routeSegment = (RouteSegment)message.Body;

                        _logger.LogInformation(DateTime.UtcNow + " UTC: Received message "
                                               + JsonConvert.SerializeObject(routeSegment, Formatting.Indented));

                        if (!String.IsNullOrEmpty(routeSegment.Mrid.ToString()))
                        {
                            await _mediator.Send(new GetIntersectingRouteNodes { RouteSegment = routeSegment });
                        }
                        else
                        {
                            _logger.LogInformation(DateTime.UtcNow + " UTC: Received message" + "RouteSegment deleted");
                        }
                    }
                }).Start();
        }

        public void Dispose()
        {
            _consumer.Dispose();
        }
    }
}
