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
    public class NewRouteNodeDigitized : INotification
    {
        public RouteNode RouteNode { get; set; }
        public Guid CmdId { get; set; }
        public string CmdType { get; set; }
        public bool? IsLastEventInCmd { get; set; }
    }

    public class NewRouteNodeDigitizedHandler : INotificationHandler<NewRouteNodeDigitized>
    {
        private readonly ILogger<NewRouteNodeDigitizedHandler> _logger;
        private readonly KafkaSetting _kafkaSettings;
        private readonly IMediator _mediator;
        private readonly IGeoDatabase _geoDatabase;

        public NewRouteNodeDigitizedHandler(
            ILogger<NewRouteNodeDigitizedHandler> logger,
            IOptions<KafkaSetting> kafkaSettings,
            IMediator mediator,
            IGeoDatabase geoDatabase)
        {
            _logger = logger;
            _kafkaSettings = kafkaSettings.Value;
            _mediator = mediator;
            _geoDatabase = geoDatabase;
        }

        public async Task Handle(NewRouteNodeDigitized request, CancellationToken token)
        {
            _logger.LogDebug($"Sending {nameof(NewRouteNodeDigitized)} with mrid '{request.RouteNode.Mrid}' to producer");

            await _geoDatabase.InsertRouteNode(request.RouteNode);
            await _mediator.Publish(new RouteNodeAdded
                {
                    RouteNode = request.RouteNode,
                    CmdId = request.CmdId,
                    CmdType = request.CmdType ?? nameof(NewRouteNodeDigitized),
                    IsLastEventInCmd = request.IsLastEventInCmd ?? true
                });
        }
    }
}
