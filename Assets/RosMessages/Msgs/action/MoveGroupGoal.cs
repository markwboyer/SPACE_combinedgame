//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace RosMessageTypes.Msgs
{
    [Serializable]
    public class MoveGroupGoal : Message
    {
        public const string k_RosMessageName = "msgs/MoveGroup";
        public override string RosMessageName => k_RosMessageName;

        //  Motion planning request to pass to planner
        public MotionPlanRequestMsg request;
        //  Planning options
        public PlanningOptionsMsg planning_options;

        public MoveGroupGoal()
        {
            this.request = new MotionPlanRequestMsg();
            this.planning_options = new PlanningOptionsMsg();
        }

        public MoveGroupGoal(MotionPlanRequestMsg request, PlanningOptionsMsg planning_options)
        {
            this.request = request;
            this.planning_options = planning_options;
        }

        public static MoveGroupGoal Deserialize(MessageDeserializer deserializer) => new MoveGroupGoal(deserializer);

        private MoveGroupGoal(MessageDeserializer deserializer)
        {
            this.request = MotionPlanRequestMsg.Deserialize(deserializer);
            this.planning_options = PlanningOptionsMsg.Deserialize(deserializer);
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.request);
            serializer.Write(this.planning_options);
        }

        public override string ToString()
        {
            return "MoveGroupGoal: " +
            "\nrequest: " + request.ToString() +
            "\nplanning_options: " + planning_options.ToString();
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#else
        [UnityEngine.RuntimeInitializeOnLoadMethod]
#endif
        public static void Register()
        {
            MessageRegistry.Register(k_RosMessageName, Deserialize, MessageSubtopic.Goal);
        }
    }
}
