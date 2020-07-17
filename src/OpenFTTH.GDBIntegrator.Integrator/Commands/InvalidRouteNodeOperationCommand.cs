using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace OpenFTTH.GDBIntegrator.Integrator.Commands
{
    public class InvalidRouteNodeOperationCommand : IRequest
    {
        public RouteNode RouteNode { get; set; }
    }

    public class InvalidRouteNodeOperationCommandHandler : IRequestHandler<InvalidRouteNodeOperationCommand, Unit>
    {
        private readonly IGeoDatabase _geoDatabase;
        private readonly ILogger<InvalidRouteNodeOperationCommandHandler> _logger;

        public InvalidRouteNodeOperationCommandHandler(IGeoDatabase geoDatabase, ILogger<InvalidRouteNodeOperationCommandHandler> logger)
        {
            _geoDatabase = geoDatabase;
            _logger = logger;
        }

        public async Task<Unit> Handle(InvalidRouteNodeOperationCommand request, CancellationToken token)
        {
            _logger.LogInformation($"Deleteting {nameof(RouteNode)} with mrid '{request.RouteNode.Mrid}'");
            await _geoDatabase.DeleteRouteNode(request.RouteNode.Mrid);

            return await Task.FromResult(new Unit());
        }
    }
}
