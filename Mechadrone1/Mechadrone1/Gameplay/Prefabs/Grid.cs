using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Mechadrone1.Rendering;

namespace Mechadrone1.Gameplay.Prefabs
{
    class Grid : GameObject
    {

        // Wireframe stuff
        const int NUM_GRIDLINES = 11;
        VertexPositionColor[] gridGeometry;
        BasicEffect wireframeEffect;

        public bool ShowGrid { get; set; }
        public bool ShowAxes { get; set; }

        public Grid(BasicEffect basicEffect) : base(null)
        {
            wireframeEffect = basicEffect;
            wireframeEffect.VertexColorEnabled = true;

            gridGeometry = new VertexPositionColor[NUM_GRIDLINES * 4 + 6];

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

            gridGeometry[NUM_GRIDLINES * 4].Position = Vector3.Zero;
            gridGeometry[NUM_GRIDLINES * 4].Color = Color.Red;
            gridGeometry[NUM_GRIDLINES * 4 + 1].Position = Vector3.Right;
            gridGeometry[NUM_GRIDLINES * 4 + 1].Color = Color.Red;
            gridGeometry[NUM_GRIDLINES * 4 + 2].Position = Vector3.Zero;
            gridGeometry[NUM_GRIDLINES * 4 + 2].Color = Color.Green;
            gridGeometry[NUM_GRIDLINES * 4 + 3].Position = Vector3.Up;
            gridGeometry[NUM_GRIDLINES * 4 + 3].Color = Color.Green;
            gridGeometry[NUM_GRIDLINES * 4 + 4].Position = Vector3.Zero;
            gridGeometry[NUM_GRIDLINES * 4 + 4].Color = Color.Blue;
            gridGeometry[NUM_GRIDLINES * 4 + 5].Position = Vector3.Backward;
            gridGeometry[NUM_GRIDLINES * 4 + 5].Color = Color.Blue;

        }


        public override List<RenderEntry> GetRenderEntries(
            int frame,
            RenderStep step,
            Matrix view,
            Matrix projection,
            Matrix cameraTransform,
            Matrix shadowCastingLightView,
            Matrix shadowCastingLightFrustum,
            RenderTarget2D shadowMap,
            List<Manifracture.DirectLight> lights)
        {
            List<RenderEntry> results = new List<RenderEntry>();

            RenderEntry re = new RenderEntry();

            // Draw grid
            re.DepthStencilState = DepthStencilState.None;
            re.Effect = wireframeEffect;
            //re.World = Matrix.Identity;
            re.View = view;
            re.Projection = projection;

            throw new NotImplementedException();
        }


        public override void Draw(RenderEntry re)
        {
            re.Effect.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, gridGeometry, gridGeometry.Length - 6, 3);
        }
    }
}
