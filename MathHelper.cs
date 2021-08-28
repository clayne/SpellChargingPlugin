using NetScriptFramework.SkyrimSE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace SpellChargingPlugin
{
    public static class MathHelper
    {
        public static void Rotate(NiPoint3 point, NiPoint3 rotationCenter, Vector3D rotation, double degree)
        {
            // create empty matrix
            var matrix = new Matrix3D();
            var vPoint = new Point3D(point.X, point.Y, point.Z);
            var vRotationCenter = new Point3D(rotationCenter.X, rotationCenter.Y, rotationCenter.Z);
            // translate matrix to rotation point
            matrix.Translate(vRotationCenter - new Point3D());

            // rotate it the way we need
            matrix.Rotate(new Quaternion(rotation, degree));

            // apply the matrix to our point
            vPoint = matrix.Transform(vPoint);
            point.X = (float)vPoint.X;
            point.Y = (float)vPoint.Y;
            point.Z = (float)vPoint.Z;
        }
    }
}
