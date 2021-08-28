using NetScriptFramework;
using NetScriptFramework.SkyrimSE;
using System;
using System.Windows.Media.Media3D;

namespace SpellChargingPlugin.ParticleSystem
{
    public class Particle : IDisposable
    {
        public bool Delete { get; set; }
        public Vector3D Velocity => _velocity;
        public NiAVObject Object => _niAvObject;

        private NiAVObject _niAvObject;
        private Vector3D _velocity;

        public Particle(NiAVObject obj)
        {
            this._niAvObject = obj;
            obj.IncRef();
        }

        public void Dispose()
        {
            _niAvObject.Detach();
            _niAvObject.DecRef();
            _niAvObject = null;
        }

        public void SetVelocity(Vector3D velocity)
        {
            _velocity = velocity;
        }

        public void AttachToNode(NiNode parent)
        {
            parent.AttachObject(_niAvObject);
        }

        public void SetScale(float scale)
        {
            _niAvObject.LocalTransform.Scale = scale;
        }

        public Particle Clone()
        {
            var ret = new Particle(_niAvObject.Clone() as NiAVObject)
            {
                _velocity = _velocity
            };
            return ret;
        }

        internal void SetFade(float fadeValue)
        {
            if (_niAvObject is BSFadeNode)
            {
                var mptr = _niAvObject.Cast<BSFadeNode>();
                if (mptr != IntPtr.Zero)
                {
                    Memory.WriteFloat(mptr + 0x130, fadeValue);
                    Memory.WriteFloat(mptr + 0x140, fadeValue);
                }
            }
        }

        public void Translate(Vector3D offset)
        {
            using (var alloc = Memory.Allocate(0x10))
            {
                var pt = MemoryObject.FromAddress<NiPoint3>(alloc.Address);
                pt.X = (float)offset.X;
                pt.Y = (float)offset.Y;
                pt.Z = (float)offset.Z;
                _niAvObject.LocalTransform.Translate(pt, _niAvObject.LocalTransform.Position);
            }
        }

        public void Rotate(NiPoint3 rotationCenter, Vector3D axis, double degree)
        {
            var point = _niAvObject.LocalTransform.Position;
            // create empty matrix
            var matrix = new Matrix3D();
            var vPoint = new Point3D(point.X, point.Y, point.Z);
            var vRotationCenter = new Point3D(rotationCenter.X, rotationCenter.Y, rotationCenter.Z);
            // translate matrix to rotation point
            matrix.Translate(vRotationCenter - new Point3D());

            // rotate it the way we need
            matrix.Rotate(new Quaternion(axis, degree));

            // apply the matrix to our point
            vPoint = matrix.Transform(vPoint);
            point.X = (float)vPoint.X;
            point.Y = (float)vPoint.Y;
            point.Z = (float)vPoint.Z;
        }
    }

}
