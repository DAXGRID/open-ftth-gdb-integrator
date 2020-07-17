using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.Integrator.EventMessages;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using OpenFTTH.GDBIntegrator.Producer;
using OpenFTTH.GDBIntegrator.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OpenFTTH.GDBIntegrator.Integrator.Commands
{
    public class NewLonelyRouteSegment : IRequest
    {
        public RouteSegment RouteSegment { get; set; }
    }

    public class NewLonelyRouteSegmentHandler : IRequestHandler<NewLonelyRouteSegment, Unit>
    {
        private readonly IGeoDatabase _geoDatabase;
        private readonly ILogger<NewLonelyRouteSegmentHandler> _logger;
        private readonly IProducer _producer;
        private readonly KafkaSetting _kafkaSetting;

        public NewLonelyRouteSegmentHandler(
            IGeoDatabase geoDatabase,
            ILogger<NewLonelyRouteSegmentHandler> logger,
            IProducer producer,
            IOptions<KafkaSetting> kafkaSetting)
        {
            _geoDatabase = geoDatabase;
            _logger = logger;
            _producer = producer;
            _kafkaSetting = kafkaSetting.Value;
        }

        public async Task<Unit> Handle(NewLonelyRouteSegment request, CancellationToken cancellationToken)
        {
            if (request.RouteSegment is null)
                throw new ArgumentNullException($"{nameof(RouteSegment)} cannot be null.");

            _logger.LogInformation($"{DateTime.UtcNow.ToString("o")}: Starting - New lonely route segment.\n");

            var eventId = Guid.NewGuid();

            var routeSegment = request.RouteSegment;
            var startNode = routeSegment.FindStartNode();
            var endNode = routeSegment.FindEndNode();

            await _geoDatabase.InsertRouteNode(startNode);
            await _producer.Produce(_kafkaSetting.EventRouteNetworkTopicName,
                                    new RouteNodeAdded(eventId, startNode.Mrid, startNode.GetGeoJsonCoordinate()));

            await _geoDatabase.InsertRouteNode(endNode);
            await _producer.Produce(_kafkaSetting.EventRouteNetworkTopicName,
                                    new RouteNodeAdded(eventId, endNode.Mrid, endNode.GetGeoJsonCoordinate()));

            await _producer.Produce(_kafkaSetting.EventRouteNetworkTopicName,
                                    new RouteSegmentAdded(eventId, routeSegment.Mrid, startNode.Mrid, endNode.Mrid, routeSegment.GetGeoJsonCoordinate()));

            _logger.LogInformation($"{DateTime.UtcNow.ToString("o")}: Finished - New lonely route segment.\n");

            return await Task.FromResult(new Unit());
        }
    }
}
