using System;
using Topos.Config;
using MediatR;
using OpenFTTH.GDBIntegrator.Config;
using OpenFTTH.GDBIntegrator.Subscriber.Kafka.Serialize;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.RouteNetwork.Commands;
using Microsoft.Extensions.Options;

namespace OpenFTTH.GDBIntegrator.Subscriber.Kafka
{
    public class PostgresSubscriber : ISubscriber
    {
        private IDisposable _consumer;
        private readonly KafkaSetting _kafkaSetting;
        private readonly IMediator _mediator;

        public PostgresSubscriber(IOptions<KafkaSetting> kafkaSetting, IMediator mediator)
        {
            _kafkaSetting = kafkaSetting.Value;
            _mediator = mediator;
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

                        if (!String.IsNullOrEmpty(routeSegment.Mrid.ToString()))
                            await _mediator.Send(new GdbUpdatedCommand());
                    }
                }).Start();
        }

        public void Dispose()
        {
            _consumer.Dispose();
        }
    }
}
