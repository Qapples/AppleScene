using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AppleScene.Animation
{
    /// <summary>
    /// A vertex type with position, normal, texture coordinates, joint indices, and weights. Used for models that
    /// have skins.
    /// </summary>
    [DataContract]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct JointVertexType : IVertexType
    {
        /// <summary>
        /// Position of the vertex.
        /// </summary>
        [DataMember] public Vector3 Position;

        /// <summary>
        /// Normal of the vertex.
        /// </summary>
        [DataMember] public Vector3 Normal;

        /// <summary>
        /// Refers to coordinates on a 2D texture. Used for imposing a texture upon a model.
        /// </summary>
        [DataMember] public Vector2 TextureCord;

        /// <summary>
        /// Each value (x, y, z, w) refers to a matrix in a provided joint matrix array that is used to change the
        /// position of the vertex based on the state of the four joints.
        /// </summary>
        [DataMember] public Vector4 Joints;

        /// <summary>
        /// Determines how much the four joints defined by the Joints field effect the position of this vertex.
        /// </summary>
        [DataMember] public Vector4 Weights;

        /// <summary>
        /// The VertexDeclaration that defines how each field is used when they are passed through a shader.
        /// </summary>
        public static readonly VertexDeclaration VertexDeclaration;

        VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;

        /// <summary>
        /// Creates a new JointVertexType instance.
        /// </summary>
        /// <param name="position">Position of the vertex.</param>
        /// <param name="normal">Normal of the vertex.</param>
        /// <param name="textureCord">Refers to coordinates on a 2D texture. Used for imposing a texture upon a model.
        /// </param>
        /// <param name="joints">Each value (x, y, z, w) refers to a matrix in a provided joint matrix array that is
        /// used to change the position of the vertex based on the state of the four joints.</param>
        /// <param name="weights">Determines how much the four joints defined by the Joints field effect the position of this vertex.</param>
        public JointVertexType(in Vector3 position, in Vector3 normal, in Vector2 textureCord, in Vector4 joints,
            in Vector4 weights) => (Position, Normal, TextureCord, Joints, Weights) =
            (position, normal, textureCord, joints, weights);

        static JointVertexType()
        {
            int offset = 0;

            //This is a bit of a trick we're doing here so that we can declare a new VertexDeclaration on the spot
            //without having to use List.Add(). The "+=" operator for int returns the value AFTER int has been
            //incremented (like i++). So, we immediately subtract the value we added to the returned value to
            //compensate.
            VertexDeclaration = new VertexDeclaration(
                new VertexElement((offset += 12) - 12, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                new VertexElement((offset += 12) - 12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
                new VertexElement((offset += 8) - 8, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),

                //BlendIndices in this case refer to the joints.
                new VertexElement((offset += 8) - 8, VertexElementFormat.Short4, VertexElementUsage.BlendIndices, 0),
                new VertexElement((offset += 16) + 16, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 0));
        }
    }
}