using System.Numerics;
using SharpGLTF.Animations;
using SharpGLTF.Schema2;
using SharpGLTF.Transforms;

namespace AppleScene.Helpers
{
    public class TransformSampler
    {
        public ICurveSampler<Vector3> TranslationSampler { get; }
        public ICurveSampler<Quaternion> RotationSampler { get; }
        public ICurveSampler<Vector3> ScaleSampler { get; }

        public TransformSampler(NodeCurveSamplers curveSamplers)
        {
            TranslationSampler = curveSamplers.Translation.CreateCurveSampler();
            RotationSampler = curveSamplers.Rotation.CreateCurveSampler();
            ScaleSampler = curveSamplers.Scale.CreateCurveSampler();
        }

        public Matrix4x4 GetTransformMatrix(float time) => Matrix4x4Factory.CreateFrom(null,
            ScaleSampler.GetPoint(time), RotationSampler.GetPoint(time), TranslationSampler.GetPoint(time));
    }
}