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
            if (message is null)
                throw new ArgumentNullException($"{nameof(ReceivedTransportMessage)} is null");

            if (message.Body is null || message.Body.Length == 0)
                return new ReceivedLogicalMessage(message.Headers, new RouteSegment(), message.Position);

            var messageBody = Encoding.UTF8.GetString(message.Body, 0, message.Body.Length);

            dynamic routeSegmentMessage = JObject.Parse(messageBody);
            var payload = routeSegmentMessage.payload;

            if (IsRouteSegmentDeleted(payload))
                return new ReceivedLogicalMessage(message.Headers, new RouteSegment(), message.Position);

            var routeSegment = CreateRouteSegmentOnPayload(payload);

            return new ReceivedLogicalMessage(message.Headers, routeSegment, message.Position);
        }

        private bool IsRouteSegmentDeleted(dynamic payload)
        {
            JToken afterPayload = payload["after"];
            return afterPayload.Type == JTokenType.Null;
        }

        private RouteSegment CreateRouteSegmentOnPayload(dynamic payload)
        {
            payload = payload.after;

            var routeSegment = new RouteSegment
            {
                Mrid = new Guid(payload.mrid.ToString()),
                Coord = payload.coord.wkb.ToString(),
                Username = payload.user_name.ToString(),
                WorkTaskMrid = payload.work_task_mrid.ToString() == string.Empty ? System.Guid.Empty : new Guid(payload.work_task_mrid),
                ApplicationName = payload.application_name.ToString()
            };

            return routeSegment;
        }

        public TransportMessage Serialize(LogicalMessage message)
        {
            throw new NotImplementedException();
        }
    }
}
