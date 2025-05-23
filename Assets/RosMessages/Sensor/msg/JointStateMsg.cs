//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Std;

namespace RosMessageTypes.Sensor
{
    [Serializable]
    public class JointStateMsg : Message
    {
        public const string k_RosMessageName = "sensor_msgs/JointState";
        public override string RosMessageName => k_RosMessageName;

        public HeaderMsg header;
        public string[] name;
        public double[] position;
        public double[] velocity;
        public double[] effort;

        public JointStateMsg()
        {
            this.header = new HeaderMsg();
            this.name = new string[0];
            this.position = new double[0];
            this.velocity = new double[0];
            this.effort = new double[0];
        }

        public JointStateMsg(HeaderMsg header, string[] name, double[] position, double[] velocity, double[] effort)
        {
            this.header = header;
            this.name = name;
            this.position = position;
            this.velocity = velocity;
            this.effort = effort;
        }

        public static JointStateMsg Deserialize(MessageDeserializer deserializer) => new JointStateMsg(deserializer);

        private JointStateMsg(MessageDeserializer deserializer)
        {
            this.header = HeaderMsg.Deserialize(deserializer);
            deserializer.Read(out this.name, deserializer.ReadLength());
            deserializer.Read(out this.position, sizeof(double), deserializer.ReadLength());
            deserializer.Read(out this.velocity, sizeof(double), deserializer.ReadLength());
            deserializer.Read(out this.effort, sizeof(double), deserializer.ReadLength());
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.header);
            serializer.WriteLength(this.name);
            serializer.Write(this.name);
            serializer.WriteLength(this.position);
            serializer.Write(this.position);
            serializer.WriteLength(this.velocity);
            serializer.Write(this.velocity);
            serializer.WriteLength(this.effort);
            serializer.Write(this.effort);
        }

        public override string ToString()
        {
            return "JointStateMsg: " +
            "\nheader: " + header.ToString() +
            "\nname: " + System.String.Join(", ", name.ToList()) +
            "\nposition: " + System.String.Join(", ", position.ToList()) +
            "\nvelocity: " + System.String.Join(", ", velocity.ToList()) +
            "\neffort: " + System.String.Join(", ", effort.ToList());
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
