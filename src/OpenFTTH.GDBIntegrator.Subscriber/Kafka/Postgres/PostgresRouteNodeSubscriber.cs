using System;
using System.Threading.Tasks;
using Topos.Config;
using MediatR;
using OpenFTTH.GDBIntegrator.Config;
using OpenFTTH.GDBIntegrator.Subscriber.Kafka.Serialize;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace OpenFTTH.GDBIntegrator.Subscriber.Kafka.Postgres
{
    public class PostgresRouteNodeSubscriber : IRouteNodeSubscriber
    {
        private IDisposable _consumer;
        private readonly KafkaSetting _kafkaSetting;
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public PostgresRouteNodeSubscriber(IOptions<KafkaSetting> kafkaSetting, IMediator mediator, ILogger<PostgresRouteSegmentSubscriber> logger)
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
                .Topics(t => t.Subscribe(_kafkaSetting.PostgresRouteNodeTopic))
                .Positions(p => p.StoreInFileSystem(_kafkaSetting.PositionFilePath))
                .Handle(async (messages, context, token) =>
                {
                    foreach (var message in messages)
                    {
                        var routeSegment = (RouteNode)message.Body;
                        await HandleSubscribedEvent(routeSegment);
                    }
                }).Start();
        }

        private async Task HandleSubscribedEvent(RouteNode routeNode)
        {
            throw new NotImplementedException();
        }
    }
}
