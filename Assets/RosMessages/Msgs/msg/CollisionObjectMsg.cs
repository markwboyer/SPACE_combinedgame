//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace RosMessageTypes.Msgs
{
    [Serializable]
    public class CollisionObjectMsg : Message
    {
        public const string k_RosMessageName = "msgs/CollisionObject";
        public override string RosMessageName => k_RosMessageName;

        //  a header, used for interpreting the poses
        public Std.HeaderMsg header;
        //  DISCLAIMER: This field is not in use yet and all other poses
        //  are still interpreted in the header frame.
        //  https://github.com/ros-planning/moveit/pull/2037
        //  implements the actual logic for this field.
        //  ---
        //  The object's pose relative to the header frame.
        //  The shapes and subframe poses are defined relative to this pose.
        public Geometry.PoseMsg pose;
        //  The id of the object (name used in MoveIt)
        public string id;
        //  The object type in a database of known objects
        public ObjectRecognition.ObjectTypeMsg type;
        //  The collision geometries associated with the object.
        //  Their poses are with respect to the object's pose
        //  Solid geometric primitives
        public Shape.SolidPrimitiveMsg[] primitives;
        public Geometry.PoseMsg[] primitive_poses;
        //  Meshes
        public Shape.MeshMsg[] meshes;
        public Geometry.PoseMsg[] mesh_poses;
        //  Bounding planes (equation is specified, but the plane can be oriented using an additional pose)
        public Shape.PlaneMsg[] planes;
        public Geometry.PoseMsg[] plane_poses;
        //  Named subframes on the object. Use these to define points of interest on the object that you want
        //  to plan with (e.g. "tip", "spout", "handle"). The id of the object will be prepended to the subframe.
        //  If an object with the id "screwdriver" and a subframe "tip" is in the scene, you can use the frame
        //  "screwdriver/tip" for planning.
        //  The length of the subframe_names and subframe_poses has to be identical.
        public string[] subframe_names;
        public Geometry.PoseMsg[] subframe_poses;
        //  Adds the object to the planning scene. If the object previously existed, it is replaced.
        public const sbyte ADD = 0;
        //  Removes the object from the environment entirely (everything that matches the specified id)
        public const sbyte REMOVE = 1;
        //  Append to an object that already exists in the planning scene. If the object does not exist, it is added.
        public const sbyte APPEND = 2;
        //  If an object already exists in the scene, new poses can be sent (the geometry arrays must be left empty)
        //  if solely moving the object is desired
        public const sbyte MOVE = 3;
        //  Operation to be performed
        public sbyte operation;

        public CollisionObjectMsg()
        {
            this.header = new Std.HeaderMsg();
            this.pose = new Geometry.PoseMsg();
            this.id = "";
            this.type = new ObjectRecognition.ObjectTypeMsg();
            this.primitives = new Shape.SolidPrimitiveMsg[0];
            this.primitive_poses = new Geometry.PoseMsg[0];
            this.meshes = new Shape.MeshMsg[0];
            this.mesh_poses = new Geometry.PoseMsg[0];
            this.planes = new Shape.PlaneMsg[0];
            this.plane_poses = new Geometry.PoseMsg[0];
            this.subframe_names = new string[0];
            this.subframe_poses = new Geometry.PoseMsg[0];
            this.operation = 0;
        }

        public CollisionObjectMsg(Std.HeaderMsg header, Geometry.PoseMsg pose, string id, ObjectRecognition.ObjectTypeMsg type, Shape.SolidPrimitiveMsg[] primitives, Geometry.PoseMsg[] primitive_poses, Shape.MeshMsg[] meshes, Geometry.PoseMsg[] mesh_poses, Shape.PlaneMsg[] planes, Geometry.PoseMsg[] plane_poses, string[] subframe_names, Geometry.PoseMsg[] subframe_poses, sbyte operation)
        {
            this.header = header;
            this.pose = pose;
            this.id = id;
            this.type = type;
            this.primitives = primitives;
            this.primitive_poses = primitive_poses;
            this.meshes = meshes;
            this.mesh_poses = mesh_poses;
            this.planes = planes;
            this.plane_poses = plane_poses;
            this.subframe_names = subframe_names;
            this.subframe_poses = subframe_poses;
            this.operation = operation;
        }

        public static CollisionObjectMsg Deserialize(MessageDeserializer deserializer) => new CollisionObjectMsg(deserializer);

        private CollisionObjectMsg(MessageDeserializer deserializer)
        {
            this.header = Std.HeaderMsg.Deserialize(deserializer);
            this.pose = Geometry.PoseMsg.Deserialize(deserializer);
            deserializer.Read(out this.id);
            this.type = ObjectRecognition.ObjectTypeMsg.Deserialize(deserializer);
            deserializer.Read(out this.primitives, Shape.SolidPrimitiveMsg.Deserialize, deserializer.ReadLength());
            deserializer.Read(out this.primitive_poses, Geometry.PoseMsg.Deserialize, deserializer.ReadLength());
            deserializer.Read(out this.meshes, Shape.MeshMsg.Deserialize, deserializer.ReadLength());
            deserializer.Read(out this.mesh_poses, Geometry.PoseMsg.Deserialize, deserializer.ReadLength());
            deserializer.Read(out this.planes, Shape.PlaneMsg.Deserialize, deserializer.ReadLength());
            deserializer.Read(out this.plane_poses, Geometry.PoseMsg.Deserialize, deserializer.ReadLength());
            deserializer.Read(out this.subframe_names, deserializer.ReadLength());
            deserializer.Read(out this.subframe_poses, Geometry.PoseMsg.Deserialize, deserializer.ReadLength());
            deserializer.Read(out this.operation);
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.header);
            serializer.Write(this.pose);
            serializer.Write(this.id);
            serializer.Write(this.type);
            serializer.WriteLength(this.primitives);
            serializer.Write(this.primitives);
            serializer.WriteLength(this.primitive_poses);
            serializer.Write(this.primitive_poses);
            serializer.WriteLength(this.meshes);
            serializer.Write(this.meshes);
            serializer.WriteLength(this.mesh_poses);
            serializer.Write(this.mesh_poses);
            serializer.WriteLength(this.planes);
            serializer.Write(this.planes);
            serializer.WriteLength(this.plane_poses);
            serializer.Write(this.plane_poses);
            serializer.WriteLength(this.subframe_names);
            serializer.Write(this.subframe_names);
            serializer.WriteLength(this.subframe_poses);
            serializer.Write(this.subframe_poses);
            serializer.Write(this.operation);
        }

        public override string ToString()
        {
            return "CollisionObjectMsg: " +
            "\nheader: " + header.ToString() +
            "\npose: " + pose.ToString() +
            "\nid: " + id.ToString() +
            "\ntype: " + type.ToString() +
            "\nprimitives: " + System.String.Join(", ", primitives.ToList()) +
            "\nprimitive_poses: " + System.String.Join(", ", primitive_poses.ToList()) +
            "\nmeshes: " + System.String.Join(", ", meshes.ToList()) +
            "\nmesh_poses: " + System.String.Join(", ", mesh_poses.ToList()) +
            "\nplanes: " + System.String.Join(", ", planes.ToList()) +
            "\nplane_poses: " + System.String.Join(", ", plane_poses.ToList()) +
            "\nsubframe_names: " + System.String.Join(", ", subframe_names.ToList()) +
            "\nsubframe_poses: " + System.String.Join(", ", subframe_poses.ToList()) +
            "\noperation: " + operation.ToString();
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
