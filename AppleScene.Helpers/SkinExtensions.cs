using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SharpGLTF.Schema2;

namespace AppleScene.Helpers
{
    /// <summary>
    /// Provides extension methods that provide extra functionality to <see cref="Skin"/> instances.
    /// </summary>
    public static class SkinExtensions
    {
        //I know we are repeating quite a bit for both overloads of CopyJointMatrices but I don't know how to elegantly
        //improve it. It's not really a big deal either way.

        //TODO: Add docs for both CopyJointMatrices overloads 

        private static readonly Dictionary<Animation, JointNode[]> JointCache = new();
       
        //both of these "param buffers" are used to call the CopyJointMatrices with just one animation without creating
        //more arrays than necessary.
        private static readonly Animation[] AnimParamBuffer = new Animation[1];

        public static Matrix[] CopyJointMatrices(this Skin skin,
            IEnumerable<(Animation animation, float currentTime)> animations, Matrix[] jointMatrices)
        {
            if (jointMatrices.Length < skin.JointsCount)
            {
                throw new IndexOutOfRangeException($"Size of joint matrices array ({jointMatrices.Length}) is " +
                                                   $"smaller than the number of joints in the skin ({skin.JointsCount}). ");
            }

            //there is usually only one visual parent, and that parent would be the node of the entire model.
            //(not the model root!). This baseNode is used in the calculation of Joint matrices.
            Matrix4x4 baseWorldMatrix = skin.VisualParents.FirstOrDefault()?.WorldMatrix ?? Matrix4x4.Identity;
            Matrix4x4.Invert(baseWorldMatrix, out var invertedWorldMatrix);

            bool firstIter = true;

            foreach (var (animation, currentTime) in animations.Reverse())
            {
                if (!JointCache.TryGetValue(animation, out var joints))
                {
                    joints = new JointNode[skin.JointsCount];
                    
                    for (int i = 0; i < skin.JointsCount; i++)
                    {
                        var (jointNode, inverseBindMatrix) = skin.GetJoint(i);
                        joints[i] = new JointNode(jointNode, null,
                            new TransformKeys(jointNode.GetCurveSamplers(animation)), inverseBindMatrix);
                    }

                    foreach (JointNode joint in joints)
                    {
                        joint.ParentJoint = Array.Find(joints, j => j.Node == joint.Node.VisualParent);
                    }

                    JointCache[animation] = joints;
                }

                int j = 0;
                foreach (JointNode joint in joints)
                {
                    Matrix4x4 jointMatrix = joint.InverseBindMatrix * invertedWorldMatrix;
                    Matrix4x4 jointWorldMatrix = joint.GetWorldTransformMatrix(currentTime);

                    jointMatrix *= jointWorldMatrix *
                                   (firstIter ? Matrix4x4.Identity : jointMatrices[j].ToNumerics());
                    
                    jointMatrices[j++] = jointMatrix;
                }

                firstIter = false;
            }

            if (firstIter)
            {
                skin.CopyBindMatrices(jointMatrices);
            }

            return jointMatrices;
        }

        public static Matrix[] CopyJointMatrices(this Skin skin, IEnumerable<Animation> animations,
            Matrix[] jointMatrices, float currentTime) =>
            skin.CopyJointMatrices(
                from anim in animations 
                select (anim, currentTime), jointMatrices);
        
        public static Matrix[] CopyJointMatrices(this Skin skin, Animation animation, Matrix[] jointMatrices,
            float currentTime)
        {
            AnimParamBuffer[0] = animation;

            return skin.CopyJointMatrices(AnimParamBuffer, jointMatrices, currentTime);
        }

        /// <summary>
        /// Creates a new array of matrices that represent the global transform matrices of each joint in a
        /// <see cref="Skin"/>
        /// </summary>
        /// <param name="skin">A <see cref="Skin"/> instance to make the joint matrices from.</param>
        /// <param name="animations">A collection <see cref="Animation"/> that will influence each joint matrix
        /// provided a float value representing time.</param>
        /// <param name="currentTime">Represents how long the animation has been occuring for in seconds.</param>
        /// <returns>An array of global-space matrices for each joint in a <see cref="Skin"/>.</returns>
        //animations is a ReadOnlySpan here for compatibility purposes.
        public static Matrix[] GetJointMatrices(this Skin skin, IEnumerable<Animation> animations, float currentTime)
        {
            Matrix[] jointMatrices = new Matrix[skin.JointsCount];

            return skin.CopyJointMatrices(animations, jointMatrices, currentTime);
        }

        public static Matrix[] GetJointMatrices(this Skin skin, Animation animation, float currentTime)
        {
            Matrix[] jointMatrices = new Matrix[skin.JointsCount];
            
            return skin.CopyJointMatrices(animation, jointMatrices, currentTime);
        }

        public static Matrix[] CopyBindMatrices(this Skin skin, Matrix[] bindMatrices)
        {
            if (bindMatrices.Length < skin.JointsCount)
            {
                throw new IndexOutOfRangeException($"Size of bind matrices array ({bindMatrices.Length}) is " +
                                                   $"smaller than the number of joints in the skin ({skin.JointsCount}). ");
            }
            
            for (int i = 0; i < bindMatrices.Length; i++)
            {
                bindMatrices[i] = Matrix.Identity;
            }

            return bindMatrices;
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
            
            return skin.CopyBindMatrices(jointMatrices);
        }
    }
}