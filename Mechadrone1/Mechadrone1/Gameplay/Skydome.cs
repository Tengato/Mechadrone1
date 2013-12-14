using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Mechadrone1.Rendering;
using Microsoft.Xna.Framework;
using Manifracture;

namespace Mechadrone1.Gameplay
{
    class Skydome : GameObject
    {
        Effect effect;
        GraphicsDevice gd;
        DepthStencilState dsSky;
        EffectParameter wvpParam;


        public Skydome(TextureCube skyTexture, Model skyModel, Effect skyFx)
            : base(null as IGameManager)
        {
            VisualModel = skyModel;
            gd = skyTexture.GraphicsDevice;
            effect = skyFx;
            effect.Parameters["skybox"].SetValue(skyTexture);
            wvpParam = effect.Parameters["worldViewProj"];

            dsSky = new DepthStencilState();
            dsSky.DepthBufferFunction = CompareFunction.LessEqual;
        }


        public override List<Rendering.RenderEntry> GetRenderEntries(
            int batchId,
            RenderStep step,
            Matrix view,
            Matrix projection,
            Matrix cameraTransform,
            Matrix shadowCastingLightView,
            Matrix shadowCastingLightProjection,
            RenderTarget2D shadowMap,
            List<DirectLight> lights)
        {
            List<RenderEntry> results = new List<RenderEntry>();

            RenderEntry re = new RenderEntry(VisualModel.Meshes[0].MeshParts[0]);

            re.View = view;
            re.Projection = projection;
            re.ShadowCastingLightView = shadowCastingLightView;
            re.ShadowCastingLightProjection = shadowCastingLightProjection;
            re.ShadowMap = null;
            re.CameraTransform = cameraTransform;
            re.SceneObject = this;
            re.Lights = lights;

            re.DrawCallback = Draw;
            re.Effect = effect;
            re.Pass = effect.CurrentTechnique.Passes[0];

            re.SortOrder = 120;
            re.DepthStencilState = dsSky;
            re.RasterizerState = RasterizerState.CullNone;

            results.Add(re);

            return results;
        }


        public override void Draw(Rendering.RenderEntry re)
        {
            wvpParam.SetValue(Matrix.CreateTranslation(re.CameraTransform.Translation) * re.View * re.Projection);

            effect.CurrentTechnique.Passes[0].Apply();

            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, re.NumVertices, re.StartIndex, re.PrimitiveCount);
        }
    }
}
