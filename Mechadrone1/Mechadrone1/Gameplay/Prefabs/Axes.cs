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
    class Axes : GameObject
    {

        // Wireframe stuff
        VertexPositionColor[] axesGeometry;
        GraphicsDevice gd;
        BasicEffect effect;

        public Axes(GraphicsDevice device)
            : base(null as IGameManager)
        {
            gd = device;

            effect = new BasicEffect(gd);
            effect.VertexColorEnabled = true;

            axesGeometry = new VertexPositionColor[6];

            axesGeometry[0].Position = Vector3.Zero;
            axesGeometry[0].Color = Color.Red;
            axesGeometry[1].Position = Vector3.Right;
            axesGeometry[1].Color = Color.Red;
            axesGeometry[2].Position = Vector3.Zero;
            axesGeometry[2].Color = Color.Green;
            axesGeometry[3].Position = Vector3.Up;
            axesGeometry[3].Color = Color.Green;
            axesGeometry[4].Position = Vector3.Zero;
            axesGeometry[4].Color = Color.Blue;
            axesGeometry[5].Position = Vector3.Backward;
            axesGeometry[5].Color = Color.Blue;

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

            re.DepthStencilState = DepthStencilState.None;
            re.SortOrder = 4000;

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

            gd.DrawUserPrimitives(PrimitiveType.LineList, axesGeometry, 0, 3);
        }
    }
}
