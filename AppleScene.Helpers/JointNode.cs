using System.Numerics;
using SharpGLTF.Schema2;
using SharpGLTF.Transforms;

namespace AppleScene.Helpers
{
    internal class JointNode
    {
        public Node Node { get; set; }
        public JointNode? ParentJoint { get; set; }
        public TransformKeys TransformKeys { get; set; }
        public Matrix4x4 InverseBindMatrix { get; set; }

        public JointNode(Node node, JointNode? parentJoint, TransformKeys transformKeys,
            Matrix4x4 inverseBindMatrix) => 
            (Node, ParentJoint, TransformKeys, InverseBindMatrix) =
            (node, parentJoint, transformKeys, inverseBindMatrix);

        public Matrix4x4 GetWorldTransformMatrix(float time)
        {
            Matrix4x4 localMatrix = TransformKeys.GetTransformMatrix(time);

            return ParentJoint is null
                ? localMatrix
                : Matrix4x4Factory.LocalToWorld(ParentJoint.GetWorldTransformMatrix(time), in localMatrix);
        }
    }
}