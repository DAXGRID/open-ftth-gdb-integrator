using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace OpenFTTH.GDBIntegrator.Integrator.Notifications
{
    public class NewRouteSegmentDigitizedByUser : INotification
    {
        public RouteNode StartRouteNode { get; set; }
        public RouteNode EndRouteNode { get; set; }
        public RouteSegment RouteSegment { get; set; }
        public Guid EventId { get; set; }
    }

    public class NewRouteSegmentDigitizedByUserHandler : INotificationHandler<NewRouteSegmentDigitizedByUser>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<NewRouteSegmentDigitizedByUserHandler> _logger;
        private readonly IGeoDatabase _geoDatabase;

        public NewRouteSegmentDigitizedByUserHandler(
            IMediator mediator,
            ILogger<NewRouteSegmentDigitizedByUserHandler> logger,
            IGeoDatabase geoDatabase)
        {
            _mediator = mediator;
            _logger = logger;
            _geoDatabase = geoDatabase;
        }

        public async Task Handle(NewRouteSegmentDigitizedByUser request, CancellationToken token)
        {
            if (request.RouteSegment is null)
                throw new ArgumentNullException($"{nameof(RouteSegment)} cannot be null.");

            _logger.LogInformation($"{DateTime.UtcNow.ToString("o")}: Starting - {nameof(NewRouteSegmentDigitizedByUser)}\n");

            var eventId = request.EventId;

            var routeSegment = request.RouteSegment;

            var startNode = request.StartRouteNode;
            var endNode = request.EndRouteNode;

            if (startNode is null)
            {
                startNode = routeSegment.FindStartNode();
                await _geoDatabase.InsertRouteNode(startNode);
                await _mediator.Publish(new RouteNodeAdded { RouteNode = startNode, EventId = eventId });
            }
            if (endNode is null)
            {
                endNode = routeSegment.FindEndNode();
                await _geoDatabase.InsertRouteNode(endNode);
                await _mediator.Publish(new RouteNodeAdded { RouteNode = endNode, EventId = eventId });
            }

            await _mediator.Publish(new RouteSegmentAdded
                { EventId = eventId, RouteSegment = routeSegment, StartRouteNode = startNode, EndRouteNode = endNode });
        }
    }
}
