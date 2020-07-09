using System;
using System.Threading.Tasks;
using System.Linq;
using Topos.Config;
using MediatR;
using OpenFTTH.GDBIntegrator.Config;
using OpenFTTH.GDBIntegrator.Subscriber.Kafka.Serialize;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.Integrator.Queries;
using OpenFTTH.GDBIntegrator.Integrator.Commands;
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
                        await HandleSubscribedEvent(routeSegment);
                    }
                }).Start();
        }

        private async Task HandleSubscribedEvent(RouteSegment routeSegment)
        {
            _logger.LogInformation(DateTime.UtcNow + " UTC: Received message "
                                   + JsonConvert.SerializeObject(routeSegment, Formatting.Indented));

            if (!String.IsNullOrEmpty(routeSegment.Mrid.ToString()))
            {
                var intersectingStartNodes = await _mediator.Send(new GetIntersectingStartRouteNodes { RouteSegment = routeSegment });
                var intersectingEndNodes = await _mediator.Send(new GetIntersectingEndRouteNodes { RouteSegment = routeSegment });

                var totalIntersectingNodes = intersectingStartNodes.Count + intersectingEndNodes.Count;

                if (totalIntersectingNodes == 0)
                {
                    _logger.LogInformation(DateTime.UtcNow + " UTC: No routenodes intersect calling NewLonelyRouteSegmentCommand");
                    await _mediator.Send(new NewLonelyRouteSegmentCommand { RouteSegment = routeSegment });
                }
                else if (totalIntersectingNodes == 1)
                {
                    _logger.LogInformation(DateTime.UtcNow + " UTC: Single routenode intersect calling ");
                    await _mediator.Send(new NewRouteSegmentDigitizedToExistingNodeCommand
                        {
                            RouteSegment = routeSegment,
                            StartRouteNode = intersectingStartNodes.FirstOrDefault(),
                            EndRouteNode = intersectingEndNodes.FirstOrDefault()
                        });
                }
            }
            else
            {
                _logger.LogInformation(DateTime.UtcNow + " UTC: Received message" + "RouteSegment deleted");
            }
        }

        public void Dispose()
        {
            _consumer.Dispose();
        }
    }
}
