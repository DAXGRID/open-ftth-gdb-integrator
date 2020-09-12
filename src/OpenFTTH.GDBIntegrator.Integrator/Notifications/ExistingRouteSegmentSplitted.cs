using System;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.RouteNetwork.Factories;
using OpenFTTH.GDBIntegrator.Integrator.Factories;
using OpenFTTH.GDBIntegrator.Integrator.Store;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using OpenFTTH.Events;
using OpenFTTH.Events.RouteNetwork;
using Microsoft.Extensions.Logging;

namespace OpenFTTH.GDBIntegrator.Integrator.Notifications
{
    public class ExistingRouteSegmentSplitted : INotification
    {
        public RouteNode RouteNode { get; set; }
        public RouteSegment RouteSegmentDigitizedByUser { get; set; }
        public bool InsertNode { get; set; }
        public bool CreateNodeAddedEvent { get; set; }
    }

    public class ExistingRouteSegmentSplittedHandler : INotificationHandler<ExistingRouteSegmentSplitted>
    {
        private readonly ILogger<ExistingRouteSegmentSplittedHandler> _logger;
        private readonly IGeoDatabase _geoDatabase;
        private readonly IRouteSegmentFactory _routeSegmentFactory;
        private readonly IRouteSegmentEventFactory _routeSegmentEventFactory;
        private readonly IRouteNodeEventFactory _routeNodeEventFactory;
        private readonly IEventStore _eventStore;

        public ExistingRouteSegmentSplittedHandler(
            ILogger<ExistingRouteSegmentSplittedHandler> logger,
            IGeoDatabase geoDatabase,
            IRouteSegmentFactory routeSegmentFactory,
            IRouteSegmentEventFactory routeSegmentEventFactory,
            IRouteNodeEventFactory routeNodeEventFactory,
            IEventStore eventStore)
        {
            _logger = logger;
            _geoDatabase = geoDatabase;
            _routeSegmentFactory = routeSegmentFactory;
            _routeSegmentEventFactory = routeSegmentEventFactory;
            _routeNodeEventFactory = routeNodeEventFactory;
            _eventStore = eventStore;
        }

        public async Task Handle(ExistingRouteSegmentSplitted request, CancellationToken token)
        {
            _logger.LogInformation($"Starting {nameof(ExistingRouteSegmentSplittedHandler)}");

            var domainEvents = new List<RouteNetworkEvent>();
            if (request.InsertNode)
            {
                await _geoDatabase.InsertRouteNode(request.RouteNode);
                var routeNodeAddedEvent = _routeNodeEventFactory.CreateAdded(request.RouteNode);
                domainEvents.Add(routeNodeAddedEvent);
            }

            if (request.CreateNodeAddedEvent && !request.InsertNode)
            {
                var routeNodeAddedEvent = _routeNodeEventFactory.CreateAdded(request.RouteNode);
                domainEvents.Add(routeNodeAddedEvent);
            }

            var intersectingRouteSegment = await GetIntersectingRouteSegment(request.RouteSegmentDigitizedByUser, request.RouteNode);

            var routeSegmentsWkt = await _geoDatabase.GetRouteSegmentsSplittedByRouteNode(request.RouteNode, intersectingRouteSegment);
            var routeSegments = _routeSegmentFactory.Create(routeSegmentsWkt);

            SetSplittedRouteSegmentValuesToNewRouteSegments(routeSegments, intersectingRouteSegment);

            var routeSegmentAddedEvents = await InsertReplacementRouteSegments(routeSegments);
            domainEvents.AddRange(routeSegmentAddedEvents);

            var routeSegmentMarkedForDeletionEvent = await DeleteRouteSegment(intersectingRouteSegment, routeSegments);
            domainEvents.Add(routeSegmentMarkedForDeletionEvent);

            var cmdId = Guid.NewGuid();
            var routeNetworkCommand = new RouteNetworkCommand(nameof(ExistingRouteSegmentSplitted), cmdId, domainEvents.ToArray());
            _eventStore.Insert(routeNetworkCommand);
        }

        private void SetSplittedRouteSegmentValuesToNewRouteSegments(List<RouteSegment> routeSegments, RouteSegment splittedRouteSegment)
        {
            foreach (var routeSegment in routeSegments)
            {
                routeSegment.WorkTaskMrid = splittedRouteSegment.WorkTaskMrid;
                routeSegment.ApplicationInfo = splittedRouteSegment.ApplicationInfo;
                routeSegment.MappingInfo = splittedRouteSegment.MappingInfo;
                routeSegment.LifeCycleInfo = splittedRouteSegment.LifeCycleInfo;
                routeSegment.NamingInfo = splittedRouteSegment.NamingInfo;
                routeSegment.RouteSegmentInfo = splittedRouteSegment.RouteSegmentInfo;
                routeSegment.SafetyInfo = splittedRouteSegment.SafetyInfo;
                routeSegment.Username = splittedRouteSegment.Username;
            }
        }

        private async Task<RouteSegment> GetIntersectingRouteSegment(RouteSegment routeSegmentDigitizedByUser, RouteNode routeNode)
        {
            RouteSegment intersectingRouteSegment = null;
            // This is if triggered by RouteNodeDigitizedByUser
            if (routeSegmentDigitizedByUser is null)
            {
                intersectingRouteSegment = await HandleIntersectionSplit(routeNode);
            }
            // This is required in case that this event was triggered by RouteSegmentDigtizedByUser
            else
            {
                intersectingRouteSegment = (await _geoDatabase.GetIntersectingRouteSegments(routeNode, routeSegmentDigitizedByUser)).First();
            }

            return intersectingRouteSegment;
        }

        private async Task<RouteSegment> HandleIntersectionSplit(RouteNode routeNode)
        {
            RouteSegment intersectingRouteSegment = null;
            var intersectingRouteSegments = await _geoDatabase.GetIntersectingRouteSegments(routeNode);
            foreach (var individualIntersectingRouteSegment in intersectingRouteSegments)
            {
                intersectingRouteSegment = individualIntersectingRouteSegment;
                var intersectingRouteNodesCount = (await _geoDatabase.GetAllIntersectingRouteNodes(individualIntersectingRouteSegment)).Count;

                if (intersectingRouteNodesCount >= 3)
                    break;
            }

            return intersectingRouteSegment;
        }

        private async Task<List<RouteNetworkEvent>> InsertReplacementRouteSegments(List<RouteSegment> routeSegments)
        {
            var routeNetworkEvents = new List<RouteNetworkEvent>();

            foreach (var routeSegment in routeSegments)
            {
                await _geoDatabase.InsertRouteSegment(routeSegment);

                var startRouteNode = (await _geoDatabase.GetIntersectingStartRouteNodes(routeSegment)).FirstOrDefault();
                var endRouteNode = (await _geoDatabase.GetIntersectingEndRouteNodes(routeSegment)).FirstOrDefault();

                var routeSegmentAddedEvent = _routeSegmentEventFactory.CreateAdded(routeSegment, startRouteNode, endRouteNode);
                routeNetworkEvents.Add(routeSegmentAddedEvent);
            }

            return routeNetworkEvents;
        }

        private async Task<RouteSegmentRemoved> DeleteRouteSegment(RouteSegment intersectingRouteSegment, List<RouteSegment> routeSegments)
        {
            await _geoDatabase.DeleteRouteSegment(intersectingRouteSegment.Mrid);
            return _routeSegmentEventFactory.CreateRemoved(intersectingRouteSegment, routeSegments.Select(x => x.Mrid));
        }
    }
}
