using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.GeoDatabase;

namespace OpenFTTH.GDBIntegrator.Integrator.Commands
{
    public class NewRouteSegmentToExistingNodeCommand : IRequest
    {
        public RouteSegment RouteSegment { get; set; }
        public RouteNode StartRouteNode { get; set; }
        public RouteNode EndRouteNode { get; set; }
    }

     public class NewRouteSegmentToExistingNodeHandler : AsyncRequestHandler<NewRouteSegmentToExistingNodeCommand>
     {
         private readonly IGeoDatabase _geoDatabase;

         public NewRouteSegmentToExistingNodeHandler(IGeoDatabase geoDatabase)
         {
             _geoDatabase = geoDatabase;
         }

         protected override async Task Handle(NewRouteSegmentToExistingNodeCommand request, CancellationToken cancellationToken)
         {
             if (request.RouteSegment is null)
                 throw new ArgumentNullException("RouteSegment cannot be null");

             if (request.StartRouteNode is null && request.EndRouteNode is null)
                 throw new ArgumentException("StartRouteNode and EndRouteNode cannot both be null");

             var routeSegment = request.RouteSegment;

             if (request.StartRouteNode is null)
                 await _geoDatabase.InsertRouteNode(routeSegment.FindStartNode());
             else
                 await _geoDatabase.InsertRouteNode(routeSegment.FindEndNode());
         }
     }
}
