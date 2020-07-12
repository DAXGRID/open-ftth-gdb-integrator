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

     public class NewRouteSegmentToExistingNodeCommandHandler : IRequestHandler<NewRouteSegmentToExistingNodeCommand, Unit>
     {
         private readonly IGeoDatabase _geoDatabase;
         private readonly ILogger<NewRouteSegmentToExistingNodeCommandHandler> _logger;
         private readonly IMediator _mediator;

         public NewRouteSegmentToExistingNodeCommandHandler(IGeoDatabase geoDatabase, ILogger<NewRouteSegmentToExistingNodeCommandHandler> logger, IMediator mediator)
         {
             _geoDatabase = geoDatabase;
             _logger = logger;
             _mediator = mediator;
         }

         public async Task<Unit> Handle(NewRouteSegmentToExistingNodeCommand request, CancellationToken cancellationToken)
         {
             _logger.LogInformation($"{DateTime.UtcNow.ToString("o")}: Starting - new routesegment to existing node.");

             if (request.RouteSegment is null)
                 throw new ArgumentNullException("RouteSegment cannot be null");

             if (request.StartRouteNode is null && request.EndRouteNode is null)
                 throw new ArgumentException("StartRouteNode and EndRouteNode cannot both be null");

             var startNode = request.RouteSegment.FindStartNode();
             var endNode = request.RouteSegment.FindEndNode();
             var eventId = Guid.NewGuid().ToString();

             if (request.StartRouteNode is null)
             {
                 await _geoDatabase.InsertRouteNode(startNode);
                 // await _mediator.Send(new RouteNodeAddedCommand
                 //     {
                 //         NodeId = startNode.Mrid.ToString(),
                 //         EventId = eventId,
                 //     });
             }
             else
                 await _geoDatabase.InsertRouteNode(endNode);

             _logger.LogInformation($"{DateTime.UtcNow.ToString("o")}: Finished - new routesegment to existing node.\n");

             return await Task.FromResult(new Unit());
         }
     }
}
