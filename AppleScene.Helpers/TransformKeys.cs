using System;
using System.IO.Pipes;
using System.Linq;
using System.Numerics;
using SharpGLTF.Animations;
using SharpGLTF.Schema2;
using SharpGLTF.Transforms;
using SharpGLTF.Validation;

namespace AppleScene.Helpers
{
    public class TransformKeys
    {
        private readonly AnimationKeys<Vector3> _scaleKeys;
        private readonly AnimationKeys<Quaternion> _rotationKeys;
        private readonly AnimationKeys<Vector3> _translationKeys;

        private static readonly VectorInterpolateHelper VectorInterpolateHelperInstance = new();
        private static readonly QuaternionInterpolateHelper QuaternionInterpolateHelperInstance = new();
        
        public TransformKeys(NodeCurveSamplers curveSamplers)
        {
            _rotationKeys = new AnimationKeys<Quaternion>(curveSamplers.Rotation, QuaternionInterpolateHelperInstance);
            _scaleKeys = new AnimationKeys<Vector3>(curveSamplers.Scale, VectorInterpolateHelperInstance);
            _translationKeys = new AnimationKeys<Vector3>(curveSamplers.Translation, VectorInterpolateHelperInstance);
        }

        public Matrix4x4 GetTransformMatrix(float time) => Matrix4x4Factory.CreateFrom(null,
            _scaleKeys.GetValue(time), _rotationKeys.GetValue(time), _translationKeys.GetValue(time));

        private class AnimationKeys<T> where T : struct
        {
            public (float Key, T Value)[]? LinearKeys { get; init; }
            public (float Key, (T TangentIn, T Value, T TangentOut) Value)[]? CubicKeys { get; init; }
            public AnimationInterpolationMode InterpolationMode { get; init; }

            private readonly IInterpolateHelper<T> _interpolateHelper;

            public AnimationKeys(IAnimationSampler<T> sampler, IInterpolateHelper<T> interpolateHelper)
            {
                _interpolateHelper = interpolateHelper;
                InterpolationMode = sampler.InterpolationMode;
                
                switch (InterpolationMode)
                {
                    case AnimationInterpolationMode.STEP or AnimationInterpolationMode.LINEAR:
                        LinearKeys = sampler.GetLinearKeys().ToArray();
                        break;
                    case AnimationInterpolationMode.CUBICSPLINE:
                        CubicKeys = sampler.GetCubicKeys().ToArray();
                        break;
                }
            }

            public T GetValue(float time)
            {
                switch (InterpolationMode)
                {
                    case AnimationInterpolationMode.STEP:
                        return LinearKeys.FindRangeContainingOffset(time).A;
                    case AnimationInterpolationMode.LINEAR:
                        var (startLinear, endLinear, valueLinear) = LinearKeys.FindRangeContainingOffset(time);
                        return _interpolateHelper.InterpolateLinear(startLinear, endLinear, valueLinear);
                    case AnimationInterpolationMode.CUBICSPLINE:
                        var (startCubic, endCubic, valueCubic) = CubicKeys.FindRangeContainingOffset(time);
                        return _interpolateHelper.InterpolateCubic(startCubic.Value, startCubic.TangentOut,
                            endCubic.Value, endCubic.TangentIn, valueCubic);
                }

                return default;
            }
        }

        private interface IInterpolateHelper<T> where T : struct
        {
            T InterpolateLinear(T start, T end, float amount);
            T InterpolateCubic(T start, T outgoingTangent, T end, T incomingTangent, float amount);
        }
        
        private class VectorInterpolateHelper : IInterpolateHelper<Vector3>
        {
            public Vector3 InterpolateLinear(Vector3 start, Vector3 end, float amount) =>
                Vector3.Lerp(start, end, amount);
            
            public Vector3 InterpolateCubic(Vector3 start, Vector3 outgoingTangent, Vector3 end,
                Vector3 incomingTangent, float amount)
                => CurveSampler.InterpolateCubic(start, outgoingTangent, end, incomingTangent, amount);
        }

        private class QuaternionInterpolateHelper : IInterpolateHelper<Quaternion>
        {
            public Quaternion InterpolateLinear(Quaternion start, Quaternion end, float amount) =>
                Quaternion.Lerp(start, end, amount);

            public Quaternion InterpolateCubic(Quaternion start, Quaternion outgoingTangent, Quaternion end,
                Quaternion incomingTangent, float amount) =>
                CurveSampler.InterpolateCubic(start, outgoingTangent, end, incomingTangent, amount);
        }
    }
}