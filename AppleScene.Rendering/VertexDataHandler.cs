using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Graphics;

namespace AppleScene.Rendering
{
    /// <summary>
    /// Class that is used for handling vertex data in bytes and to create VertexBuffers.
    /// Based off of the "VertexBufferContent" implementation in MonoScene
    /// (https://github.com/vpenades/MonoScene/blob/master/src/MonoScene.Runtime.Content/Meshes/VertexBufferContent.cs)
    /// </summary>
    public class VertexDataHandler
    {
        private byte[] _vertexData;
        
        private int _vertexCount;

        private VertexElement[] _vertexElements;
        
        /// <summary>
        /// How large (in bytes) a vertex is. Used to advance to the next vertex.
        /// </summary>
        private int _vertexStride;

        private VertexDeclaration VertexDeclaration => new(_vertexStride, _vertexElements);

        /// <summary>
        /// Creates an instance of VertexDataHandler.
        /// </summary>
        /// <param name="vertexData">The vertex data in the from of an array.</param>
        /// <param name="declaration">The VertexDeclaration instance that defines how the vertices should be interpreted
        /// </param>
        public VertexDataHandler(byte[] vertexData, VertexDeclaration declaration)
        {
            (_vertexData, _vertexCount) = (vertexData, vertexData.Length / declaration.VertexStride);
            (_vertexElements, _vertexStride) = (declaration.GetVertexElements(), declaration.VertexStride);
        }

        /// <summary>
        /// Creates an instance of VertexDataHandler (with a span instead of an array)
        /// </summary>
        /// <param name="vertexData">The vertex data in the form of a ReadOnlySpan.</param>
        /// <param name="declaration">The VertexDeclaration instance that defines how the vertices should be interpreted
        /// </param>
        public VertexDataHandler(in ReadOnlySpan<byte> vertexData, VertexDeclaration declaration) :
            this(vertexData.ToArray(), declaration)
        {
        }

        /// <summary>
        /// Adds additional vertices to the stored buffer.
        /// </summary>
        /// <param name="newVertexData">The vertex data to add.</param>
        /// <param name="newDeclaration">The declaration of the additional vertices.</param>
        /// <exception cref="ArgumentException">Thrown if this method fails due to invalid agurments.</exception>
        /// <returns>A tuple representing the total bytes of the buffer and the total amount of vertices after the new
        /// vertex data as been added to the buffer.</returns>
        public (int totalBytes, int totalVertexCount) AddVertices(in ReadOnlySpan<byte> newVertexData,
            VertexDeclaration newDeclaration)
        {
            //how many new vertices will be added
            int newVertexCount = newVertexData.Length / newDeclaration.VertexStride;

            if (newVertexCount < 1) return (_vertexData.Length, 0);

            if (_vertexCount < 1)
            {
                _vertexElements = newDeclaration.GetVertexElements();
                _vertexStride = newDeclaration.VertexStride;
            }
            else
            {
                VertexDeclaration tempDecl = VertexDeclaration;

                //operator override
                if (tempDecl != newDeclaration)
                {
                    throw new ArgumentException("Current declaration is not the same as new declaration. " +
                                                $"Current declaration: {tempDecl}\nNew declaration: {newDeclaration}");
                }
            }

            //resize the vertexData array and attempt to copy over the new vertex data.
            int totalBytes = newVertexCount * newDeclaration.VertexStride;
            Array.Resize(ref _vertexData, totalBytes);

            if (!newVertexData.TryCopyTo(_vertexData.AsSpan()[_vertexCount..]))
            {
                throw new ArgumentException(
                    $"Unable to transfer new vertex data onto the established data. _vertexData " +
                    $"length: {_vertexData.Length}. _vertexCount: {_vertexCount}. newVertexDataLength: " +
                    $"{newVertexData.Length}.");
            }

            _vertexCount += newVertexCount;

            return (totalBytes, _vertexCount);
        }

        /// <summary>
        /// Adds additional vertices to the stored buffer.
        /// </summary>
        /// <param name="newVertexData">The vertex data to add.</param>
        /// <param name="newDeclaration">The declaration of the additional vertices.</param>
        /// <param name="result">If successful, this value will be a tuple representing the total bytes of the buffer
        /// and the total amount of vertices after the new vertex data as been added to the buffer. If unsuccessful,
        /// this value will be <c>(-1, 1)</c></param>
        /// <returns>If the vertices were successfully appended, then <c>true</c> is returned. Otherwise, <c>false</c>.
        /// </returns>
        public bool TryAddVertices(in ReadOnlySpan<byte> newVertexData, VertexDeclaration newDeclaration,
            out (int totalBytes, int totalVertexCount) result)
        {
            try
            {
                result = AddVertices(in newVertexData, newDeclaration);

                return true;
            }
            catch (ArgumentException e)
            {
                Debug.WriteLine($"TryAddVertices fail. Exception: {e}");
                result = (-1, -1);
                
                return false;
            }
        }

        /// <summary>
        /// Generates a vertex buffer based on the vertex data stored within this object.
        /// </summary>
        /// <param name="graphicsDevice">GraphicsDevice instance used to create a vertex buffer.</param>
        /// <param name="usage">Hints at how the buffer will be used. <c>BufferUsage.None</c> by default.</param>
        /// <returns>A new vertex buffer created from the vertex data.</returns>
        public VertexBuffer GenerateVertexBuffer(GraphicsDevice graphicsDevice, BufferUsage usage = BufferUsage.None)
        {
            VertexBuffer outBuffer = new(graphicsDevice, VertexDeclaration, _vertexCount, usage);
            outBuffer.SetData(_vertexData, 0, _vertexCount * _vertexStride);

            return outBuffer;
        }
    }
}