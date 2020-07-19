using System;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace OpenFTTH.GDBIntegrator.Integrator.Commands
{
    public class InvalidRouteNodeOperation : IRequest
    {
        public RouteNode RouteNode { get; set; }
        public Guid EventId { get; set; }
    }

    public class InvalidRouteNodeOperationHandler : IRequestHandler<InvalidRouteNodeOperation>
    {
        private readonly IGeoDatabase _geoDatabase;
        private readonly ILogger<InvalidRouteNodeOperationHandler> _logger;

        public InvalidRouteNodeOperationHandler(IGeoDatabase geoDatabase, ILogger<InvalidRouteNodeOperationHandler> logger)
        {
            _geoDatabase = geoDatabase;
            _logger = logger;
        }

        public async Task<Unit> Handle(InvalidRouteNodeOperation request, CancellationToken token)
        {
            _logger.LogInformation($"{DateTime.UtcNow.ToString("o")}: Deleteting {nameof(RouteNode)} with mrid '{request.RouteNode.Mrid}'");
            await _geoDatabase.DeleteRouteNode(request.RouteNode.Mrid);

            return await Task.FromResult(new Unit());
        }
    }
}
