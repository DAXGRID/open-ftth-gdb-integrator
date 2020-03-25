using System;
using System.Text;
using Topos.Serialization;
using Newtonsoft.Json.Linq;
using OpenFTTH.GDBIntegrator.Model;

namespace OpenFTTH.GDBIntegrator.Subscriber.Kafka.Serialize
{
    public class RouteSegmentSerializer : IMessageSerializer
    {
        public ReceivedLogicalMessage Deserialize(ReceivedTransportMessage message)
        {
            if (message.Body is null || message.Body.Length == 0)
                return new ReceivedLogicalMessage(message.Headers, new RouteSegment(), message.Position);

            var messageBody = Encoding.UTF8.GetString(message.Body, 0, message.Body.Length);

            dynamic routeSegmentMessage = JObject.Parse(messageBody);

            var payload = routeSegmentMessage.payload;
            JToken afterPayload = payload["after"];

            var isRouteSegmentDeleted = afterPayload.Type == JTokenType.Null;
            if (isRouteSegmentDeleted)
                return new ReceivedLogicalMessage(message.Headers, new RouteSegment(), message.Position);

            payload = payload.after;

            var routeSegment = new RouteSegment
            {
                Mrid = new Guid(payload.mrid.ToString()),
                Coord = payload.coord.wkb.ToString(),
                Username = payload.user_name.ToString(),
                WorkTaskMrid = payload.work_task_mrid.ToString() == string.Empty ? System.Guid.Empty : new Guid(payload.work_task_mrid),
                ApplicationName = payload.application_name.ToString()
            };

            return new ReceivedLogicalMessage(message.Headers, routeSegment, message.Position);
        }

        public TransportMessage Serialize(LogicalMessage message)
        {
            throw new NotImplementedException();
        }
    }
}
