using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using SharpGLTF.Runtime;
using SharpGLTF.Schema2;
using SharpGLTF.Transforms;

namespace AppleScene.Helpers
{
    /// <summary>
    /// Provides extension methods that provide extra functionality to <see cref="IMeshPrimitiveDecoder"/> instances.
    /// </summary>
    public static class MeshPrimitiveDecoderExtensions
    {
        /// <summary>
        /// Using an <see cref="IMeshPrimitiveDecoder"/>, gets the data from a primitive and contacts it into a
        /// <see cref="Span{T}"/> where T is byte within an XNA context.
        /// </summary>
        /// <param name="primitive">The <see cref="IMeshPrimitiveDecoder"/> instance to get the data from.</param>
        /// <param name="decl"><see cref="VertexDeclaration"/> instances that determines the format of the data
        /// in the primitive. Using the <see cref="MeshPrimitiveExtensions.GetDeclaration"/> extension method on a <see cref="MeshPrimitive"/>
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
    }
}