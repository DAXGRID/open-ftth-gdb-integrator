using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Topos.Producer;
using Microsoft.Extensions.Logging;
using OpenFTTH.GDBIntegrator.Producer;

namespace OpenFTTH.GDBIntegrator.Integrator.Commands
{
    public class RouteSegmentAddedCommand : IRequest
    {
        public string EventType => "RouteSegmentAddedCommand";
        public string EventId { get; set; }
        public string EventTs => DateTime.UtcNow.ToString();
        public string CmdId { get; set; }
        public string SegmentId { get; set; }
        public string FromNodeId { get; set; }
        public string ToNodeId { get; set; }
        public string Geometry { get; set; }
    }

    public class RouteSegmentAddedCommandHandler : AsyncRequestHandler<RouteSegmentAddedCommand>
    {
        private readonly IProducer _producer;
        private readonly ILogger _logger;

        public RouteSegmentAddedCommandHandler(IProducer producer, ILogger<RouteSegmentAddedCommandHandler> logger)
        {
            _producer = producer;
            _logger = logger;
        }

        protected override async Task Handle(RouteSegmentAddedCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Producing RouteSegmentAdded event to event.route-network", request);
            await _producer.Produce("event.route-network", new ToposMessage(request));
        }
    }
}
