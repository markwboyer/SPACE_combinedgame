using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Std;
using RosMessageTypes.Actionlib;

namespace RosMessageTypes.Msgs
{
    public class PickupActionGoal : ActionGoal<PickupGoal>
    {
        public const string k_RosMessageName = "msgs/PickupActionGoal";
        public override string RosMessageName => k_RosMessageName;


        public PickupActionGoal() : base()
        {
            this.goal = new PickupGoal();
        }

        public PickupActionGoal(HeaderMsg header, GoalIDMsg goal_id, PickupGoal goal) : base(header, goal_id)
        {
            this.goal = goal;
        }
        public static PickupActionGoal Deserialize(MessageDeserializer deserializer) => new PickupActionGoal(deserializer);

        PickupActionGoal(MessageDeserializer deserializer) : base(deserializer)
        {
            this.goal = PickupGoal.Deserialize(deserializer);
        }
        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.header);
            serializer.Write(this.goal_id);
            serializer.Write(this.goal);
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
