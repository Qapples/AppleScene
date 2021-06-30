#nullable enable
using System;
using System.Linq;
using AppleScene.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpGLTF.Runtime;
using SharpGLTF.Schema2;
using PrimitiveType = Microsoft.Xna.Framework.Graphics.PrimitiveType;

namespace AppleScene.Rendering
{
    /// <summary>
    /// Represents the data of a <see cref="MeshPrimitive"/>.
    /// </summary>
    public struct PrimitiveData : IDisposable
    {
        /// <summary>
        /// <see cref="VertexDataHandler"/> instance that handles the vertex data of the primitive.
        /// </summary>
        public VertexDataHandler? VertexData { get; private set; }
        
        /// <summary>
        /// If the primitive has joints, this field represents the world-space transforms of each joint the primitive
        /// has. If the primitive does not have joints, then this field is null.
        /// </summary>
        public Matrix[]? JointMatrices { get; private set; }

        private readonly VertexBuffer _vertexBuffer;
        private readonly IndexBuffer _indexBuffer;

        private readonly GraphicsDevice _graphicsDevice;

        /// <summary>
        /// Creates a <see cref="PrimitiveData"/> instance.
        /// </summary>
        /// <param name="vertexData">A <see cref="VertexDataHandler"/> instance that represents the data of the
        /// vertices.</param>
        /// <param name="indexBuffer">The <see cref="IndexBuffer"/> that is loaded with the indices of the primitive.
        /// </param>
        /// <param name="graphicsDevice">Used to generate a <see cref="VertexBuffer"/> and to draw the primitive.
        /// </param>
        /// <param name="jointMatrices">If the primitive has joints, this matrix represents the world-space transforms
        /// of each joint the primitive has. If the primitive has no joints, set this parameter to null to indicate
        /// as such.</param>
        public PrimitiveData(VertexDataHandler vertexData, IndexBuffer indexBuffer, GraphicsDevice graphicsDevice,
            Matrix[]? jointMatrices = null)
        {
            (VertexData, JointMatrices, _indexBuffer, _graphicsDevice) =
                (vertexData, jointMatrices, indexBuffer, graphicsDevice);

            _vertexBuffer = vertexData.GenerateVertexBuffer(graphicsDevice);
        }

        /// <summary>
        /// Creates a <see cref="PrimitiveData"/> instance with a <see cref="MeshPrimitive"/> and an
        /// optional <see cref="Skin"/>.
        /// </summary>
        /// <param name="primitive">The <see cref="MeshPrimitive"/> instance to get the vertex and index data from
        /// </param>
        /// <param name="graphicsDevice">Used to generate a <see cref="VertexBuffer"/> and to draw the primitive.
        /// </param>
        /// <param name="primitiveSkin">If the primitive has joints, this <see cref="Skin"/> instance will be used to
        /// generate the joint matrices which are used to manipulate the position of the vertices based on it's own
        /// position and orientation. If the primitive does not have joints, set this parameter to null to indicate
        /// as such.</param>
        public PrimitiveData(MeshPrimitive primitive, GraphicsDevice graphicsDevice, Skin? primitiveSkin = null)
        {
            IMeshPrimitiveDecoder decoder = primitive.GetDecoder();
            VertexDeclaration decl = primitive.GetDeclaration();
            
            VertexData = new VertexDataHandler(decoder.GetXnaByteData(decl), decl);

            _vertexBuffer = VertexData.GenerateVertexBuffer(graphicsDevice);
            _indexBuffer = primitive.GetIndexBuffer(graphicsDevice);

            _graphicsDevice = graphicsDevice;

            JointMatrices = primitiveSkin?.GetJointMatrices();
        }
         
        /// <summary>
        /// Draws the primitive based on the data stored in this instance. 
        /// </summary>
        /// <param name="worldMatrix">The world matrix that represents the scale, position, and rotation of the mesh to
        /// be drawn. This parameter is irrelevant if the effect parameter does not implement
        /// <see cref="IEffectMatrices"/>.</param>
        /// <param name="viewMatrix">The view matrix that represents from what perspective the model is being viewed
        /// from. This parameter has no effect if the Effect parameter does not implement
        /// <see cref="IEffectMatrices"/>.</param>
        /// <param name="projectionMatrix">The projection matrix that represents certain properties of the viewer
        /// (field of view, render distance, etc.) This parameter has no effect if the Effect property does not
        /// implement <see cref="IEffectMatrices"/>.</param>
        /// <param name="effect">An <see cref="Effect"/> instance that influences the way the primitive is drawn.
        /// This parameter must implement <see cref="IEffectMatrices"/> to apply world, view, and projection matrices.
        /// In addition, this parameter must implement <see cref="IEffectBones"/> for skinning, joints, and animation
        /// to be applied.</param>
        /// <param name="rasterizerState">The <see cref="RasterizerState"/> the stored <see cref="GraphicsDevice"/>
        /// will use when rendering the mesh.</param>
        public void Draw(in Matrix worldMatrix, in Matrix viewMatrix, in Matrix projectionMatrix, Effect effect,
            RasterizerState rasterizerState)
        {
            RasterizerState prevState = _graphicsDevice.RasterizerState;
            _graphicsDevice.RasterizerState = rasterizerState;
            _graphicsDevice.SetVertexBuffer(_vertexBuffer);
            _graphicsDevice.Indices = _indexBuffer;

            //An effect can still have matrices without being an IEffectMatrices if a base Effect instance is passed
            //as a parameter rather than a subclass of Effect.
            if (effect is IEffectMatrices matrices)
            {
                (matrices.World, matrices.View, matrices.Projection) = (worldMatrix, viewMatrix, projectionMatrix);
            }

            if (effect is IEffectBones bones && JointMatrices is not null)
            {
                bones.SetBoneTransforms(JointMatrices);
            }

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                
                _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0,
                    _graphicsDevice.Indices.IndexCount / 3);
            }

            _graphicsDevice.RasterizerState = prevState;
        }

        #nullable disable
        public void Dispose()
        {
            VertexData = null;
            JointMatrices = null;
            
            _vertexBuffer.Dispose();
            _indexBuffer.Dispose();
        }
    }
}