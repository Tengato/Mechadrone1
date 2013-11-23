using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace SlagformCommon
{
    public class BepuConverter
    {
        public static BEPUutilities.Vector3 Convert(Vector3 a)
        {
            return new BEPUutilities.Vector3(a.X, a.Y, a.Z);
        }


        public static Vector2 Convert(BEPUutilities.Vector2 a)
        {
            return new Vector2(a.X, a.Y);
        }


        public static Vector3 Convert(BEPUutilities.Vector3 a)
        {
            return new Vector3(a.X, a.Y, a.Z);
        }


        public static BoundingBox Convert(BEPUutilities.BoundingBox b)
        {
            return new BoundingBox(Convert(b.Min), Convert(b.Max));
        }


        public static BEPUutilities.Quaternion Convert(Quaternion q)
        {
            return new BEPUutilities.Quaternion(q.X, q.Y, q.Z, q.W);
        }


        public static Quaternion Convert(BEPUutilities.Quaternion q)
        {
            return new Quaternion(q.X, q.Y, q.Z, q.W);
        }
    }
}
