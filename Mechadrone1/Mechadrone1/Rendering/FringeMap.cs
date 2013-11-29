using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Mechadrone1.Rendering
{
    class FringeMap
    {
        public static Texture2D CreateFringeMap(GraphicsDevice gd)
        {
            const int FRINGE_MAP_SIZE = 1024;
            Texture2D fringeMap = new Texture2D(gd, FRINGE_MAP_SIZE, 1, false, SurfaceFormat.Vector4);

            Vector4[] proceduralTexData = new Vector4[FRINGE_MAP_SIZE];

            // these lambdas are in 100s of nm,
            //  they represent the wavelengths of light for each respective 
            //  color channel.  They are only approximate so that the texture
            //  can repeat.
            // (600,500,400)nm - should be more like (600,550,440):
            Vector3 lamRGB = new Vector3(6.0f, 5.0f, 4.0f);
            // these offsets are used to perturb the phase of the interference
            //   if you are using very thick "thin films" you will want to
            //   modify these offests to avoid complete contructive interference
            //   at a particular depth.. Just a tweak-able.
            Vector3 offsetRGB = Vector3.Zero;
            // p is the period of the texture, it is the LCM of the wavelengths,
            //  this is the depth in nm when the pattern will repeat.  I was too
            //  lazy to write up a LCM function, so you have to provide it.
            //lcm(6,5,4)
            float p = 60.0f;
            // vd is the depth of the thin film relative to the texture index
            float vd = p;
            // now compute the color values using this formula:
            //  1/2 ( Sin( 2Pi * d/lam* + Pi/2 + O) + 1 )
            //   where d is the current depth, or "i*vd" and O is some offset* so that
            //   we avoid complete constructive interference in all wavelenths at some
            //   depth.

            float u;
            for (int i = 0; i < FRINGE_MAP_SIZE; i++)
            {
                u = (float)i / (float)(FRINGE_MAP_SIZE - 1);
                Vector4 rgb = Vector4.Zero;
                rgb.X = 0.5f * (float)(Math.Sin(MathHelper.TwoPi * (u * vd) / lamRGB.X + MathHelper.PiOver2 + offsetRGB.X) + 1.0f);
                rgb.Y = 0.5f * (float)(Math.Sin(MathHelper.TwoPi * (u * vd) / lamRGB.Y + MathHelper.PiOver2 + offsetRGB.Y) + 1.0f);
                rgb.Z = 0.5f * (float)(Math.Sin(MathHelper.TwoPi * (u * vd) / lamRGB.Z + MathHelper.PiOver2 + offsetRGB.Z) + 1.0f);
                rgb.W = 0.0f;
                proceduralTexData[i] = rgb;
            }

            fringeMap.SetData<Vector4>(proceduralTexData);

            return fringeMap;
        }
    }
}
