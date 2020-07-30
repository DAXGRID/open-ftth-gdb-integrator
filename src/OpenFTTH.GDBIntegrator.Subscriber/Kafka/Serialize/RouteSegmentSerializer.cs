using System;
using System.Text;
using System.Collections;
using Topos.Serialization;
using Newtonsoft.Json.Linq;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.Integrator.ConsumerMessages;

namespace OpenFTTH.GDBIntegrator.Subscriber.Kafka.Serialize
{
    public class RouteSegmentSerializer : IMessageSerializer
    {
        public ReceivedLogicalMessage Deserialize(ReceivedTransportMessage message)
        {
            if (message is null)
                throw new ArgumentNullException($"{nameof(ReceivedTransportMessage)} is null");

            if (message.Body is null || message.Body.Length == 0)
                return new ReceivedLogicalMessage(message.Headers, new RouteSegmentMessage(), message.Position);

            var messageBody = Encoding.UTF8.GetString(message.Body, 0, message.Body.Length);

            dynamic topicMessageBody = JObject.Parse(messageBody);
            var payload = topicMessageBody.payload;

            if (IsTombStoneMessage(payload))
                return new ReceivedLogicalMessage(message.Headers, new RouteSegmentMessage(), message.Position);

            var routeSegmentMessage = CreateRouteSegmentMessage(payload);

            return new ReceivedLogicalMessage(message.Headers, routeSegmentMessage, message.Position);
        }

        private bool IsTombStoneMessage(dynamic payload)
        {
            JToken afterPayload = payload["after"];
            return afterPayload.Type == JTokenType.Null;
        }

        private RouteSegmentMessage CreateRouteSegmentMessage(dynamic payload)
        {
            var payloadBefore = payload.before;
            var payloadAfter = payload.after;

            RouteSegment routeSegmentBefore = null;
            if ((JObject)payloadBefore != null)
                routeSegmentBefore = CreateRouteSegment(payloadBefore);

            var routeSegmentAfter = CreateRouteSegment(payloadAfter);

            return new RouteSegmentMessage(routeSegmentBefore, routeSegmentAfter);
        }

        private RouteSegment CreateRouteSegment(dynamic routeSegment)
        {
           return new RouteSegment
            {
                Mrid = new Guid(routeSegment.mrid.ToString()),
                Coord = Convert.FromBase64String(routeSegment.coord.wkb.ToString()),
                Username = routeSegment.user_name.ToString(),
                WorkTaskMrid = routeSegment.work_task_mrid.ToString() == string.Empty ? System.Guid.Empty : new Guid(routeSegment.work_task_mrid.ToString()),
                ApplicationName = routeSegment.application_name.ToString(),
                MarkAsDeleted = (bool)routeSegment.marked_to_be_deleted
            };
        }

        public TransportMessage Serialize(LogicalMessage message)
        {
            throw new NotImplementedException();
        }
    }
}
