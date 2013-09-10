using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mechadrone1.Rendering;

namespace Mechadrone1.Gameplay.Prefabs
{
    class WireBox : GameObject
    {

        VertexPositionColor[] markerGeometry;

        public WireBox(Vector3[] verts, ref VertexPositionColor[] gpuBox)
        {

            markerGeometry = new VertexPositionColor[24];

            int a = 0;
            for (int i = 0; i < 4; i++)
            {
                gpuBox[a].Color = i == 0 ? Color.HotPink : Color.Cyan;
                gpuBox[a++].Position = verts[i];
                gpuBox[a].Color = i == 0 ? Color.HotPink : Color.Cyan;
                int n = (i + 1) % 4;
                gpuBox[a++].Position = verts[n];
            }
            for (int i = 0; i < 4; i++)
            {
                gpuBox[a].Color = i == 0 ? Color.HotPink : Color.Yellow;
                gpuBox[a++].Position = verts[i + 4];
                gpuBox[a].Color = i == 0 ? Color.HotPink : Color.Yellow;
                int n = (i + 1) % 4;
                gpuBox[a++].Position = verts[n + 4];
            }
            for (int i = 0; i < 4; i++)
            {
                gpuBox[a].Color = Color.LimeGreen;
                gpuBox[a++].Position = verts[i];
                gpuBox[a].Color = Color.LimeGreen;
                gpuBox[a++].Position = verts[i + 4];
            }

        }

        public override void Draw(RenderEntry re)
        {
            re.Effect.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, markerGeometry, 0, 12);
        }
    }
}
