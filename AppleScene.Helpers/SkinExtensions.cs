using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Microsoft.Xna.Framework;
using SharpGLTF.Schema2;

namespace AppleScene.Helpers
{
    /// <summary>
    /// Provides extension methods that provide extra functionality to <see cref="Skin"/> instances.
    /// </summary>
    public static class SkinExtensions
    {
        /// <summary>
        /// Creates a new array of matrices that represent the global transform matrices of each joint in a
        /// <see cref="Skin"/>
        /// </summary>
        /// <param name="skin">A <see cref="Skin"/> instance to make the joint matrices from.</param>
        /// <param name="animations">An array of <see cref="Animation"/> that will influence each joint matrix
        /// provided a float value representing time.</param>
        /// <param name="currentTime">Represents how long the animation has been occuring for in seconds.</param>
        /// <returns>An array of global-space matrices for each joint in a <see cref="Skin"/>.</returns>
        //animations is a ReadOnlySpan here for compatibility purposes.
        public static Matrix[] GetJointMatrices(this Skin skin, in ReadOnlySpan<Animation> animations, float currentTime)
        {
            Matrix[] jointMatrices = new Matrix[skin.JointsCount];

            //there is usually only one visual parent, and that parent would be the node of the entire model.
            //(not the model root!). This baseNode is used in the calculation of Joint matrices.
            Node baseNodeOfSkin = skin.VisualParents.First();

            for (int i = 0; i < skin.JointsCount; i++)
            {
                (Node joint, Matrix inverseBindMatrix) = skin.GetJoint(i);

                Matrix jointMatrix = inverseBindMatrix * Matrix.Invert(baseNodeOfSkin.WorldMatrix);
                foreach (Animation animation in animations)
                {
                    jointMatrix *= joint.GetWorldMatrix(animation, currentTime);
                }

                jointMatrices[i] = jointMatrix;
            }

            return jointMatrices.ToArray();
        }

        /// <summary>
        /// Creates a new array of matrices that represent the global transform matrices of each joint in a
        /// <see cref="Skin"/>. (With a collection of <see cref="ActiveAnimation"/> instances instead)
        /// </summary>
        /// <param name="skin">A <see cref="Skin"/> instance to make the joint matrices from.</param>
        /// <param name="animations">An array of <see cref="ActiveAnimation"/> instances that will influence
        /// each joint matrix with a <see cref="TimeSpan"/> a value representing how long the animation has been
        /// active for.</param>
        // we're repeating code here so that we don't have to create any more arrays or lists than we need to and
        // instead use ActiveAnimation instances directly.
        public static Matrix[] GetJointMatrices(this Skin skin, in ReadOnlySpan<ActiveAnimation> animations)
        {
            Matrix[] jointMatrices = new Matrix[skin.JointsCount];

            //there is usually only one visual parent, and that parent would be the node of the entire model.
            //(not the model root!). This baseNode is used in the calculation of Joint matrices.
            Node baseNodeOfSkin = skin.VisualParents.First();

            for (int i = 0; i < skin.JointsCount; i++)
            {
                (Node joint, Matrix inverseBindMatrix) = skin.GetJoint(i);

                Matrix jointMatrix = inverseBindMatrix * Matrix.Invert(baseNodeOfSkin.WorldMatrix);
                foreach (ActiveAnimation animation in animations)
                {
                    jointMatrix *=
                        joint.GetWorldMatrix(animation.Animation, (float) animation.CurrentTime.TotalSeconds);
                }

                jointMatrices[i] = jointMatrix;
            }

            return jointMatrices.ToArray();
        }

        /// <summary>
        /// Creates a new array of matrices that represent the global transform matrices of each joint in a
        /// <see cref="Skin"/>. (With a single <see cref="Animation"/> instance instead of an array)
        /// </summary>
        /// <param name="skin">A <see cref="Skin"/> instance to make the joint matrices from.</param>
        /// <param name="animation">An <see cref="Animation"/> that will influence each joint matrix provided a float
        /// value representing time.</param>
        /// <param name="currentTime">Represents how long the animation has been occuring for in seconds.</param>
        /// <returns>An array of global-space matrices for each joint in a <see cref="Skin"/>.</returns>
        public static Matrix[] GetJointMatrices(this Skin skin, Animation animation, float currentTime) =>
            skin.GetJointMatrices(new ReadOnlySpan<Animation>(new[] {animation}), currentTime);

        /// <summary>
        /// Returns an array of matrices that will result in the given <see cref="Skin"/> instance to be in it's bind
        /// position
        /// </summary>
        /// <param name="skin"><see cref="Skin"/> instance to create the matrices from.</param>
        /// <returns>An array of <see cref="Matrix.Identity"/> that should result in the given <see cref="Skin"/>
        /// to be in it's bind position.</returns>
        public static Matrix[] GetBindMatrices(this Skin skin)
        {
            Matrix[] jointMatrices = new Matrix[skin.JointsCount];

            for (int i = 0; i < jointMatrices.Length; i++)
            {
                jointMatrices[i] = Matrix.Identity;
            }

            return jointMatrices;
        }
    }
    
    /// <summary>
    /// Represents an <see cref="Animation"/> instance along with how long the animation has been running for.
    /// </summary>
    public record ActiveAnimation
    {
        /// <summary>
        /// The <see cref="Animation"/> instance.
        /// </summary>
        public Animation Animation { get; init; }
        
        /// <summary>
        /// Determines if the animation is active or not.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// How long the animation has been running for.
        /// </summary>
        public TimeSpan CurrentTime;
    }
}