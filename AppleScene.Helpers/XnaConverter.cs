using Microsoft.Xna.Framework;

namespace AppleScene.Helpers
{
    /// <summary>
    /// Class responsible for converting SharpGLTF data types into a corresponding XNA datatype.
    /// </summary>
    public static class XnaConverter
    {
        //We're going to explicitly define each type that can be converted as a separate method as to avoid boxing from
        //using "object"

        /// <summary>
        /// Converts <see cref="System.Numerics.Matrix4x4"/> to <see cref="Microsoft.Xna.Framework.Matrix"/>.
        /// </summary>
        /// <param name="matrix"><see cref="System.Numerics.Matrix4x4"/> instance to convert.</param>
        /// <returns>A new <see cref="Microsoft.Xna.Framework.Matrix"/> instance that holds the same values as the
        /// original <see cref="System.Numerics.Matrix4x4"/></returns>
        public static Matrix ToXna(this in System.Numerics.Matrix4x4 matrix) =>
            new(matrix.M11, matrix.M12, matrix.M13, matrix.M14, 
                matrix.M21, matrix.M22, matrix.M23, matrix.M24,
                matrix.M31, matrix.M32, matrix.M33, matrix.M34,
                matrix.M41, matrix.M42, matrix.M43, matrix.M44);

        /// <summary>
        /// Converts <see cref="System.Numerics.Vector3"/> to <see cref="Microsoft.Xna.Framework.Vector3"/>.
        /// </summary>
        /// <param name="vector">S<see cref="System.Numerics.Vector3"/> instance to convert.</param>
        /// <returns>A new <see cref="Microsoft.Xna.Framework.Vector3"/> instance that holds the same values as the
        /// original <see cref="System.Numerics.Vector3"/></returns>
        public static Vector3 ToXna(this in System.Numerics.Vector3 vector) => new(vector.X, vector.Y, vector.Z);

        /// <summary>
        /// Converts <see cref="System.Numerics.Vector4"/> to <see cref="Microsoft.Xna.Framework.Vector4"/>.
        /// </summary>
        /// <param name="vector">S<see cref="System.Numerics.Vector4"/> instance to convert.</param>
        /// <returns>A new <see cref="Microsoft.Xna.Framework.Vector4"/> instance that holds the same values as the
        /// original <see cref="System.Numerics.Vector4"/></returns>
        public static Vector4 ToXna(this in System.Numerics.Vector4 vector) =>
            new(vector.X, vector.Y, vector.Z, vector.W);
    }
}