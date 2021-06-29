using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using SharpGLTF.Runtime;
using SharpGLTF.Schema2;

namespace AppleScene.Helpers
{
    /// <summary>
    /// Provides extension methods that provide extra functionality to <see cref="MeshPrimitive"/> instances.
    /// </summary>
    public static class MeshPrimitiveExtensions
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
        public static VertexDeclaration GetDeclaration(this MeshPrimitive primitive)
        {
            List<VertexElement> elements = new();
            int offset = 0;

            foreach (var accessor in _singleAccessors)
            {
                if (primitive.VertexAccessors.TryGetValue(accessor.Name, out _))
                {
                    elements.Add(new VertexElement(offset, accessor.Format, accessor.Usage, 0));
                    offset += accessor.Size;
                }
            }

            foreach (var accessor in _multiAccessors)
            {
                for (int i = 0; primitive.VertexAccessors.TryGetValue($"{accessor.Name}_{i}", out _); i++)
                {
                    elements.Add(new VertexElement(offset, accessor.Format, accessor.Usage, i));
                    offset += accessor.Size;
                }
            }

            return new VertexDeclaration(offset, elements.ToArray());
        }

        /// <summary>
        /// Gets the <see cref="IMeshPrimitiveDecoder"/> from a <see cref="MeshPrimitive"/>.
        /// </summary>
        /// <remarks>This extension method calls the Decode method from <see cref="SharpGLTF.Runtime.MeshDecoder"/>,
        /// which is potentially an expensive operation. It is recommended to use this method as little as possible.
        /// </remarks>
        /// <param name="primitive">The <see cref="primitive"/> instance to decode.</param>
        /// <returns>A <see cref="IMeshPrimitiveDecoder"/> instance that can be used to get data from the primitive
        /// </returns>
        public static IMeshPrimitiveDecoder GetDecoder(this MeshPrimitive primitive) =>
            primitive.LogicalParent.Decode().Primitives[primitive.LogicalIndex];

        /// <summary>
        /// Creates an <see cref="IndexBuffer"/> for a <see cref="MeshPrimitive"/>.
        /// </summary>
        /// <remarks>Creating an <see cref="IndexBuffer"/> is memory expensive. Use this method sparingly.</remarks>
        /// <param name="primitive">The <see cref="MeshPrimitive"/> instance to make an IndexBuffer from.</param>
        /// <param name="graphicsDevice">Used to create the <see cref="IndexBuffer"/>.</param>
        /// <param name="usage">The <see cref="BufferUsage"/> parameter value used when creating the
        /// <see cref="IndexBuffer"/>. By default, it is <see cref="BufferUsage.None"/>.</param>
        /// <returns>The <see cref="IndexBuffer"/> with the the indices from the primitive.</returns>
        public static IndexBuffer GetIndexBuffer(this MeshPrimitive primitive, GraphicsDevice graphicsDevice,
            BufferUsage usage = BufferUsage.None)
        {
            uint[] indexArray = new uint[primitive.IndexAccessor.Count];
            primitive.IndexAccessor.AsIndicesArray().CopyTo(indexArray, 0);

            IndexBuffer outBuffer = new(graphicsDevice, IndexElementSize.ThirtyTwoBits, indexArray.Length, usage);
            outBuffer.SetData(indexArray);

            return outBuffer;
        }
        
        /// <summary>
        /// Provides information on VertexAccessors which are used to gather data regarding vertices.
        /// </summary>
        /// <param name="Name">The name of the accessor.</param>
        /// <param name="Size">The size of data format. (ex: Vector3 would have an offset of 12)</param>
        /// <param name="Format">The type of data the accessor is referring to.</param>
        /// <param name="Usage">What the data will be defining about a vertex.</param>
        internal sealed record AccessorDefinition(string Name, int Size, VertexElementFormat Format,
            VertexElementUsage Usage);
    }
}