using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using Microsoft.Extensions.Logging;

namespace OpenFTTH.GDBIntegrator.Integrator.Commands
{
    public class NewRouteSegmentToExistingNodeCommand : IRequest
    {
        public RouteSegment RouteSegment { get; set; }
        public RouteNode StartRouteNode { get; set; }
        public RouteNode EndRouteNode { get; set; }
    }

     public class NewRouteSegmentToExistingNodeCommandHandler : AsyncRequestHandler<NewRouteSegmentToExistingNodeCommand>
     {
         private readonly IGeoDatabase _geoDatabase;
         private readonly ILogger<NewRouteSegmentBetweenTwoExistingNodesCommandHandler> _logger;

         public NewRouteSegmentToExistingNodeCommandHandler(IGeoDatabase geoDatabase, ILogger<NewRouteSegmentBetweenTwoExistingNodesCommandHandler> logger)
         {
             _geoDatabase = geoDatabase;
             _logger = logger;
         }

         protected override async Task Handle(NewRouteSegmentToExistingNodeCommand request, CancellationToken cancellationToken)
         {
             _logger.LogInformation($"{DateTime.UtcNow} UTC: Starting - new routesegment to existing node.");

             if (request.RouteSegment is null)
                 throw new ArgumentNullException("RouteSegment cannot be null");

             if (request.StartRouteNode is null && request.EndRouteNode is null)
                 throw new ArgumentException("StartRouteNode and EndRouteNode cannot both be null");

             var routeSegment = request.RouteSegment;

             if (request.StartRouteNode is null)
                 await _geoDatabase.InsertRouteNode(routeSegment.FindStartNode());
             else
                 await _geoDatabase.InsertRouteNode(routeSegment.FindEndNode());

             _logger.LogInformation($"{DateTime.UtcNow} UTC: Finished - new routesegment to existing node.\n");
         }
     }
}
