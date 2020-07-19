using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.RouteNetwork.Validators;
using OpenFTTH.GDBIntegrator.Config;
using OpenFTTH.GDBIntegrator.Producer;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using OpenFTTH.GDBIntegrator.Integrator.Notifications;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OpenFTTH.GDBIntegrator.Integrator.Commands
{
    public class GeoDatabaseUpdated : IRequest
    {
        public object UpdatedEntity { get; set; }
    }

    public class GeoDatabaseUpdatedHandler : IRequestHandler<GeoDatabaseUpdated, Unit>
    {
        private static Semaphore _pool = new Semaphore(1, 1);
        private readonly ILogger<RouteNodeAddedHandler> _logger;
        private readonly KafkaSetting _kafkaSettings;
        private readonly IProducer _producer;
        private readonly IRouteSegmentValidator _routeSegmentValidator;
        private readonly IMediator _mediator;
        private readonly ApplicationSetting _applicationSettings;
        private readonly IGeoDatabase _geoDatabase;

        public GeoDatabaseUpdatedHandler(
            ILogger<RouteNodeAddedHandler> logger,
            IOptions<KafkaSetting> kafkaSettings,
            IProducer producer,
            IRouteSegmentValidator routeSegmentValidator,
            IMediator mediator,
            IOptions<ApplicationSetting> applicationSettings,
            IGeoDatabase geoDatabase)
        {
            _logger = logger;
            _kafkaSettings = kafkaSettings.Value;
            _producer = producer;
            _routeSegmentValidator = routeSegmentValidator;
            _mediator = mediator;
            _applicationSettings = applicationSettings.Value;
            _geoDatabase = geoDatabase;
        }

        public async Task<Unit> Handle(GeoDatabaseUpdated request, CancellationToken token)
        {
            try
            {
                _pool.WaitOne();
                if (request.UpdatedEntity is RouteNode)
                {
                    var notificationEvent = await HandleRouteNodeUpdated((RouteNode)request.UpdatedEntity);
                    await _mediator.Publish(notificationEvent);
                }
                else if (request.UpdatedEntity is RouteSegment)
                {
                    var notificationEvent = await HandleRouteSegmentUpdated((RouteSegment)request.UpdatedEntity);
                    await _mediator.Publish(notificationEvent);
                }
                _pool.Release();
            }
            catch(Exception e)
            {
                _logger.LogError(e.Message);
                _pool.Release();
            }

            return await Task.FromResult(new Unit());
        }

        private async Task<INotification> HandleRouteNodeUpdated(RouteNode routeNode)
        {
            var eventId = Guid.NewGuid();

            if (routeNode is null)
                throw new ArgumentNullException($"Parameter {nameof(routeNode)} cannot be null");

            // If the GDB integrator produced the message do nothing
            if (routeNode.ApplicationName == _applicationSettings.ApplicationName)
                return null;

            var intersectingRouteSegments = await _geoDatabase.GetIntersectingRouteSegments(routeNode);
            _logger.LogWarning("Count" + intersectingRouteSegments.Count);

            if (intersectingRouteSegments.Count == 0)
                return new RouteNodeAdded { EventId = eventId, RouteNode = routeNode };

            if (intersectingRouteSegments.Count == 1)
            {
                return new ExistingRouteSegmentSplittedByUser
                {
                    RouteNode = routeNode,
                    IntersectingRouteSegment = intersectingRouteSegments.FirstOrDefault(),
                    EventId = eventId
                };
            }

            return new InvalidRouteNodeOperation { RouteNode = routeNode, EventId = eventId };
        }

        private async Task<INotification> HandleRouteSegmentUpdated(RouteSegment routeSegment)
        {
            var eventId = Guid.NewGuid();

            if (routeSegment.ApplicationName == _applicationSettings.ApplicationName)
                return null;

            if (!_routeSegmentValidator.LineIsValid(routeSegment.GetLineString()))
                return new InvalidRouteSegmentOperation { RouteSegment = routeSegment, EventId = eventId };

            var intersectingStartNodes = await _geoDatabase.GetIntersectingStartRouteNodes(routeSegment);
            var intersectingEndNodes = await _geoDatabase.GetIntersectingEndRouteNodes(routeSegment);

            var totalIntersectingNodes = intersectingStartNodes.Count + intersectingEndNodes.Count;

            if (intersectingStartNodes.Count <= 1 && intersectingEndNodes.Count <= 1)
            {
                return new NewRouteSegmentDigitizedByUser
                {
                    RouteSegment = routeSegment,
                    StartRouteNode = intersectingStartNodes.FirstOrDefault(),
                    EndRouteNode = intersectingEndNodes.FirstOrDefault(),
                    EventId = eventId
                };
            }

            return new InvalidRouteSegmentOperation { RouteSegment = routeSegment, EventId = eventId };
        }
    }
}
