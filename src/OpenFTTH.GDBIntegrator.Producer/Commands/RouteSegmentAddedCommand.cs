using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Topos.Producer;

namespace OpenFTTH.GDBIntegrator.Producer.Commands
{
    public class RouteSegmentAddedCommand : IRequest
    {
        public string EventType => "RouteSegmentAddedCommand";
        public string EventId { get; set; }
        public string EventTs => DateTime.UtcNow.ToString();
        public string CmdId => Guid.NewGuid().ToString();
        public string SegmentId { get; set; }
        public string FromNodeId { get; set; }
        public string ToNodeId { get; set; }
        public string Geometry { get; set; }
    }

    public class RouteSegmentAddedCommandHandler : IRequestHandler<RouteSegmentAddedCommand>
    {
        private readonly IProducer _producer;

        public RouteSegmentAddedCommandHandler(IProducer producer)
        {
            _producer = producer;
        }

        public async Task<Unit> Handle(RouteSegmentAddedCommand request, CancellationToken cancellationToken)
        {
            await _producer.Produce("event.route-network", new ToposMessage(request));
            return default;
        }
    }
}
