using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.RouteNetwork.Validators;
using OpenFTTH.GDBIntegrator.Config;
using OpenFTTH.GDBIntegrator.Producer;
using OpenFTTH.GDBIntegrator.Integrator.Queries;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OpenFTTH.GDBIntegrator.Integrator.Notifications
{
    public class GeoDatabaseUpdated : INotification
    {
        public object UpdatedEntity { get; set; }
    }

    public class GeoDatabaseUpdatedHandler : INotificationHandler<GeoDatabaseUpdated>
    {
        private readonly ILogger<RouteNodeAddedHandler> _logger;
        private readonly KafkaSetting _kafkaSettings;
        private readonly IProducer _producer;
        private readonly IRouteSegmentValidator _routeSegmentValidator;
        private readonly IMediator _mediator;
        private readonly ApplicationSetting _applicationSettings;

        public GeoDatabaseUpdatedHandler(
            ILogger<RouteNodeAddedHandler> logger,
            IOptions<KafkaSetting> kafkaSettings,
            IProducer producer,
            IRouteSegmentValidator routeSegmentValidator,
            IMediator mediator,
            IOptions<ApplicationSetting> applicationSettings)
        {
            _logger = logger;
            _kafkaSettings = kafkaSettings.Value;
            _producer = producer;
            _routeSegmentValidator = routeSegmentValidator;
            _mediator = mediator;
            _applicationSettings = applicationSettings.Value;
        }


        public async Task Handle(GeoDatabaseUpdated request, CancellationToken token)
        {
            if (request.UpdatedEntity is RouteNode)
            {
                await HandleRouteNodeUpdated((RouteNode)request.UpdatedEntity);
            }
            else if (request.UpdatedEntity is RouteSegment)
            {
                await HandleRouteSegmentUpdated((RouteSegment)request.UpdatedEntity);
            }
        }

        private async Task HandleRouteNodeUpdated(RouteNode routeNode)
        {
            if (routeNode is null)
                throw new ArgumentNullException($"Parameter {nameof(routeNode)} cannot be null");

            // If the GDB integrator produced the message do nothing
            if (routeNode.ApplicationName == _applicationSettings.ApplicationName)
                return;

            var intersectingRouteSegments = await _mediator.Send(new GetIntersectingRouteSegmentsOnRouteNode { RouteNode = routeNode });

            if (intersectingRouteSegments.Count == 0)
            {
                await _mediator.Publish(new NewLonelyRouteNode { RouteNode = routeNode });
                return;
            }

            if (intersectingRouteSegments.Count == 1)
            {
                await _mediator.Publish(new ExistingRouteSegmentSplittedByUser
                {
                    RouteNode = routeNode,
                    IntersectingRouteSegment = intersectingRouteSegments.FirstOrDefault()
                });

                return;
            }

            await _mediator.Publish(new InvalidRouteNodeOperation { RouteNode = routeNode });
        }

        private async Task HandleRouteSegmentUpdated(RouteSegment routeSegment)
        {
            if (!_routeSegmentValidator.LineIsValid(routeSegment.GetLineString()))
            {
                await _mediator.Publish(new InvalidRouteSegmentOperation { RouteSegment = routeSegment });
                return;
            }

            var intersectingStartNodes = await _mediator.Send(
                new GetIntersectingStartRouteNodes { RouteSegment = routeSegment });
            var intersectingEndNodes = await _mediator.Send(
                new GetIntersectingEndRouteNodes { RouteSegment = routeSegment });

            var totalIntersectingNodes = intersectingStartNodes.Count + intersectingEndNodes.Count;

            if (intersectingStartNodes.Count <= 1 && intersectingEndNodes.Count <= 1)
            {
                await _mediator.Publish(new NewRouteSegmentDigitizedByUser
                {
                    RouteSegment = routeSegment,
                    StartRouteNode = intersectingStartNodes.FirstOrDefault(),
                    EndRouteNode = intersectingEndNodes.FirstOrDefault()
                });

                return;
            }

            await _mediator.Publish(new InvalidRouteSegmentOperation { RouteSegment = routeSegment });
        }
    }
}
