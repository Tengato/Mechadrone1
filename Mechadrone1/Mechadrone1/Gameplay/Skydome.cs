using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Mechadrone1.Rendering;
using Microsoft.Xna.Framework;

namespace Mechadrone1.Gameplay
{
    class Skydome : GameObject
    {
        Effect effect;
        GraphicsDevice gd;
        DepthStencilState dsSky;
        EffectParameter wvp;

        public Skydome(TextureCube skyTexture, Model skyModel, Effect skyFx)
        {
            VisualModel = skyModel;
            gd = skyTexture.GraphicsDevice;
            effect = skyFx;
            effect.Parameters["skybox"].SetValue(skyTexture);
            wvp = effect.Parameters["worldViewProj"];

            dsSky = new DepthStencilState();
            dsSky.DepthBufferFunction = CompareFunction.LessEqual;
        }


        public override List<Rendering.RenderEntry> GetRenderEntries(int batchId, Rendering.RenderStep step, Microsoft.Xna.Framework.Matrix view, Microsoft.Xna.Framework.Matrix projection, Microsoft.Xna.Framework.Matrix cameraTransform, Microsoft.Xna.Framework.Matrix shadowCastingLightView, Microsoft.Xna.Framework.Matrix shadowCastingLightProjection, RenderTarget2D shadowMap, List<Manifracture.DirectLight> lights)
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
            wvp.SetValue(Matrix.CreateTranslation(re.CameraTransform.Translation) * re.View * re.Projection);

            effect.CurrentTechnique.Passes[0].Apply();

            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, re.NumVertices, re.StartIndex, re.PrimitiveCount);
        }
    }
}
