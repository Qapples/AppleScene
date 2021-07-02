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
        /// <param name="animations"></param>
        /// <param name="time"></param>
        /// <returns>An array of global-space matrices for each joint in a <see cref="Skin"/>.</returns>
        //animations is a ReadOnlySpan here for compatibility purposes.
        public static Matrix[] GetJointMatrices(this Skin skin, in ReadOnlySpan<Animation> animations, float time)
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
                    jointMatrix *= joint.GetWorldMatrix(animation, time);
                }

                jointMatrices[i] = jointMatrix;
            }

            return jointMatrices.ToArray();
        }
        
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
}