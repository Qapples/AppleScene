#nullable enable
using System;
using System.Collections.Generic;
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
    public sealed class PrimitiveData : IDisposable
    {
        /// <summary>
        /// <see cref="VertexDataHandler"/> instance that handles the vertex data of the primitive.
        /// </summary>
        public VertexDataHandler VertexData { get; private set; }
        
        /// <summary>
        /// Represents the skin of the primitive. If null, then the primitive has no skin.
        /// </summary>
        public Skin? Skin { get; private set; }

        private Matrix[]? _jointMatrices;
        
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
        /// <param name="skin">If the primitive has joints, this <see cref="Skin"/> instance will be used to
        /// generate the joint matrices which are used to manipulate the position of the vertices based on it's own
        /// position and orientation. If the primitive does not have joints, set this parameter to null to indicate
        /// as such.</param>
        public PrimitiveData(VertexDataHandler vertexData, IndexBuffer indexBuffer, GraphicsDevice graphicsDevice,
            Skin? skin = null)
        {
            (VertexData, Skin, _indexBuffer, _graphicsDevice) =
                (vertexData, skin, indexBuffer, graphicsDevice);

            _vertexBuffer = vertexData.GenerateVertexBuffer(graphicsDevice);

            if (skin is not null)
            {
                _jointMatrices = new Matrix[skin.JointsCount];
            }
        }

        /// <summary>
        /// Creates a <see cref="PrimitiveData"/> instance with a <see cref="MeshPrimitive"/> and an
        /// optional <see cref="Skin"/>.
        /// </summary>
        /// <param name="primitive">The <see cref="MeshPrimitive"/> instance to get the vertex and index data from
        /// </param>
        /// <param name="graphicsDevice">Used to generate a <see cref="VertexBuffer"/> and to draw the primitive.
        /// </param>
        /// <param name="skin">If the primitive has joints, this <see cref="Skin"/> instance will be used to
        /// generate the joint matrices which are used to manipulate the position of the vertices based on it's own
        /// position and orientation. If the primitive does not have joints, set this parameter to null to indicate
        /// as such.</param>
        public PrimitiveData(MeshPrimitive primitive, GraphicsDevice graphicsDevice, Skin? skin = null)
        {
            IMeshPrimitiveDecoder decoder = primitive.GetDecoder();
            VertexDeclaration decl = primitive.GetDeclaration();

            (VertexData, Skin) = (new VertexDataHandler(decoder.GetXnaByteData(decl), decl), skin);

            (_vertexBuffer, _indexBuffer) =
                (VertexData.GenerateVertexBuffer(graphicsDevice), primitive.GetIndexBuffer(graphicsDevice));

            _graphicsDevice = graphicsDevice;
            
            if (skin is not null)
            {
                _jointMatrices = new Matrix[skin.JointsCount];
            }
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
        /// <param name="animations">This represents the animations that will be applied to <see cref="Skin"/>. Each
        /// <see cref="ActiveAnimation"/> instance also comes with a <see cref="ActiveAnimation.CurrentTime"/> that
        /// indicates how long the animation has been running for. To represent a lack of animations, pass
        /// <see cref="Array.Empty{T}"/> to indicate so.</param>
        /// <param name="jointTransforms">These matrices will be applied to the joints of the <see cref="Skin"/>.
        /// Pass <see cref="ReadOnlySpan{T}.Empty"/> to indicate that no joint transformations outside of animations
        /// will be applied. </param>
        /// <param name="effect">An <see cref="Effect"/> instance that influences the way the primitive is drawn.
        /// This parameter must implement <see cref="IEffectMatrices"/> to apply world, view, and projection matrices.
        /// In addition, this parameter must implement <see cref="IEffectBones"/> for skinning, joints, and animation
        /// to be applied.</param>
        /// <param name="rasterizerState">The <see cref="RasterizerState"/> the stored <see cref="GraphicsDevice"/>
        /// will use when rendering the mesh.</param>
        // we're using a ReadOnlySpan here for compatibility purposes (it can reference anything without creating any
        // additional copies (I think)).
        public void Draw(in Matrix worldMatrix, in Matrix viewMatrix, in Matrix projectionMatrix,
            IList<ActiveAnimation> animations, in ReadOnlySpan<Matrix> jointTransforms, Effect effect,
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
            
            if (effect is IEffectBones bones && Skin is not null && _jointMatrices is not null)
            {
                if (animations.Count > 0)
                {
                    Skin.CopyJointMatrices(animations, _jointMatrices);
                }
                else
                {
                    Skin.CopyBindMatrices(_jointMatrices);
                }

                for (int i = 0; i < _jointMatrices.Length; i++)
                {
                    _jointMatrices[i] *= jointTransforms[i];
                }
                
                bones.SetBoneTransforms(_jointMatrices);
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
        /// <summary>
        /// Diposes the stored buffer
        /// </summary>
        public void Dispose()
        {
            _vertexBuffer.Dispose();
            _indexBuffer.Dispose();
        }
    }
}