#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using AppleScene.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpGLTF.Schema2;

namespace AppleScene.Rendering
{
    /// <summary>
    /// Represents data regarding instances of <see cref="Mesh"/> and includes multiple <see cref="PrimitiveData"/>
    /// instances.
    /// </summary>
    public class MeshData : IDisposable
    {
        /// <summary>
        /// Data regarding each primitive in the loaded mesh
        /// </summary>
        public PrimitiveData[] Primitives { get; set; }

        /// <summary>
        /// <see cref="GraphicsDevice"/> instance used to draw the mesh and to create <see cref="VertexBuffer"/> and
        /// <see cref="IndexBuffer"/> instances.
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; set; }
        
        /// <summary>
        /// Represents how the mesh be drawn. Contains information such as the world, view, and projection matrices
        /// that influence where the mesh will be drawn and skinning/joints behavior.
        /// </summary>
        public Effect Effect { get; set; }

        /// <summary>
        /// Creates a new instance of <see cref="MeshData"/>.
        /// </summary>
        /// <param name="mesh"><see cref="Mesh"/> instance to make the <see cref="MeshData"/> instance from.</param>
        /// <param name="graphicsDevice">Used to draw the mesh and to create <see cref="VertexBuffer"/> and
        /// <see cref="IndexBuffer"/> instances. </param>
        /// <param name="effect"> Represents how the mesh be drawn. Contains information such as the world, view,
        /// and projection matrices that influence where the mesh will be drawn and skinning/joints behavior.</param>
        /// <param name="skin">Optional <see cref="Skin"/> instance that defines skinning data so that the mesh
        /// can be influenced by animation.</param>
        public MeshData(Mesh mesh, GraphicsDevice graphicsDevice, Effect effect, Skin? skin = null)
        {
            Primitives = new PrimitiveData[mesh.Primitives.Count];
            
            for (int i = 0; i < mesh.Primitives.Count; i++)
            {
                Primitives[i] = new PrimitiveData(mesh.Primitives[i], graphicsDevice, skin);
            }
            
            (GraphicsDevice, Effect) = (graphicsDevice, effect);
        }

        /// <summary>
        /// Draws each of the <see cref="MeshPrimitive"/> instances. Also updates the animations in 
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
        /// <see cref="ReadOnlySpan{T}.Empty"/> to indicate so.</param>
        /// <param name="rasterizerState">The <see cref="RasterizerState"/> the stored <see cref="GraphicsDevice"/>
        /// will use when rendering the mesh.</param>
        public void Draw(in Matrix worldMatrix, in Matrix viewMatrix, in Matrix projectionMatrix,
            in ReadOnlySpan<ActiveAnimation> animations, RasterizerState rasterizerState)
        {
            foreach (var primitive in Primitives)
            {
                primitive.Draw(in worldMatrix, in viewMatrix, in projectionMatrix, in animations, Effect,
                    rasterizerState);
            }
        }

        /// <summary>
        /// Disposes the <see cref="MeshData"/> instances and it's <see cref="PrimitiveData"/> instances.
        /// </summary>
        public void Dispose()
        {
            //call this so that any derived types with a finalizer doesn't have to do this
            GC.SuppressFinalize(this);

            for (int i = 0; i < Primitives.Length; i++)
            {
                Primitives[i].Dispose();
            }
            
            Effect.Dispose();
        }
    }
}