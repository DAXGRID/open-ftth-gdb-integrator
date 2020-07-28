using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.RouteNetwork.Factories;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace OpenFTTH.GDBIntegrator.Integrator.Notifications
{
    public class NewRouteSegmentDigitizedByUser : INotification
    {
        public RouteSegment RouteSegment { get; set; }
        public Guid EventId { get; set; }
    }

    public class NewRouteSegmentDigitizedByUserHandler : INotificationHandler<NewRouteSegmentDigitizedByUser>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<NewRouteSegmentDigitizedByUserHandler> _logger;
        private readonly IGeoDatabase _geoDatabase;
        private readonly IRouteNodeFactory _routeNodeFactory;

        public NewRouteSegmentDigitizedByUserHandler(
            IMediator mediator,
            ILogger<NewRouteSegmentDigitizedByUserHandler> logger,
            IGeoDatabase geoDatabase,
            IRouteNodeFactory routeNodeFactory)
        {
            _mediator = mediator;
            _logger = logger;
            _geoDatabase = geoDatabase;
            _routeNodeFactory = routeNodeFactory;
        }

        public async Task Handle(NewRouteSegmentDigitizedByUser request, CancellationToken token)
        {
            if (request.RouteSegment is null)
                throw new ArgumentNullException($"{nameof(RouteSegment)} cannot be null.");

            _logger.LogInformation($"{DateTime.UtcNow.ToString("o")}: Starting - {nameof(NewRouteSegmentDigitizedByUser)}\n");

            var eventId = request.EventId;

            var routeSegment = request.RouteSegment;
            var startNode = (await _geoDatabase.GetIntersectingStartRouteNodes(routeSegment)).FirstOrDefault();
            var endNode = (await _geoDatabase.GetIntersectingEndRouteNodes(routeSegment)).FirstOrDefault();

            if (startNode is null)
            {
                var startPoint = routeSegment.FindStartPoint();
                startNode = _routeNodeFactory.Create(startPoint);
                await _geoDatabase.InsertRouteNode(startNode);
                await _mediator.Publish(new RouteNodeAdded
                    {
                        RouteNode = startNode,
                        EventId = eventId,
                        CmdType = nameof(NewRouteSegmentDigitizedByUser)
                    });
            }
            if (endNode is null)
            {
                var endPoint = routeSegment.FindStartPoint();
                endNode = _routeNodeFactory.Create(endPoint);
                await _geoDatabase.InsertRouteNode(endNode);
                await _mediator.Publish(new RouteNodeAdded
                    {
                        RouteNode = endNode,
                        EventId = eventId,
                        CmdType = nameof(NewRouteSegmentDigitizedByUser)
                    });
            }

            await _mediator.Publish(new RouteSegmentAdded
            {
                EventId = eventId,
                RouteSegment = routeSegment,
                StartRouteNode = startNode,
                EndRouteNode = endNode,
                CmdType = nameof(NewRouteSegmentDigitizedByUser)
            });
        }
    }
}
