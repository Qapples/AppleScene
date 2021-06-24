using Microsoft.Xna.Framework;
using SharpGLTF.Schema2;

namespace AppleScene.Animation
{
    /// <summary>
    /// Represents a joint of a skin in a gLTF model.
    /// </summary>
    public class Joint
    {
        /// <summary>
        /// Node this joint is associated with.
        /// </summary>
        public Node Node { get; set; }

        /// <summary>
        /// Refers to a matrix in a jointMatrices array. This matrix that represents the position and orientation of the
        /// joint. Used to calculate the skin matrix for the vertices.
        /// </summary>
        public int JointMatrixIndex { get; set; }

        /// <summary>
        /// Returns a new matrix instance that is a copy of the matrix found in the provided jointMatrices at the
        /// JointMatrixIndex
        /// </summary>
        public Matrix JointMatrix => _jointMatrices[JointMatrixIndex];
        
        /// <summary>
        /// The inverse of the global transform when the bone is at it's bind (or default) position. 
        /// </summary>
        public Matrix InverseBindTransform { get; set; }


        private Matrix[] _jointMatrices;

        /// <summary>
        /// Creates an instance of Joint.
        /// </summary>
        /// <param name="node">The node the joint is attached to.</param>
        /// <param name="jointMatrixIndex"></param>
        /// <param name="globalTransformOfMesh">The global transform of the mesh. Used to calculate the JointMatrix
        /// property.</param>
        /// <param name="inverseBindTransform">The inverse bind transform of the joint.</param>
        /// <param name="jointMatrices"></param>
        public Joint(Node node, Matrix[] jointMatrices, int jointMatrixIndex, in Matrix globalTransformOfMesh,
            in Matrix inverseBindTransform)
        {
            (Node, _jointMatrices, JointMatrixIndex, jointMatrices[jointMatrixIndex], InverseBindTransform) = 
                (node, jointMatrices, jointMatrixIndex, Matrix.Invert(globalTransformOfMesh) * node.WorldMatrix * inverseBindTransform, inverseBindTransform);
        }
    }
}