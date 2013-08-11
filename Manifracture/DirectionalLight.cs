using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Manifracture
{
    public struct DirectLight
    {
        public Vector4 Ambient;
        public Vector4 Diffuse;
        public Vector4 Specular;
        public Vector3 Direction;
        public float Energy;
    }
}
