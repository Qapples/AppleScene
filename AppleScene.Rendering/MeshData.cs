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
        /// Represents a list of all animations that are currently active. Not all active animations necessarily have
        /// to be from <see cref="Animations"/>, but it is highly recommended that they are.
        /// </summary>
        public List<ActiveAnimation> ActiveAnimations { get; init; }
        
        /// <summary>
        /// A list of all the animations the mesh should be capable of using.
        /// </summary>
        public ImmutableArray<Animation> Animations { get; init; }

        /// <summary>
        /// Returns true if <see cref="Animations"/> has any animations contained within it. Otherwise, false.
        /// </summary>
        public bool HasAnimations => Animations.IsEmpty;
        
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
        /// <param name="animations">Optional <see cref="ImmutableArray{T}"/> that represents a list of all the
        /// animations the mesh is capable of using.</param>
        public MeshData(Mesh mesh, GraphicsDevice graphicsDevice, Effect effect, Skin? skin = null,
            ImmutableArray<Animation>? animations = null)
        {
            Primitives = new PrimitiveData[mesh.Primitives.Count];
            
            for (int i = 0; i < mesh.Primitives.Count; i++)
            {
                Primitives[i] = new PrimitiveData(mesh.Primitives[i], graphicsDevice, skin);
            }
            
            (GraphicsDevice, Effect) = (graphicsDevice, effect);
            (Animations, ActiveAnimations) = (animations ?? ImmutableArray<Animation>.Empty,
                new List<ActiveAnimation>());
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
        /// <param name="elapsedTime"></param>
        /// <param name="rasterizerState">The <see cref="RasterizerState"/> the stored <see cref="GraphicsDevice"/>
        /// will use when rendering the mesh.</param>
        public void Draw(in Matrix worldMatrix, in Matrix viewMatrix, in Matrix projectionMatrix, 
            in TimeSpan elapsedTime, RasterizerState rasterizerState)
        {
            foreach (var primitive in Primitives)
            {
                //we can get away with passing a List as a Span here because we won't be adding or removing anything
                //from the list during drawing.
                primitive.Draw(in worldMatrix, in viewMatrix, in projectionMatrix,
                    CollectionsMarshal.AsSpan(ActiveAnimations), Effect, rasterizerState);
            }
            
            //Update ActiveAnimations
            for (int i = ActiveAnimations.Count - 1; i > -1; i--)
            {
                ref TimeSpan currentTime = ref ActiveAnimations[i].CurrentTime;
                currentTime += elapsedTime;

                if ((float) currentTime.TotalSeconds >= ActiveAnimations[i].Animation.Duration)
                {
                    ActiveAnimations.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Adds an animation to the <see cref="ActiveAnimations"/> list and therefore declaring it as active. Does not
        /// add to <see cref="ActiveAnimations"/> if the animation parameter given is already referenced in
        /// <see cref="ActiveAnimations"/>. If the animation parameter given does not exist in
        /// <see cref="Animations"/>, the animation will still be added, but the model may not be animated or displayed
        /// correctly.
        /// </summary>
        /// <param name="animation">The <see cref="Animation"/> instance to activate.</param>
        public void ActivateAnimation(Animation animation)
        {
            //reminder that we're only seeing if the "animation" parameter is reference. not an actual equity check.
            if (ActiveAnimations.Any(e => e.Animation == animation))
            {
                Debug.WriteLine($"The animation parameter given is already active. Animation object: {animation}");
                return;
            }

            if (Animations.Any(e => e == animation))
            {
                Debug.WriteLine($"The animation parameter given. does not exist in the Animations property. The " +
                                $"model may not be animated correctly. Animation object: {animation}");
            }
            
            ActiveAnimations.Add(new ActiveAnimation
            {
                Animation = animation,
                CurrentTime = TimeSpan.Zero
            });
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