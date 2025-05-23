//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace RosMessageTypes.Msgs
{
    [Serializable]
    public class GetMotionSequenceRequest : Message
    {
        public const string k_RosMessageName = "msgs/GetMotionSequence";
        public override string RosMessageName => k_RosMessageName;

        //  Planning request with a list of motion commands
        public MotionSequenceRequestMsg request;

        public GetMotionSequenceRequest()
        {
            this.request = new MotionSequenceRequestMsg();
        }

        public GetMotionSequenceRequest(MotionSequenceRequestMsg request)
        {
            this.request = request;
        }

        public static GetMotionSequenceRequest Deserialize(MessageDeserializer deserializer) => new GetMotionSequenceRequest(deserializer);

        private GetMotionSequenceRequest(MessageDeserializer deserializer)
        {
            this.request = MotionSequenceRequestMsg.Deserialize(deserializer);
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.request);
        }

        public override string ToString()
        {
            return "GetMotionSequenceRequest: " +
            "\nrequest: " + request.ToString();
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#else
        [UnityEngine.RuntimeInitializeOnLoadMethod]
#endif
        public static void Register()
        {
            MessageRegistry.Register(k_RosMessageName, Deserialize);
        }
    }
}
