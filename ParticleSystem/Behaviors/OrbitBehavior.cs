using NetScriptFramework.SkyrimSE;
using System;
using System.Windows.Media.Media3D;

namespace SpellChargingPlugin.ParticleSystem.Behaviors
{
    /// <summary>
    /// Rotate around point in LOCAL space. TODO: Make it work in world space.
    /// </summary>
    public class OrbitBehavior : IParticleBehavior
    {
        public Func<bool> Active { get; set; } = () => true;

        private readonly Vector3D _center;
        private readonly Vector3D _axis;
        private readonly Particle _particle;
        private readonly float _speedFactor;

        public OrbitBehavior(Particle particle, Vector3D center, Vector3D axis, float speedFactor)
        {
            _particle = particle;
            _center = center;
            _axis = axis;
            _speedFactor = speedFactor;
        }

        public void Update(float elapsedSeconds)
        {
            if (!Active())
                return;

            Rotate(_particle.Object.LocalTransform.Position, _center, _axis, 180f * elapsedSeconds * _speedFactor);
        }

        // TODO: there is probably a more efficient way (caching? AVX?)
        private void Rotate(NiPoint3 point, Vector3D rotationCenter, Vector3D axis, double angle)
        {
            // create empty matrix
            var matrix = new Matrix3D();
            var vPoint = new Point3D(point.X, point.Y, point.Z);
            var vRotationCenter = new Point3D(rotationCenter.X, rotationCenter.Y, rotationCenter.Z);
            // translate matrix to rotation point
            matrix.Translate(vRotationCenter - new Point3D());

            // rotate it the way we need
            matrix.Rotate(new Quaternion(axis, angle));

            // apply the matrix to our point
            vPoint = matrix.Transform(vPoint);
            point.X = (float)vPoint.X;
            point.Y = (float)vPoint.Y;
            point.Z = (float)vPoint.Z;
        }
    }
}
