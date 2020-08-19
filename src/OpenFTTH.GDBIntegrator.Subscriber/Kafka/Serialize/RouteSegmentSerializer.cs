using System;
using System.Text;
using System.Linq;
using Topos.Serialization;
using Newtonsoft.Json.Linq;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.Subscriber.Kafka.Serialize.Mapper;
using OpenFTTH.GDBIntegrator.Integrator.ConsumerMessages;
using OpenFTTH.Events.Core.Infos;
using OpenFTTH.Events.RouteNetwork.Infos;

namespace OpenFTTH.GDBIntegrator.Subscriber.Kafka.Serialize
{
    public class RouteSegmentSerializer : IMessageSerializer
    {
        private readonly ISerializationMapper _serializationMapper;

        public RouteSegmentSerializer(ISerializationMapper serializationMapper)
        {
            _serializationMapper = serializationMapper;
        }

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
           var mappedRouteSegment =  new RouteSegment
            {
                Mrid = new Guid(routeSegment.mrid.ToString()),
                Coord = Convert.FromBase64String(routeSegment.coord.wkb.ToString()),
                Username = routeSegment.user_name.ToString(),
                WorkTaskMrid = routeSegment.work_task_mrid.ToString() == string.Empty ? System.Guid.Empty : new Guid(routeSegment.work_task_mrid.ToString()),
                ApplicationName = routeSegment.application_name.ToString(),
                MarkAsDeleted = (bool)routeSegment.marked_to_be_deleted,
                ApplicationInfo = routeSegment.application_info.ToString(),
                DeleteMe = (bool)routeSegment.delete_me,
                LifeCycleInfo = new LifecycleInfo(
                    _serializationMapper.MapDeploymentState((string)routeSegment.lifecycle_deployment_state),
                    (DateTime?)routeSegment.lifecycle_installation_date,
                    (DateTime?)routeSegment.lifecycle_removal_date
                    ),
                MappingInfo = new MappingInfo(
                    _serializationMapper.MapMappingMethod((string)routeSegment.mapping_method),
                    (string)routeSegment.mapping_vertical_accuracy,
                    (string)routeSegment.mapping_horizontal_accuracy,
                    (DateTime?)routeSegment.mapping_survey_date,
                    (string)routeSegment.mapping_source_info
                    ),
                NamingInfo = new NamingInfo(
                    (string)routeSegment.naming_name,
                    (string)routeSegment.naming_description
                    ),
                SafetyInfo = new SafetyInfo(
                    (string)routeSegment.safety_classification,
                    (string)routeSegment.safety_remark
                    ),
                RouteSegmentInfo = new RouteSegmentInfo(
                    _serializationMapper.MapRouteSegmentKind((string)routeSegment.routesegment_kind),
                    (string)routeSegment.routesegment_width,
                    (string)routeSegment.routesegment_height
                    )
            };

            // Make fully empty objects into nulls.
            mappedRouteSegment.LifeCycleInfo = AreAnyPropertiesNotNull<LifecycleInfo>(mappedRouteSegment.LifeCycleInfo) ? mappedRouteSegment.LifeCycleInfo : null;
            mappedRouteSegment.MappingInfo = AreAnyPropertiesNotNull<MappingInfo>(mappedRouteSegment.MappingInfo) ? mappedRouteSegment.MappingInfo : null;
            mappedRouteSegment.NamingInfo = AreAnyPropertiesNotNull<NamingInfo>(mappedRouteSegment.NamingInfo) ? mappedRouteSegment.NamingInfo : null;
            mappedRouteSegment.RouteSegmentInfo = AreAnyPropertiesNotNull<RouteSegmentInfo>(mappedRouteSegment.RouteSegmentInfo) ? mappedRouteSegment.RouteSegmentInfo : null;
            mappedRouteSegment.SafetyInfo = AreAnyPropertiesNotNull<SafetyInfo>(mappedRouteSegment.SafetyInfo) ? mappedRouteSegment.SafetyInfo : null;

            return mappedRouteSegment;
        }

        private bool AreAnyPropertiesNotNull<T>(object obj)
        {
            return typeof(T).GetProperties().Any(propertyInfo => propertyInfo.GetValue(obj) != null);
        }

        public TransportMessage Serialize(LogicalMessage message)
        {
            throw new NotImplementedException();
        }
    }
}
