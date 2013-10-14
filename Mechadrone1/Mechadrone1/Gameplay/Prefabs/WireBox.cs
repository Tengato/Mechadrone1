using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mechadrone1.Rendering;
using Skelemator;

namespace Mechadrone1.Gameplay.Prefabs
{
    class WireBox : GameObject
    {
        VertexPositionColor[] markerGeometry;
        GraphicsDevice gd;
        BasicEffect effect;

        public WireBox(GraphicsDevice device, float edgeLength) : base(null)
        {
            gd = device;

            effect = new BasicEffect(gd);
            effect.VertexColorEnabled = true;

            markerGeometry = new VertexPositionColor[24];

            Vector3 roughBoxRadius = Vector3.One * edgeLength / 2.0f;
            BoundingBox roughBox = new BoundingBox(-roughBoxRadius, roughBoxRadius);

            Vector3[] verts = roughBox.GetCorners();

            int a = 0;
            for (int i = 0; i < 4; i++)
            {
                // Draw one line per iteration (2 verts):
                markerGeometry[a].Color = i == 0 ? Color.HotPink : Color.Cyan;
                markerGeometry[a++].Position = verts[i];
                markerGeometry[a].Color = i == 0 ? Color.HotPink : Color.Cyan;
                int n = (i + 1) % 4;
                markerGeometry[a++].Position = verts[n];
            }
            for (int i = 0; i < 4; i++)
            {
                markerGeometry[a].Color = i == 0 ? Color.HotPink : Color.Yellow;
                markerGeometry[a++].Position = verts[i + 4];
                markerGeometry[a].Color = i == 0 ? Color.HotPink : Color.Yellow;
                int n = (i + 1) % 4;
                markerGeometry[a++].Position = verts[n + 4];
            }
            for (int i = 0; i < 4; i++)
            {
                markerGeometry[a].Color = Color.LimeGreen;
                markerGeometry[a++].Position = verts[i];
                markerGeometry[a].Color = Color.LimeGreen;
                markerGeometry[a++].Position = verts[i + 4];
            }
        }

        public override List<RenderEntry> GetRenderEntries(
            int batchId,
            RenderStep step,
            Matrix view,
            Matrix projection,
            Matrix cameraTransform,
            Matrix shadowCastingLightView,
            Matrix shadowCastingLightProjection,
            RenderTarget2D shadowMap,
            List<Manifracture.DirectLight> lights)
        {

            world = Matrix.CreateScale(Scale) * Matrix.CreateFromQuaternion(Orientation) * Matrix.CreateTranslation(Position);

            List<RenderEntry> results = new List<RenderEntry>();
            RenderEntry re = new RenderEntry();

            re.VertexBuffer = null;
            re.NumVertices = 24;
            re.IndexBuffer = null;
            re.VertexOffset = 0;
            re.StartIndex = 0;
            re.RenderOptions = RenderOptions.None;
            re.PrimitiveCount = 12;

            re.View = view;
            re.Projection = projection;
            re.ShadowCastingLightView = shadowCastingLightView;
            re.ShadowCastingLightProjection = shadowCastingLightProjection;
            re.ShadowMap = shadowMap;
            re.CameraTransform = cameraTransform;
            re.SceneObject = this;
            re.Lights = lights;

            re.DrawCallback = Draw;
            re.Effect = effect;
            re.Pass = effect.CurrentTechnique.Passes[0];

            results.Add(re);

            return results;

        }

        public override void Draw(RenderEntry re)
        {
            effect.World = world;
            effect.View = re.View;
            effect.Projection = re.Projection;

            effect.CurrentTechnique.Passes[0].Apply();

            gd.DrawUserPrimitives(PrimitiveType.LineList, markerGeometry, 0, 12);
        }
    }
}
