using System.Numerics;

namespace AppleScene.Helpers
{
    /// <summary>
    /// Offers extension methods for working with <see cref="System.Numerics"/>
    /// </summary>
    public static class NumericsExtensions
    {   
        /// <summary>
        /// Gets the upper vector formed from the second row M21, M22, M23 elements.
        /// </summary>
        public static Vector3 GetUp(this ref Matrix4x4 matrix) => new(matrix.M21, matrix.M22, matrix.M23);
        
        /// <summary>
        /// Gets the down vector formed from the second row -M21, -M22, -M23 elements.
        /// </summary>
        public static Vector3 GetDown(this ref Matrix4x4 matrix) => new(-matrix.M21, -matrix.M22, -matrix.M23);
        
        /// <summary>
        /// Gets the backward vector formed from the third row M31, M32, M33 elements.
        /// /// </summary>
        public static Vector3 GetBackward(this ref Matrix4x4 matrix) => new(matrix.M31, matrix.M32, matrix.M33);
        
        /// <summary>
        /// Gets the forward vector formed from the third row -M31, -M32, -M33 elements.
        /// </summary>
        public static Vector3 GetForward(this ref Matrix4x4 matrix) => new(-matrix.M31, -matrix.M32, -matrix.M33);

        /// <summary>
        /// Gets the right vector formed from the first row M11, M12, M13 elements.
        /// </summary>
        public static Vector3 GetRight(this ref Matrix4x4 matrix) => new(matrix.M11, matrix.M12, matrix.M13);
        
        /// <summary>
        /// Gets the left vector formed from the first row -M11, -M12, -M13 elements.
        /// </summary>
        public static Vector3 GetLeft(this ref Matrix4x4 matrix) => new(-matrix.M11, -matrix.M12, -matrix.M13);
    }
}