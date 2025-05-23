//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace RosMessageTypes.Msgs
{
    [Serializable]
    public class GetPositionIKResponse : Message
    {
        public const string k_RosMessageName = "msgs/GetPositionIK";
        public override string RosMessageName => k_RosMessageName;

        //  The returned solution 
        //  (in the same order as the list of joints specified in the IKRequest message)
        public RobotStateMsg solution;
        public MoveItErrorCodesMsg error_code;

        public GetPositionIKResponse()
        {
            this.solution = new RobotStateMsg();
            this.error_code = new MoveItErrorCodesMsg();
        }

        public GetPositionIKResponse(RobotStateMsg solution, MoveItErrorCodesMsg error_code)
        {
            this.solution = solution;
            this.error_code = error_code;
        }

        public static GetPositionIKResponse Deserialize(MessageDeserializer deserializer) => new GetPositionIKResponse(deserializer);

        private GetPositionIKResponse(MessageDeserializer deserializer)
        {
            this.solution = RobotStateMsg.Deserialize(deserializer);
            this.error_code = MoveItErrorCodesMsg.Deserialize(deserializer);
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.solution);
            serializer.Write(this.error_code);
        }

        public override string ToString()
        {
            return "GetPositionIKResponse: " +
            "\nsolution: " + solution.ToString() +
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
