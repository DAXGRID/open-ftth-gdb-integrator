using System;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.RouteNetwork.Factories;
using OpenFTTH.GDBIntegrator.Producer;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using Microsoft.Extensions.Logging;

namespace OpenFTTH.GDBIntegrator.Integrator.Commands
{
    public class ExistingRouteSegmentSplittedByRouteNode : IRequest
    {
        public RouteNode RouteNode { get; set; }
        public RouteSegment IntersectingRouteSegment { get; set; }
    }

    public class ExistingRouteSegmentSplittedByRouteNodeHandler : IRequestHandler<ExistingRouteSegmentSplittedByRouteNode, Unit>
    {
        private readonly ILogger<ExistingRouteSegmentSplittedByRouteNodeHandler> _logger;
        private readonly IProducer _producer;
        private readonly IGeoDatabase _geoDatabase;
        private readonly IRouteSegmentFactory _routeSegmentFactory;

        public ExistingRouteSegmentSplittedByRouteNodeHandler(
            ILogger<ExistingRouteSegmentSplittedByRouteNodeHandler> logger,
            IProducer producer,
            IGeoDatabase geoDatabase,
            IRouteSegmentFactory routeSegmentFactory)
        {
            _logger = logger;
            _producer = producer;
            _geoDatabase = geoDatabase;
            _routeSegmentFactory = routeSegmentFactory;
        }

        public async Task<Unit> Handle(ExistingRouteSegmentSplittedByRouteNode request, CancellationToken token)
        {
            _logger.LogInformation($"{DateTime.UtcNow.ToString("o")}: Starting Existing route segment splitted by route node");

            var routeSegmentsWkt = await _geoDatabase.GetRouteSegmentsSplittedByRouteNode(request.RouteNode, request.IntersectingRouteSegment);

            var routeSegments = _routeSegmentFactory.Create(routeSegmentsWkt);

            foreach(var routeSegment in routeSegments)
            {
                _logger.LogInformation($"{DateTime.UtcNow.ToString("o")}: Inserting routesegment: {routeSegment.Mrid}");
                await _geoDatabase.InsertRouteSegment(routeSegment);
            }

            _logger.LogInformation($"{DateTime.UtcNow.ToString("o")}: Deleting routesegment: {request.IntersectingRouteSegment.Mrid}");
            await _geoDatabase.DeleteRouteSegment(request.IntersectingRouteSegment.Mrid);

            return await Task.FromResult(new Unit());
        }
    }
}
