using System;
using System.Text;
using System.Linq;
using Topos.Serialization;
using Newtonsoft.Json.Linq;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.RouteNetwork.Mapping;
using OpenFTTH.GDBIntegrator.Integrator.ConsumerMessages;
using OpenFTTH.Events.Core.Infos;
using OpenFTTH.Events.RouteNetwork.Infos;

namespace OpenFTTH.GDBIntegrator.Subscriber.Kafka.Serialize
{
    public class RouteNetworkSerializer : IMessageSerializer
    {
        private readonly IInfoMapper _infoMapper;

        public RouteNetworkSerializer(IInfoMapper infoMapper)
        {
            _infoMapper = infoMapper;
        }

        public ReceivedLogicalMessage Deserialize(ReceivedTransportMessage message)
        {
            if (message is null)
                throw new ArgumentNullException($"{nameof(ReceivedTransportMessage)} is null");

            if (message.Body is null || message.Body.Length == 0)
                return new ReceivedLogicalMessage(message.Headers, new RouteNodeMessage(), message.Position);

            dynamic payload = null;

            try
            {
                var messageBody = Encoding.UTF8.GetString(message.Body, 0, message.Body.Length);

                dynamic topicMessageBody = JObject.Parse(messageBody);
                payload = topicMessageBody.payload;

                if (IsTombStoneMessage(payload))
                    return new ReceivedLogicalMessage(message.Headers, new RouteSegmentMessage(), message.Position);

                if (payload.source.table.ToString() == "route_segment")
                {
                    var routeSegmentMessage = (RouteSegmentMessage)CreateRouteSegmentMessage(payload);

                    if (routeSegmentMessage?.After != null && routeSegmentMessage.After?.Coord is null)
                    {
                        // Cannot roll back this should never happen, but we have seen it once.
                        if (routeSegmentMessage?.Before != null && routeSegmentMessage.Before?.Coord is null)
                        {
                            // In case this happens we delete the node.
                            return new ReceivedLogicalMessage(message.Headers, new InvalidMessage(routeSegmentMessage, true), message.Position);
                        }

                        return new ReceivedLogicalMessage(message.Headers, new InvalidMessage(routeSegmentMessage), message.Position);
                    }

                    return new ReceivedLogicalMessage(message.Headers, routeSegmentMessage, message.Position);
                }
                else if (payload.source.table.ToString() == "route_node")
                {
                    var routeNodeMessage = (RouteNodeMessage)CreateRouteNodeMessage(payload);

                    if (routeNodeMessage?.After != null && routeNodeMessage.After?.Coord is null)
                    {
                        // Cannot roll back this should never happen, but we have seen it once.
                        if (routeNodeMessage?.Before != null && routeNodeMessage.Before?.Coord is null)
                        {
                            // In case this happens we delete the node.
                            return new ReceivedLogicalMessage(message.Headers, new InvalidMessage(routeNodeMessage, true), message.Position);
                        }

                        return new ReceivedLogicalMessage(message.Headers, new InvalidMessage(routeNodeMessage), message.Position);
                    }

                    return new ReceivedLogicalMessage(message.Headers, routeNodeMessage, message.Position);
                }
            }
            catch (Exception e)
            {
                // TODO rewrite to error logging
                Console.WriteLine(e.StackTrace + "\n" + "With object: " + Newtonsoft.Json.JsonConvert.SerializeObject(payload, Newtonsoft.Json.Formatting.Indented));
            }

            throw new Exception($"No valid deserilizaiton for type {payload.source.table.ToString()}");
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
            var mappedRouteSegment = new RouteSegment
            {
                Mrid = new Guid(routeSegment.mrid.ToString()),
                Coord = (JObject)routeSegment.coord is null ? null : Convert.FromBase64String(routeSegment.coord.wkb.ToString()),
                Username = routeSegment.user_name.ToString(),
                WorkTaskMrid = routeSegment.work_task_mrid.ToString() == string.Empty ? System.Guid.Empty : new Guid(routeSegment.work_task_mrid.ToString()),
                ApplicationName = routeSegment.application_name.ToString(),
                MarkAsDeleted = (bool)routeSegment.marked_to_be_deleted,
                ApplicationInfo = routeSegment.application_info.ToString(),
                DeleteMe = (bool)routeSegment.delete_me,

                LifeCycleInfo = new LifecycleInfo(
                     _infoMapper.MapDeploymentState((string)routeSegment.lifecycle_deployment_state),
                     (DateTime?)routeSegment.lifecycle_installation_date,
                     (DateTime?)routeSegment.lifecycle_removal_date
                     ),
                MappingInfo = new MappingInfo(
                     _infoMapper.MapMappingMethod((string)routeSegment.mapping_method),
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
                     _infoMapper.MapRouteSegmentKind((string)routeSegment.routesegment_kind),
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

        private RouteNodeMessage CreateRouteNodeMessage(dynamic payload)
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
            var mappedRouteNode = new RouteNode
            {
                ApplicationInfo = routeNode.application_info.ToString(),
                ApplicationName = routeNode.application_name.ToString(),
                Coord = (JObject)routeNode.coord is null ? null : Convert.FromBase64String(routeNode.coord.wkb.ToString()),
                MarkAsDeleted = (bool)routeNode.marked_to_be_deleted,
                DeleteMe = (bool)routeNode.delete_me,
                Mrid = new Guid(routeNode.mrid.ToString()),
                Username = routeNode.user_name.ToString(),
                WorkTaskMrid = routeNode.work_task_mrid.ToString() == string.Empty ? System.Guid.Empty : new Guid(routeNode.work_task_mrid.ToString()),

                LifeCycleInfo = new LifecycleInfo(
                    _infoMapper.MapDeploymentState((string)routeNode.lifecycle_deployment_state),
                    (DateTime?)routeNode.lifecycle_installation_date,
                    (DateTime?)routeNode.lifecycle_removal_date
                    ),
                MappingInfo = new MappingInfo(
                    _infoMapper.MapMappingMethod((string)routeNode.mapping_method),
                    (string)routeNode.mapping_vertical_accuracy,
                    (string)routeNode.mapping_horizontal_accuracy,
                    (DateTime?)routeNode.mapping_survey_date,
                    (string)routeNode.mapping_source_info
                    ),
                NamingInfo = new NamingInfo(
                    (string)routeNode.naming_name,
                    (string)routeNode.naming_description
                    ),
                RouteNodeInfo = new RouteNodeInfo(
                    _infoMapper.MapRouteNodeKind((string)routeNode.routenode_kind),
                    _infoMapper.MapRouteNodeFunction((string)routeNode.routenode_function)
                    ),
                SafetyInfo = new SafetyInfo(
                    (string)routeNode.safety_classification,
                    (string)routeNode.safety_remark
                    )
            };

            // Make fully empty objects into nulls.
            mappedRouteNode.LifeCycleInfo = AreAnyPropertiesNotNull<LifecycleInfo>(mappedRouteNode.LifeCycleInfo) ? mappedRouteNode.LifeCycleInfo : null;
            mappedRouteNode.MappingInfo = AreAnyPropertiesNotNull<MappingInfo>(mappedRouteNode.MappingInfo) ? mappedRouteNode.MappingInfo : null;
            mappedRouteNode.NamingInfo = AreAnyPropertiesNotNull<NamingInfo>(mappedRouteNode.NamingInfo) ? mappedRouteNode.NamingInfo : null;
            mappedRouteNode.RouteNodeInfo = AreAnyPropertiesNotNull<RouteNodeInfo>(mappedRouteNode.RouteNodeInfo) ? mappedRouteNode.RouteNodeInfo : null;
            mappedRouteNode.SafetyInfo = AreAnyPropertiesNotNull<SafetyInfo>(mappedRouteNode.SafetyInfo) ? mappedRouteNode.SafetyInfo : null;

            return mappedRouteNode;
        }

        private bool AreAnyPropertiesNotNull<T>(object obj)
        {
            return typeof(T).GetProperties().Any(propertyInfo => propertyInfo.GetValue(obj) != null);
        }

        private bool IsTombStoneMessage(dynamic payload)
        {
            JToken afterPayload = payload["after"];
            return afterPayload.Type == JTokenType.Null;
        }

        public TransportMessage Serialize(LogicalMessage message)
        {
            throw new NotImplementedException();
        }
    }
}
