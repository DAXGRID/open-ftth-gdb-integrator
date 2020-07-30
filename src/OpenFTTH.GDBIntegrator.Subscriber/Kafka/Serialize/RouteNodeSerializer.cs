using System;
using System.Text;
using Topos.Serialization;
using Newtonsoft.Json.Linq;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.Subscriber.Kafka.Messages;

namespace OpenFTTH.GDBIntegrator.Subscriber.Kafka.Serialize
{
    public class RouteNodeSerializer : IMessageSerializer
    {
        public ReceivedLogicalMessage Deserialize(ReceivedTransportMessage message)
        {
            if (message is null)
                throw new ArgumentNullException($"{nameof(ReceivedTransportMessage)} is null");

            if (message.Body is null || message.Body.Length == 0)
                return new ReceivedLogicalMessage(message.Headers, new RouteNodeMessage(), message.Position);

            var messageBody = Encoding.UTF8.GetString(message.Body, 0, message.Body.Length);

            dynamic routeSegmentMessage = JObject.Parse(messageBody);
            var payload = routeSegmentMessage.payload;

            if (IsTombStoneMessage(payload))
                return new ReceivedLogicalMessage(message.Headers, new RouteNodeMessage(), message.Position);

            var routeNode = CreateRouteNodeOnPayload(payload);

            return new ReceivedLogicalMessage(message.Headers, routeNode, message.Position);
        }

        private bool IsTombStoneMessage(dynamic payload)
        {
            JToken afterPayload = payload["after"];
            return afterPayload.Type == JTokenType.Null;
        }

        private RouteNodeMessage CreateRouteNodeOnPayload(dynamic payload)
        {
            var payloadBefore = payload.before;
            var payloadAfter = payload.after;

            RouteNode routeNodeBefore = null;
            if ((JObject)payloadBefore != null)
                routeNodeBefore = CreateRouteNode(payloadBefore);

            var routeNodeAfter = CreateRouteNode(payloadAfter);

            return new RouteNodeMessage(routeNodeBefore, routeNodeAfter);
        }

        private RouteNode CreateRouteNode(dynamic routeNode)
        {
            return new RouteNode
                (
                    new Guid(routeNode.mrid.ToString()),
                    Convert.FromBase64String(routeNode.coord.wkb.ToString()),
                    routeNode.work_task_mrid.ToString() == string.Empty ? System.Guid.Empty : new Guid(routeNode.work_task_mrid.ToString()),
                    routeNode.user_name.ToString(),
                    routeNode.application_name.ToString(),
                    (bool)routeNode.marked_to_be_deleted
                );
        }

        public TransportMessage Serialize(LogicalMessage message)
        {
            throw new NotImplementedException();
        }
    }
}
