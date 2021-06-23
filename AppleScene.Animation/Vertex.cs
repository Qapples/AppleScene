using AppleScene.Animation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpGLTF.Schema2;

namespace AppleScene.Animation
{
    /// <summary>
    /// Represents a vertex of a mesh.
    /// </summary>
    public struct Vertex
    {
        /// <summary>
        /// Defines the data of the vertex such as position, normal, and texture cords.
        /// </summary>
        public VertexPositionNormalTexture VertexData;

        /// <summary>
        /// The Joints that have an affect on this vertex.
        /// </summary>
        public JointVector Joints;

        /// <summary>
        /// How much this vertex is affected by the joints.
        /// </summary>
        public Vector4 Weights;

        /// <summary>
        /// Creates an instance of Vertex.
        /// </summary>
        /// <param name="vertexData">Defines the data of the vertex such as position, normal, and texture cords.</param>
        /// <param name="joints">The Joints that have an affect on this vertex.</param>
        /// <param name="weights">How much this vertex is affected by the joints.</param>
        public Vertex(in VertexPositionNormalTexture vertexData, JointVector joints, Vector4 weights) =>
            (VertexData, Joints, Weights) = (vertexData, joints, weights);

        /// <summary>
        /// Creates an instance of Vertex (with a tuple of Joints instead of JointVector)
        /// </summary>
        /// <param name="vertexData">Defines the data of the vertex such as position, normal, and texture cords.</param>
        /// <param name="joints">The four Joints that have an affect on this vertex.</param>
        /// <param name="weights">How much this vertex is affected by the joints.</param>
        public Vertex(in VertexPositionNormalTexture vertexData, (Joint x, Joint y, Joint z, Joint w) joints,
            Vector4 weights) => (VertexData, Joints, Weights) =
            (vertexData, new JointVector(joints.x, joints.y, joints.z, joints.w), weights);
    }
}