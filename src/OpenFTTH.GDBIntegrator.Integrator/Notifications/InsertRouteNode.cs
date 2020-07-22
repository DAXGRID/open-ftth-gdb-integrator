using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.Config;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OpenFTTH.GDBIntegrator.Integrator.Notifications
{
    public class InsertRouteNode : INotification
    {
        public RouteNode RouteNode { get; set; }
        public Guid EventId { get; set; }
    }

    public class InsertRouteNodeHandler : INotificationHandler<InsertRouteNode>
    {
        private readonly ILogger<InsertRouteNodeHandler> _logger;
        private readonly KafkaSetting _kafkaSettings;
        private readonly IMediator _mediator;
        private readonly IGeoDatabase _geoDatabase;

        public InsertRouteNodeHandler(
            ILogger<InsertRouteNodeHandler> logger,
            IOptions<KafkaSetting> kafkaSettings,
            IMediator mediator,
            IGeoDatabase geoDatabase)
        {
            _logger = logger;
            _kafkaSettings = kafkaSettings.Value;
            _mediator = mediator;
            _geoDatabase = geoDatabase;
        }


        public async Task Handle(InsertRouteNode request, CancellationToken token)
        {
            _logger.LogInformation($"Sending {nameof(InsertRouteNode)} with mrid '{request.RouteNode.Mrid}' to producer");

            await _geoDatabase.InsertRouteNode(request.RouteNode);
            await _mediator.Publish(new RouteNodeAdded { RouteNode = request.RouteNode, EventId = request.EventId });
        }
    }
}
