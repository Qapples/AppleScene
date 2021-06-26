#nullable enable
using System;
using System.Diagnostics;
using AppleScene.Animation;
using Microsoft.Xna.Framework.Graphics;
using SharpGLTF.Schema2;

namespace AppleScene.Helpers
{
    /// <summary>
    /// Provides methods in assisting with dealing with SharpGLTF Mesh instances in a Monogame context.
    /// </summary>
    public static class MeshHelper
    {
        /// <summary>
        /// Gets the vertex type that a primitive uses for it's vertices.
        /// </summary>
        /// <param name="primitive">The primitive to get the vertex type from.</param>
        /// <returns>If successful, the vertex type of the primitive is used. If unsuccessful and the type cannot be
        /// determined, then null is returned.</returns>
        public static Type? GetVertexType(this MeshPrimitive primitive)
        {
            //This mask represents what accessors the primitive has to determine what VertexType to use.
            byte mask = (byte) (IsAccessor(primitive, "POSITION") +
                                (IsAccessor(primitive, "NORMAL") << 1) +
                                (IsAccessor(primitive, "TEXCOORD_0") << 2) +
                                (IsAccessor(primitive, "JOINTS_0") << 3) +
                                (IsAccessor(primitive, "WEIGHTS_0") << 4));

            switch (mask)
            {
                case 0b0000_0101: //has both Position and TexCoord_0
                    return typeof(VertexPositionTexture);
                case 0b0000_0111: //has Position, Normal, and TexCoord_0
                    return typeof(VertexPositionNormalTexture);
                case 0b0001_111: //has Position, Normal, TexCoord_0, Joints_0, and Weights_0
                    return typeof(JointVertexType);
                default:
                    Debug.WriteLine($"GetVertexType: Cannot find valid vertex type for primitive: {primitive}. " +
                                    $"Byte mask: {mask}. Returning null.");
                    return null;
            }
        }

        private static byte IsAccessor(MeshPrimitive primitive, string name) =>
            primitive.VertexAccessors[name] is null ? (byte) 0 : (byte) 1;
    }
}