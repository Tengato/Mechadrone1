using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Mechadrone1.Rendering;
using Skelemator;

namespace Mechadrone1.Gameplay.Prefabs
{
    class Grid : GameObject
    {

        // Wireframe stuff
        const int NUM_GRIDLINES = 11;
        VertexPositionColor[] gridGeometry;
        GraphicsDevice gd;
        BasicEffect effect;

        public Grid(GraphicsDevice device)
            : base(null as IGameManager)
        {
            gd = device;

            effect = new BasicEffect(gd);
            effect.VertexColorEnabled = true;

            gridGeometry = new VertexPositionColor[NUM_GRIDLINES * 4];

            int gridRadius = (NUM_GRIDLINES - 1) / 2;

            for (int i = 0; i < NUM_GRIDLINES; i++)
            {
                gridGeometry[4 * i].Position = new Vector3(-gridRadius, 0.0f, -gridRadius + i);
                gridGeometry[4 * i].Color = Color.LightGray;
                gridGeometry[4 * i + 1].Position = new Vector3(gridRadius, 0.0f, -gridRadius + i);
                gridGeometry[4 * i + 1].Color = Color.LightGray;
                gridGeometry[4 * i + 2].Position = new Vector3(-gridRadius + i, 0.0f, -gridRadius);
                gridGeometry[4 * i + 2].Color = Color.LightGray;
                gridGeometry[4 * i + 3].Position = new Vector3(-gridRadius + i, 0.0f, gridRadius);
                gridGeometry[4 * i + 3].Color = Color.LightGray;
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

            gd.DrawUserPrimitives(PrimitiveType.LineList, gridGeometry, 0, NUM_GRIDLINES * 2);
        }
    }
}
