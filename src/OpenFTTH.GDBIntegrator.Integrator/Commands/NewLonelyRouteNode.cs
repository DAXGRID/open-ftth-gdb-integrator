using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.Config;
using OpenFTTH.GDBIntegrator.Producer;
using OpenFTTH.GDBIntegrator.Integrator.EventMessages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OpenFTTH.GDBIntegrator.Integrator.Commands
{
    public class NewLonelyRouteNode : IRequest
    {
        public RouteNode RouteNode { get; set; }
    }

    public class NewLonelyRouteNodeHandler : IRequestHandler<NewLonelyRouteNode, Unit>
    {
        private readonly ILogger<NewLonelyRouteNodeHandler> _logger;
        private readonly IProducer _producer;
        private readonly KafkaSetting _kafkaSetting;

        public NewLonelyRouteNodeHandler(
            ILogger<NewLonelyRouteNodeHandler> logger,
            IProducer producer,
            IOptions<KafkaSetting> kafkaSetting
            )
        {
            _logger = logger;
            _producer = producer;
            _kafkaSetting = kafkaSetting.Value;
        }

        public async Task<Unit> Handle(NewLonelyRouteNode request, CancellationToken cancellationToken)
        {
            if (request.RouteNode is null)
                throw new ArgumentNullException($"{nameof(RouteNode)} cannot be null.");

            _logger.LogInformation($"{DateTime.UtcNow.ToString("o")}: Starting - New lonely routenode.\n");

            var eventId = Guid.NewGuid();
            await _producer.Produce(_kafkaSetting.EventRouteNetworkTopicName, new RouteNodeAdded(
                             eventId,
                             request.RouteNode.Mrid,
                             request.RouteNode.GetGeoJsonCoordinate()));

            _logger.LogInformation($"{DateTime.UtcNow.ToString("o")}: Finished - New lonely routenode.\n");

            return await Task.FromResult(new Unit());
        }
    }
}
