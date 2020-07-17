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
    public class NewRouteSegmentBetweenTwoExistingNodes : IRequest
    {
        public RouteSegment RouteSegment { get; set; }
        public RouteNode StartRouteNode { get; set; }
        public RouteNode EndRouteNode { get; set; }
    }

    public class NewRouteSegmentBetweenTwoExistingNodesHandler : IRequestHandler<NewRouteSegmentBetweenTwoExistingNodes, Unit>
    {
        private readonly ILogger<NewRouteSegmentBetweenTwoExistingNodesHandler> _logger;
        private readonly IProducer _producer;
        private readonly IOptions<KafkaSetting> _kafkaSetting;

        public NewRouteSegmentBetweenTwoExistingNodesHandler(
            ILogger<NewRouteSegmentBetweenTwoExistingNodesHandler> logger,
            IProducer producer,
            IOptions<KafkaSetting> kafkaSetting
            )
        {
            _logger = logger;
            _producer = producer;
            _kafkaSetting = kafkaSetting;
        }

        public async Task<Unit> Handle(NewRouteSegmentBetweenTwoExistingNodes request, CancellationToken cancellationToken)
        {
            if (request.RouteSegment is null)
                throw new ArgumentNullException($"{nameof(RouteSegment)} cannot be null.");

            _logger.LogInformation($"{DateTime.UtcNow.ToString("o")}: Starting - New route segment between two existing nodes.\n");

            await _producer.Produce(_kafkaSetting.Value.EventRouteNetworkTopicName,
                                    new RouteSegmentAdded(
                                        Guid.NewGuid(),
                                        request.RouteSegment.Mrid,
                                        request.StartRouteNode.Mrid,
                                        request.EndRouteNode.Mrid,
                                        request.RouteSegment.GetGeoJsonCoordinate()));

            _logger.LogInformation($"{DateTime.UtcNow.ToString("o")}: Finished - New route segment between two existing nodes.\n");

            return await Task.FromResult(new Unit());
        }
    }
}
