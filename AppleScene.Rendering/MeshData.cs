#nullable enable
using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpGLTF.Schema2;
using PrimitiveType = Microsoft.Xna.Framework.Graphics.PrimitiveType;

namespace AppleScene.Rendering
{
    /// <summary>
    /// A class containing data about gLTF meshes. (vertex data, skinning data, etc.)
    /// </summary>
    public class MeshData
    {
        /// <summary>
        /// Vertices of each primitive list in the mesh.
        /// </summary>
        public VertexPositionNormalTexture[][] Vertices { get; set; }

        /// <summary>
        /// The GraphicsDevice instance used to create Effects and to draw the mesh.
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; set; }
        
        /// <summary>
        /// Affects the manor in which the mesh is drawn.
        /// </summary>
        public Effect Effect { get; set; }

        private VertexBuffer[] _vertexBuffers;

        /// <summary>
        /// Creates an instance of MeshData from a sharpGLTF mesh instance
        /// </summary>
        /// <param name="mesh">Mesh to get the data from.</param>
        /// <param name="graphicsDevice">GraphicsDevice instance used to create effects and draw the mesh.</param>
        /// <param name="effect">An effect object that impacts the manor in which the mesh is drawn. If not set to,
        /// then a basic default effect will be used instead. <br/>
        /// Default effect properties: <br/>
        /// Alpha = 1 <br/>
        /// VertexColorEnabled = true <br/>
        /// LightingEnabled = true <br/>
        /// </param>
        public MeshData(Mesh mesh, GraphicsDevice graphicsDevice, Effect? effect = null)
        {
            int count = mesh.Primitives.Count;
            
            Vertices = new VertexPositionNormalTexture[count][];
            GraphicsDevice = graphicsDevice;
            Effect = effect ?? new BasicEffect(graphicsDevice)
                {Alpha = 1, VertexColorEnabled = true, LightingEnabled = true};
            _vertexBuffers = new VertexBuffer[count];

            //Get the vertices.
            for (int i = 0; i < count; i++)
            {
                MeshPrimitive primitive = mesh.Primitives[i];

                var (positionVectors, normalVectors, texCords) = (
                    primitive.VertexAccessors["POSITION"].AsVector3Array(),
                    primitive.VertexAccessors["NORMAL"].AsVector3Array(),
                    primitive.VertexAccessors["TEXCOORD_0"].AsVector2Array());

                var (posCount, normalCount, texCount) = (positionVectors.Count, normalVectors.Count, texCords.Count);
                if (posCount != normalCount || posCount != texCount)
                {
                    Debug.WriteLine("One of the vertex lengths for position, normal, and texture cords are not" +
                                    $"the same. Ignoring primitive at index {i}. PositionVectors count: {posCount}. " +
                                    $"NormalVectors count: {normalCount}. TextureCords count: {texCount}");
                    continue;
                }

                Vertices[i] = new VertexPositionNormalTexture[posCount];
                for (int j = 0; j < posCount; j++)
                {
                    Vertices[i][j] = new VertexPositionNormalTexture(positionVectors[i], normalVectors[i], texCords[i]);
                }

                _vertexBuffers[i] = new VertexBuffer(graphicsDevice, typeof(VertexPositionNormalTexture), posCount,
                    BufferUsage.WriteOnly);
                _vertexBuffers[i].SetData(Vertices[i]);
            }
        }

        /// <summary>
        /// Draws the mesh based on the data.
        /// </summary>
        public void Draw(in Matrix worldMatrix, in Matrix viewMatrix, in Matrix projectionMatrix, RasterizerState rasterizerState, int primitiveIndex = 0)
        {
            RasterizerState prevState = GraphicsDevice.RasterizerState;
            GraphicsDevice.RasterizerState = rasterizerState;
            GraphicsDevice.SetVertexBuffer(_vertexBuffers[primitiveIndex]);

            foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, Vertices[primitiveIndex].Length);
            }

            GraphicsDevice.RasterizerState = prevState;
        }
    }
}