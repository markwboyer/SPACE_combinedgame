using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Std;
using RosMessageTypes.Actionlib;

namespace RosMessageTypes.Msgs
{
    public class MoveGroupActionResult : ActionResult<MoveGroupResult>
    {
        public const string k_RosMessageName = "msgs/MoveGroupActionResult";
        public override string RosMessageName => k_RosMessageName;


        public MoveGroupActionResult() : base()
        {
            this.result = new MoveGroupResult();
        }

        public MoveGroupActionResult(HeaderMsg header, GoalStatusMsg status, MoveGroupResult result) : base(header, status)
        {
            this.result = result;
        }
        public static MoveGroupActionResult Deserialize(MessageDeserializer deserializer) => new MoveGroupActionResult(deserializer);

        MoveGroupActionResult(MessageDeserializer deserializer) : base(deserializer)
        {
            this.result = MoveGroupResult.Deserialize(deserializer);
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
