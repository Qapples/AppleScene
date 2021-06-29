using System;
using System.Linq;
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
        /// Creates a new list of matrices that represent the global transform matrices of each joint in a
        /// <see cref="Skin"/>
        /// </summary>
        /// <param name="skin">S<see cref="Skin"/> instance to make the joint matrices from.</param>
        /// <returns>An array of global-space matrices for each joint in a skin,</returns>
        public static Matrix[] GetJointMatrices(this Skin skin)
        {
            Matrix[] jointMatrices = new Matrix[skin.JointsCount];
            
            //there is usually only one visual parent, and that parent would be the node of the entire model.
            //(not the model root!). This baseNode is used in the calculation of Joint matrices.
            Node baseNodeOfSkin = skin.VisualParents.First();

            for (int i = 0; i < skin.JointsCount; i++)
            {
                var (node, inverseBindMatrix) = skin.GetJoint(i);

                jointMatrices[i] = Matrix.Invert(baseNodeOfSkin.WorldMatrix) * node.WorldMatrix *
                                   inverseBindMatrix;
            }

            return jointMatrices;
        }
    }
}