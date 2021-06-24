#nullable enable
using System.Diagnostics;
using System.Linq;
using AppleScene.Animation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Design;
using Microsoft.Xna.Framework.Graphics;
using SharpGLTF.Schema2;
using SharpGLTF.Transforms;
using PrimitiveType = Microsoft.Xna.Framework.Graphics.PrimitiveType;

namespace AppleScene.Rendering
{
    /// <summary>
    /// A class containing data about gLTF meshes. (vertex data, skinning data, etc.)
    /// </summary>
    public class MeshData
    {
        //We're separating the vertices from the joint and weights so that we don't have to create an extra copy of the
        //vertices when making the vertex buffer

        /// <summary>
        /// Vertices of each primitive list in the mesh.
        /// </summary>
        public VertexPositionNormalTexture[][] Vertices { get; set; }
        
        /// <summary>
        /// The x, y, z, and w values are indexes for the Joints property. A vertex at Vertices[i][j] is affected or
        /// manipulated by four joints: Joints[i][JointsForVertices[i][j].X], Joints[i][JointsForVertices[i][j].Y],
        /// Joints[i][JointsForVertices[i][j].Z], and Joints[i][JointsForVertices[i][j].W] 
        /// </summary>
        public Vector4[]?[] JointsForVertices { get; set; }
        
        public Joint[]?[] Joints { get; set; }
        
        /// <summary>
        /// How much of an effect the joint at Joints[i][j] have on the vertex at Vertices[i][j]. If an array is null,
        /// then the primitive associated with that array does not have weights or joints.
        /// </summary>
        public Vector4[]?[] Weights { get; set; }
        
        /// <summary>
        /// Represents the transform of each of the joints. If an array is
        /// null, then the primitive associated with that array does not have joints.
        /// </summary>
        public Matrix[]?[] JointMatrices { get; set; }

        /// <summary>
        /// The GraphicsDevice instance used to create Effects (if need be) and to draw the mesh.
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; set; }
        
        /// <summary>
        /// Affects the manor in which the mesh is drawn.
        /// </summary>
        public Effect Effect { get; set; }

        private readonly VertexBuffer[] _vertexBuffers;
        private readonly IndexBuffer[] _indexBuffers;

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

            GraphicsDevice = graphicsDevice;
            Effect = effect ?? new BasicEffect(graphicsDevice)
                {Alpha = 1, VertexColorEnabled = true, LightingEnabled = true};
            (_vertexBuffers, _indexBuffers) = (new VertexBuffer[count], new IndexBuffer[count]);
            (Vertices, Joints, JointsForVertices, Weights, JointMatrices) = (new VertexPositionNormalTexture[count][],
                new Joint[count][], new Vector4[count][], new Vector4[count][], new Matrix[count][]);

            ModelRoot modelRoot = mesh.LogicalParent;

            //Get the vertices.
            for (int i = 0; i < count; i++)
            {
                MeshPrimitive primitive = mesh.Primitives[i];

                //TODO: There may be more than one set of joints, weights, or texture cords. Account for them in the future perhaps?
                var (positionVectors, normalVectors, texCords, jointVectors, weightVectors) = (
                    primitive.VertexAccessors["POSITION"].AsVector3Array(),
                    primitive.VertexAccessors["NORMAL"].AsVector3Array(),
                    primitive.VertexAccessors["TEXCOORD_0"].AsVector2Array(),
                    primitive.VertexAccessors["JOINTS_0"].AsVector4Array(),
                    primitive.VertexAccessors["WEIGHTS_0"].AsVector4Array());

                if (positionVectors is null || normalVectors is null || texCords is null)
                {
                    Debug.WriteLine($"The following necessary accessors were not found:\n" +
                                    $"Position accessor: {(positionVectors is null ? "Not Found" : "Found")}\n" +
                                    $"Normal accessor: {(normalVectors is null ? "Not Found" : "Found")}\n" +
                                    $"Texture cords accessor: {(texCords is null ? "Not Found" : "Found")}\n");
                    continue;
                }

                var (posCount, normalCount, texCount, jointCount, weightCount) = (positionVectors.Count,
                    normalVectors.Count, texCords.Count, jointVectors.Count, weightVectors.Count);
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
                    Vertices[i][j] = new VertexPositionNormalTexture(positionVectors[j], normalVectors[j], texCords[j]);
                }

