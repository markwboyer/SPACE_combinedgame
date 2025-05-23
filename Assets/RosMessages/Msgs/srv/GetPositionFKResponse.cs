//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace RosMessageTypes.Msgs
{
    [Serializable]
    public class GetPositionFKResponse : Message
    {
        public const string k_RosMessageName = "msgs/GetPositionFK";
        public override string RosMessageName => k_RosMessageName;

        //  The resultant vector of PoseStamped messages that contains the (stamped) poses of the requested links
        public Geometry.PoseStampedMsg[] pose_stamped;
        //  The list of link names corresponding to the poses
        public string[] fk_link_names;
        public MoveItErrorCodesMsg error_code;

        public GetPositionFKResponse()
        {
            this.pose_stamped = new Geometry.PoseStampedMsg[0];
            this.fk_link_names = new string[0];
            this.error_code = new MoveItErrorCodesMsg();
        }

        public GetPositionFKResponse(Geometry.PoseStampedMsg[] pose_stamped, string[] fk_link_names, MoveItErrorCodesMsg error_code)
        {
            this.pose_stamped = pose_stamped;
            this.fk_link_names = fk_link_names;
            this.error_code = error_code;
        }

        public static GetPositionFKResponse Deserialize(MessageDeserializer deserializer) => new GetPositionFKResponse(deserializer);

        private GetPositionFKResponse(MessageDeserializer deserializer)
        {
            deserializer.Read(out this.pose_stamped, Geometry.PoseStampedMsg.Deserialize, deserializer.ReadLength());
            deserializer.Read(out this.fk_link_names, deserializer.ReadLength());
            this.error_code = MoveItErrorCodesMsg.Deserialize(deserializer);
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.WriteLength(this.pose_stamped);
            serializer.Write(this.pose_stamped);
            serializer.WriteLength(this.fk_link_names);
            serializer.Write(this.fk_link_names);
            serializer.Write(this.error_code);
        }

        public override string ToString()
        {
            return "GetPositionFKResponse: " +
            "\npose_stamped: " + System.String.Join(", ", pose_stamped.ToList()) +
            "\nfk_link_names: " + System.String.Join(", ", fk_link_names.ToList()) +
            "\nerror_code: " + error_code.ToString();
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#else
        [UnityEngine.RuntimeInitializeOnLoadMethod]
#endif
        public static void Register()
        {
            MessageRegistry.Register(k_RosMessageName, Deserialize, MessageSubtopic.Response);
        }
    }
}
