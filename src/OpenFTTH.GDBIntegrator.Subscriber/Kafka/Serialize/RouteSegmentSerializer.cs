using System;
using System.Text;
using System.Collections;
using Topos.Serialization;
using Newtonsoft.Json.Linq;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.Subscriber.Kafka.Messages;

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
            {
                routeSegmentBefore = new RouteSegment
                {
                    Mrid = new Guid(payloadBefore.mrid.ToString()),
                    Coord = Convert.FromBase64String(payloadBefore.coord.wkb.ToString()),
                    Username = payloadBefore.user_name.ToString(),
                    WorkTaskMrid = payloadBefore.work_task_mrid.ToString() == string.Empty ? System.Guid.Empty : new Guid(payloadBefore.work_task_mrid.ToString()),
                    ApplicationName = payloadBefore.application_name.ToString()
                };
            }

            var routeSegmentAfter = new RouteSegment
            {
                Mrid = new Guid(payloadAfter.mrid.ToString()),
                Coord = Convert.FromBase64String(payloadAfter.coord.wkb.ToString()),
                Username = payloadAfter.user_name.ToString(),
                WorkTaskMrid = payloadAfter.work_task_mrid.ToString() == string.Empty ? System.Guid.Empty : new Guid(payloadAfter.work_task_mrid.ToString()),
                ApplicationName = payloadAfter.application_name.ToString()
            };

            return new RouteSegmentMessage(routeSegmentBefore, routeSegmentAfter);
        }

        public TransportMessage Serialize(LogicalMessage message)
        {
            throw new NotImplementedException();
        }
    }
}