                //If there are joints
                if (jointCount > 0)
                {
                    //there is usually only one visual parent, and that parent would be the node of the entire model.
                    //(not the model root!). This baseNode is used in the calculation of Joint matrices.
                    Node baseNodeOfMesh = mesh.VisualParents.First();

                    //Get all the data regarding joints from the skin.
                    Skin modelSkin = modelRoot.LogicalSkins[i];
                    Joints[i] = new Joint[modelSkin.JointsCount];
                    JointMatrices[i] = new Matrix[modelSkin.JointsCount];
                    
                    for (int j = 0; j < Joints.Length; j++)
                    {
                        //can't use deconstruction because of the implicitly between Matrix and Matrix4x4
                        (Node Joint, Matrix InverseBindMatrix) modelJoint = modelSkin.GetJoint(j);

                        Joints[i]![j] = new Joint(modelJoint.Joint, JointMatrices[i]!, j, baseNodeOfMesh.WorldMatrix,
                            in modelJoint.InverseBindMatrix);
                    }

                    //JointsForVertices is for each vertex
                    JointsForVertices[i] = new Vector4[jointCount];
                    for (int j = 0; j < jointVectors.Count; j++)
                    {
                        JointsForVertices[i]![j] = jointVectors[j];
                    }
                }

                //If there are weights
                if (weightCount > 0)
                {
                    Weights[i] = new Vector4[weightCount];
                    for (int j = 0; j < weightCount; j++)
                    {
                        Weights[i]![j] = weightVectors[j];
                    }
                }

                _vertexBuffers[i] = new VertexBuffer(graphicsDevice, typeof(VertexPositionNormalTexture), posCount,
                    BufferUsage.WriteOnly);
                _vertexBuffers[i].SetData(Vertices[i]);

                uint[] indexBuffer = new uint[primitive.IndexAccessor.Count];
                primitive.IndexAccessor.AsIndicesArray().CopyTo(indexBuffer, 0);
                _indexBuffers[i] =
                    new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, indexBuffer.Length, BufferUsage.None);
                _indexBuffers[i].SetData(indexBuffer);
            }
        }

        /// <summary>
        /// Draws the mesh based on the data.
        /// </summary>
        /// <param name="worldMatrix">The world matrix that represents the scale, position, and rotation of the mesh to
        /// be drawn. This parameter has no "effect" (pun not-intended) if the Effect property does not implement
        /// IEffectMatrices.</param>
        /// <param name="viewMatrix">The view matrix that represents from what perspective the model is being viewed
        /// from. This parameter has no effect if the Effect property does not implement IEffectMatrices.</param>
        /// <param name="projectionMatrix">The projection matrix that represents certain properties of the viewer
        /// (field of view, render distance, etc.) This parameter has no effect if the Effect property does not
        /// implement IEffectMatrices.</param>
        /// <param name="rasterizerState">The RasterizierState the GraphicsDevice will use when rendering the mesh.</param>
        /// <param name="primitiveIndex">Determines which MeshPrimitive to draw. Default index is zero.</param>
        public void Draw(in Matrix worldMatrix, in Matrix viewMatrix, in Matrix projectionMatrix, RasterizerState rasterizerState, int primitiveIndex = 0)
        {
            RasterizerState prevState = GraphicsDevice.RasterizerState;
            GraphicsDevice.RasterizerState = rasterizerState;
            GraphicsDevice.SetVertexBuffer(_vertexBuffers[primitiveIndex]);
            GraphicsDevice.Indices = _indexBuffers[primitiveIndex];
            
            if (Effect is IEffectMatrices matrices)
            {
                (matrices.World, matrices.View, matrices.Projection) = (worldMatrix, viewMatrix, projectionMatrix);
            }

            foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0,
                    GraphicsDevice.Indices.IndexCount / 3);
            }

            GraphicsDevice.RasterizerState = prevState;
        }
    }
}