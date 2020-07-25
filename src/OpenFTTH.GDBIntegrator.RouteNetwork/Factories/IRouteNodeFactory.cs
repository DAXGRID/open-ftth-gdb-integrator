using NetTopologySuite.Geometries;

namespace OpenFTTH.GDBIntegrator.RouteNetwork.Factories
{
    public interface IRouteNodeFactory
    {
        RouteNode Create(Point point);
    }
}
