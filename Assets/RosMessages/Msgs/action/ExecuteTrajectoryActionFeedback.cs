using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Std;
using RosMessageTypes.Actionlib;

namespace RosMessageTypes.Msgs
{
    public class ExecuteTrajectoryActionFeedback : ActionFeedback<ExecuteTrajectoryFeedback>
    {
        public const string k_RosMessageName = "msgs/ExecuteTrajectoryActionFeedback";
        public override string RosMessageName => k_RosMessageName;


        public ExecuteTrajectoryActionFeedback() : base()
        {
            this.feedback = new ExecuteTrajectoryFeedback();
        }

        public ExecuteTrajectoryActionFeedback(HeaderMsg header, GoalStatusMsg status, ExecuteTrajectoryFeedback feedback) : base(header, status)
        {
            this.feedback = feedback;
        }
        public static ExecuteTrajectoryActionFeedback Deserialize(MessageDeserializer deserializer) => new ExecuteTrajectoryActionFeedback(deserializer);

        ExecuteTrajectoryActionFeedback(MessageDeserializer deserializer) : base(deserializer)
        {
            this.feedback = ExecuteTrajectoryFeedback.Deserialize(deserializer);
        }
        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.header);
            serializer.Write(this.status);
            serializer.Write(this.feedback);
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
