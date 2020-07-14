using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using OpenFTTH.GDBIntegrator.Producer;
using OpenFTTH.GDBIntegrator.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OpenFTTH.GDBIntegrator.Integrator.Commands
{
    public class NewLonelyRouteSegmentCommand : IRequest
    {
        public RouteSegment RouteSegment { get; set; }
    }

    public class NewLonelyRouteSegmentCommandHandler : IRequestHandler<NewLonelyRouteSegmentCommand, Unit>
    {
        private readonly IGeoDatabase _geoDatabase;
        private readonly ILogger<NewLonelyRouteSegmentCommandHandler> _logger;
        private readonly IProducer _producer;
        private readonly KafkaSetting _kafkaSetting;

        public NewLonelyRouteSegmentCommandHandler(
            IGeoDatabase geoDatabase,
            ILogger<NewLonelyRouteSegmentCommandHandler> logger,
            IProducer producer,
            IOptions<KafkaSetting> kafkaSetting)
        {
            _geoDatabase = geoDatabase;
            _logger = logger;
            _producer = producer;
            _kafkaSetting = kafkaSetting.Value;
        }

        public async Task<Unit> Handle(NewLonelyRouteSegmentCommand request, CancellationToken cancellationToken)
        {
            if (request.RouteSegment is null)
                throw new ArgumentNullException($"{nameof(RouteSegment)} cannot be null.");

            _logger.LogInformation($"{DateTime.UtcNow.ToString("o")}: Starting - New lonely route segment.\n");

            var routeSegment = request.RouteSegment;
            var startNode = routeSegment.FindStartNode();
            var endNode = routeSegment.FindEndNode();

            await _geoDatabase.InsertRouteNode(startNode);
            await _producer.Produce(_kafkaSetting.EventRouteNetworkTopicName, startNode);

            await _geoDatabase.InsertRouteNode(endNode);
            await _producer.Produce(_kafkaSetting.EventRouteNetworkTopicName, endNode);

            _logger.LogInformation($"{DateTime.UtcNow.ToString("o")}: Finished - New lonely route segment.\n");

            return await Task.FromResult(new Unit());
        }
    }
}
