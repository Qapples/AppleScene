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
using SharpGLTF.Runtime;
using SharpGLTF.Schema2;
using SharpGLTF.Transforms;

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
        /// Using an <see cref="IMeshPrimitiveDecoder"/>, gets the data from a primitive and contacts it into a
        /// <see cref="Span{T}"/> where T is byte within an XNA context.
        /// </summary>
        /// <param name="primitive">The <see cref="IMeshPrimitiveDecoder"/> instance to get the data from.</param>
        /// <param name="decl"><see cref="VertexDeclaration"/> instances that determines the format of the data
        /// in the primitive. Using the <see cref="GetDeclaration"/> extension method on a <see cref="MeshPrimitive"/>
        /// instance is one way to obtain the instance needed for this parameter.</param>
        /// <returns>A <see cref="Span{T}"/> of bytes that represents the contacted data of the primitive for use in an
        /// XNA/MonoGame context.</returns>
        public static Span<byte> GetXnaByteData(this IMeshPrimitiveDecoder primitive, VertexDeclaration decl)
        {
            Span<byte> outSpan = new byte[primitive.VertexCount * decl.VertexStride];
            VertexElement[] vertexElements = decl.GetVertexElements();

            for (int i = 0; i < primitive.VertexCount; i++)
            {
                SparseWeight8 skinWeights = primitive.GetSkinWeights(i);
                int offset = i * decl.VertexStride;

                foreach (var elm in vertexElements)
                {
                    int offsetParam = offset + elm.Offset;

                    switch (elm.VertexElementUsage)
                    {
                        case VertexElementUsage.Position:
                            Encode(ref outSpan, offsetParam, primitive.GetPosition(i), elm.VertexElementFormat);
                            break;
                        case VertexElementUsage.Normal:
                            Encode(ref outSpan, offsetParam, primitive.GetNormal(i), elm.VertexElementFormat);
                            break;
                        case VertexElementUsage.Tangent:
                            Encode(ref outSpan, offsetParam, primitive.GetTangent(i), elm.VertexElementFormat);
                            break;
                        case VertexElementUsage.Color:
                            Encode(ref outSpan, offsetParam, primitive.GetColor(i, elm.UsageIndex),
                                elm.VertexElementFormat);
                            break;
                        case VertexElementUsage.TextureCoordinate:
                            Encode(ref outSpan, offsetParam, primitive.GetTextureCoord(i, elm.UsageIndex));
                            break;
                        case VertexElementUsage.BlendIndices:
                            Encode(ref outSpan, offsetParam, new Vector4(skinWeights.Index0, skinWeights.Index1,
                                skinWeights.Index2, skinWeights.Index3), elm.VertexElementFormat);
                            break;
                        case VertexElementUsage.BlendWeight:
                            Encode(ref outSpan, offsetParam, new Vector4(skinWeights.Weight0, skinWeights.Weight1,
                                skinWeights.Weight2, skinWeights.Weight4), elm.VertexElementFormat);
                            break;
                    }
                }
            }

            return outSpan;
        }

        private static void Encode(ref Span<byte> span, int offset, in Vector2 value)
        {
            Vector2 temp = value;
            MemoryMarshal.Write(span[offset..], ref temp);
        }

        private static bool Encode(ref Span<byte> span, int offset, in Vector3 value, in VertexElementFormat format)
        {
            switch (format)
            {
                case VertexElementFormat.Vector3:
                    Vector3 temp = value;
                    MemoryMarshal.Write(span[offset..], ref temp);
                    return true;
                case VertexElementFormat.Color:
                    Color color = new Color(value);
                    MemoryMarshal.Write(span[offset..], ref color);
                    return true;
                default:
                    Debug.WriteLine($"Unable to convert Vector3 from the format {format}.");
                    return false;
            }
        }

        private static bool Encode(ref Span<byte> span, int offset, in Vector4 value, in VertexElementFormat format)
        {
            switch (format)
            {
                case VertexElementFormat.Vector4:
                    Vector4 temp = value;
                    MemoryMarshal.Write(span[offset..], ref temp);
                    return true;
                case VertexElementFormat.Byte4:
                    Byte4 byte4 = new Byte4(value);
                    MemoryMarshal.Write(span[offset..], ref byte4);
                    return true;
                case VertexElementFormat.Color:
                    Color color = new Color(value);
                    MemoryMarshal.Write(span[offset..], ref color);
                    return true;
                case VertexElementFormat.Short4:
                    Short4 short4 = new Short4(value);
                    MemoryMarshal.Write(span[offset..], ref short4);
                    return true;
                case VertexElementFormat.NormalizedShort4:
                    NormalizedByte4 normalizedByte4 = new NormalizedByte4(value);
                    MemoryMarshal.Write(span[offset..], ref normalizedByte4);
                    return true;
                case VertexElementFormat.HalfVector4:
                    HalfVector4 halfVector4 = new HalfVector4(value);
                    MemoryMarshal.Write(span[offset..], ref halfVector4);
                    return true;
                default:
                    Debug.WriteLine($"Unable to convert Vector4 from the fromat {format}.");
                    return false;
            }
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