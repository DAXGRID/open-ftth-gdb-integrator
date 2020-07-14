using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using OpenFTTH.GDBIntegrator.Config;
using OpenFTTH.GDBIntegrator.Producer;
using OpenFTTH.GDBIntegrator.Integrator.EventMessages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OpenFTTH.GDBIntegrator.Integrator.Commands
{
    public class NewRouteSegmentToExistingNodeCommand : IRequest
    {
        public RouteSegment RouteSegment { get; set; }
        public RouteNode StartRouteNode { get; set; }
        public RouteNode EndRouteNode { get; set; }
    }

    public class NewRouteSegmentToExistingNodeCommandHandler : IRequestHandler<NewRouteSegmentToExistingNodeCommand, Unit>
    {
        private readonly IGeoDatabase _geoDatabase;
        private readonly ILogger<NewRouteSegmentToExistingNodeCommandHandler> _logger;
        private readonly IProducer _producer;
        private readonly KafkaSetting _kafkaSettings;


        public NewRouteSegmentToExistingNodeCommandHandler(
            IGeoDatabase geoDatabase,
            ILogger<NewRouteSegmentToExistingNodeCommandHandler> logger,
            IProducer producer,
            IOptions<KafkaSetting> kafkaSettings
            )
        {
            _geoDatabase = geoDatabase;
            _logger = logger;
            _producer = producer;
            _kafkaSettings = kafkaSettings.Value;
        }

        public async Task<Unit> Handle(NewRouteSegmentToExistingNodeCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{DateTime.UtcNow.ToString("o")}: Starting - new routesegment to existing node.");

            if (request.RouteSegment is null)
                throw new ArgumentNullException("RouteSegment cannot be null");

            if (request.StartRouteNode is null && request.EndRouteNode is null)
                throw new ArgumentException("StartRouteNode and EndRouteNode cannot both be null");

            var startNode = request.StartRouteNode;
            var endNode = request.EndRouteNode;
            var eventId = Guid.NewGuid();

            if (startNode is null)
            {
                startNode = request.RouteSegment.FindStartNode();
                await _geoDatabase.InsertRouteNode(startNode);
                await _producer.Produce(_kafkaSettings.EventRouteNetworkTopicName,
                                        new RouteNodeAdded(eventId, startNode.Mrid, startNode.GetGeoJsonCoordinate()));
            }
            else
            {
                endNode = request.RouteSegment.FindEndNode();
                await _geoDatabase.InsertRouteNode(endNode);
                await _producer.Produce(_kafkaSettings.EventRouteNetworkTopicName,
                                        new RouteNodeAdded(eventId, endNode.Mrid, endNode.GetGeoJsonCoordinate()));
            }

            await _producer.Produce(_kafkaSettings.EventRouteNetworkTopicName,
                                    new RouteSegmentAdded(
                                        eventId,
                                        request.RouteSegment.Mrid,
                                        startNode.Mrid,
                                        endNode.Mrid,
                                        request.RouteSegment.GetWkbString()));

            _logger.LogInformation($"{DateTime.UtcNow.ToString("o")}: Finished - new routesegment to existing node.\n");

            return await Task.FromResult(new Unit());
        }
    }
}
