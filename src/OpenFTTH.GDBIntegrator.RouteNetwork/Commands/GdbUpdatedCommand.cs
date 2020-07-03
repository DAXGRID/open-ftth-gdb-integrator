using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NetTopologySuite.IO;
using NetTopologySuite.Geometries;

namespace OpenFTTH.GDBIntegrator.RouteNetwork.Commands
{
    public class GdbUpdatedCommand : IRequest
    {
        public RouteSegment RouteSegment { get; set; }
    }

    public class GdbUpdatedCommandHandler : IRequestHandler<GdbUpdatedCommand, Unit>
    {
        private IMediator _mediator;

        public GdbUpdatedCommandHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public Task<Unit> Handle(GdbUpdatedCommand request, CancellationToken cancellationToken)
        {
            var routeSegment = request.RouteSegment;

            if (routeSegment.Mrid == Guid.Empty)
                return default;

            var routeSegmentBytes = Convert.FromBase64String(routeSegment.Coord);
            var wkbReader = new WKBReader();
            var geometry = wkbReader.Read(routeSegmentBytes);
            var lineString = (LineString)geometry;

            var wkbWriter = new WKBWriter();
            var startPoint = new Point(lineString.StartPoint.X, lineString.StartPoint.Y);
            var endPoint = new Point(lineString.EndPoint.X, lineString.EndPoint.Y);

            var startNode = new RouteNode(Guid.NewGuid(), wkbWriter.Write(startPoint), Guid.NewGuid(), String.Empty, String.Empty);
            var endNode = new RouteNode(Guid.NewGuid(), wkbWriter.Write(endPoint), Guid.NewGuid(), String.Empty, String.Empty);

           return default;
        }
    }
}
