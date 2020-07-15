using System;
using System.Text;
using Topos.Serialization;
using Newtonsoft.Json.Linq;
using OpenFTTH.GDBIntegrator.RouteNetwork;

namespace OpenFTTH.GDBIntegrator.Subscriber.Kafka.Serialize
{
    public class RouteNodeSerializer : IMessageSerializer
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

            if (IsTombStoneMessage(payload))
                return new ReceivedLogicalMessage(message.Headers, new RouteSegment(), message.Position);

            var routeNode = CreateRouteNodeOnPayload(payload);

            return new ReceivedLogicalMessage(message.Headers, routeNode, message.Position);
        }

        private bool IsTombStoneMessage(dynamic payload)
        {
            JToken afterPayload = payload["after"];
            return afterPayload.Type == JTokenType.Null;
        }

        private RouteNode CreateRouteNodeOnPayload(dynamic payload)
        {
            var payloadAfter = payload.after;

            return new RouteNode
                (
                    new Guid(payloadAfter.mrid.ToString()),
                    Convert.FromBase64String(payloadAfter.coord.wkb.ToString()),
                    payloadAfter.work_task_mrid.ToString() == string.Empty ? System.Guid.Empty : new Guid(payloadAfter.work_task_mrid.ToString()),
                    payloadAfter.user_name.ToString(),
                    payloadAfter.application_name.ToString()
                );
        }

        public TransportMessage Serialize(LogicalMessage message)
        {
            throw new NotImplementedException();
        }
    }
}
