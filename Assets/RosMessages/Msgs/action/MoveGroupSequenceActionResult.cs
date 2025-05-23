using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Std;
using RosMessageTypes.Actionlib;

namespace RosMessageTypes.Msgs
{
    public class MoveGroupSequenceActionResult : ActionResult<MoveGroupSequenceResult>
    {
        public const string k_RosMessageName = "msgs/MoveGroupSequenceActionResult";
        public override string RosMessageName => k_RosMessageName;


        public MoveGroupSequenceActionResult() : base()
        {
            this.result = new MoveGroupSequenceResult();
        }

        public MoveGroupSequenceActionResult(HeaderMsg header, GoalStatusMsg status, MoveGroupSequenceResult result) : base(header, status)
        {
            this.result = result;
        }
        public static MoveGroupSequenceActionResult Deserialize(MessageDeserializer deserializer) => new MoveGroupSequenceActionResult(deserializer);

        MoveGroupSequenceActionResult(MessageDeserializer deserializer) : base(deserializer)
        {
            this.result = MoveGroupSequenceResult.Deserialize(deserializer);
        }
        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.header);
            serializer.Write(this.status);
            serializer.Write(this.result);
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
