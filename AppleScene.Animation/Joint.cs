using Microsoft.Xna.Framework;
using SharpGLTF.Schema2;

namespace AppleScene.Animation
{
    public class Joint
    {
        /// <summary>
        /// Node this joint is associated with.
        /// </summary>
        public Node Node { get; set; }

        /// <summary>
        /// Matrix that represents the position and orientation of the joint. Used to calculate the skin matrix for the
        /// vertices.
        /// </summary>
        public Matrix JointMatrix { get; set; }
        
        /// <summary>
        /// The inverse of the global transform when the bone is at it's bind (or default) position. 
        /// </summary>
        public Matrix InverseBindTransform { get; set; }

        /// <summary>
        /// Creates an instance of Joint.
        /// </summary>
        /// <param name="node">The node the joint is attached to.</param>
        /// <param name="globalTransformOfMesh">The global transform of the mesh. Used to calculate the JointMatrix
        /// property.</param>
        /// <param name="inverseBindTransform">The inverse bind transform of the joint.</param>
        public Joint(Node node, in Matrix globalTransformOfMesh, in Matrix inverseBindTransform) =>
            (Node, JointMatrix, InverseBindTransform) = (node,
                Matrix.Invert(globalTransformOfMesh) * node.WorldMatrix * inverseBindTransform, inverseBindTransform);
    }

    public record JointVector(Joint X, Joint Y, Joint Z, Joint W);
}