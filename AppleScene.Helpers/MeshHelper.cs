#nullable enable
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using AppleScene.Animation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using SharpGLTF.Schema2;

namespace AppleScene.Helpers
{
    /// <summary>
    /// Provides methods in assisting with dealing with SharpGLTF Mesh instances in a Monogame context.
    /// </summary>
    public static class MeshHelper
    {
        /// <summary>
        /// Accessors that usually only exist once in a <see cref="MeshPrimitive"/>
        /// </summary>
        private static readonly AccessorDefinition[] _singleAccessors =
        {
            new("POSITION", 12, VertexElementFormat.Vector3, VertexElementUsage.Position),
            new("NORMAL", 12, VertexElementFormat.Vector3, VertexElementUsage.Normal)
        };

        /// <summary>
        /// Accessors that can exist more than once in a <see cref="MeshPrimitive"/>. Add "_x" (where x is any positive
        /// integer) to the end of the name to indicate which accessor to obtain.
        /// </summary>
        private static readonly AccessorDefinition[] _multiAccessors =
        {
            new("TEXCOORD", 8, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate),
            new("JOINTS", 8, VertexElementFormat.Short4, VertexElementUsage.BlendIndices),
            new("WEIGHTS", 16, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight)
        };
        
        /// <summary>
        /// Gets the VertexDeclaration of the vertices of a specified <see cref="MeshPrimitive"/>.
        /// </summary>
        /// <param name="primitive">The <see cref="MeshPrimitive"/> to get the VertexDeclaration of.</param>
        /// <returns>The VertexDeclaration which states what data each vertex has and what they are used for.</returns>
        public static VertexDeclaration GetDeclarationFromPrimitive(this MeshPrimitive primitive)
        {
            List<VertexElement> elements = new();
            int offset = 0;
            
            foreach (var accessor in _singleAccessors)
            {
                if (primitive.VertexAccessors.TryGetValue(accessor.Name, out _))
                {
                    elements.Add(new VertexElement((offset += accessor.Offset), accessor.Format, accessor.Usage, 0));
                }
            }

            foreach (var accessor in _multiAccessors)
            {
                for (int i = 0; primitive.VertexAccessors.TryGetValue($"{accessor.Name}_{i}", out _); i++)
                {
                    elements.Add(new VertexElement((offset += accessor.Offset), accessor.Format, accessor.Usage, i));
                }
            }
            
            return new VertexDeclaration(offset, elements.ToArray());
        }
    }

    /// <summary>
    /// Provides information on VertexAccessors which are used to gather data regarding vertices.
    /// </summary>
    /// <param name="Name">The name of the accessor.</param>
    /// <param name="Offset">The size of data format.</param>
    /// <param name="Format">The type of data the accessor is referring to.</param>
    /// <param name="Usage">What the data will be defining about a vertex.</param>
    internal sealed record AccessorDefinition(string Name, int Offset, VertexElementFormat Format, VertexElementUsage Usage);
}