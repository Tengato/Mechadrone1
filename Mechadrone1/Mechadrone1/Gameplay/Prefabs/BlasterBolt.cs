﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mechadrone1.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Manifracture;
using Skelemator;
using BEPUphysics.Entities;
using BEPUphysics.BroadPhaseEntries.Events;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using SlagformCommon;
using BEPUphysics.CollisionTests;

namespace Mechadrone1.Gameplay.Prefabs
{
    class BlasterBolt : SimulatedGameObject
    {
        public BlasterBolt(IGameManager owner)
            : base(owner)
        {
            CastsShadow = false;
        }


        public BlasterBolt(BlasterBolt a)
            : base(a)
        { }


        public override void Initialize()
        {
            base.Initialize();

            Entity soEnt = SimulationObject as Entity;

            soEnt.IsAffectedByGravity = false;
            soEnt.CollisionInformation.Events.InitialCollisionDetected += CollisionEventHandler;
            BEPUutilities.Vector3 initialImpulse = BepuConverter.Convert(Vector3.Transform(Vector3.Backward, Orientation) * 250.0f);
            soEnt.ApplyLinearImpulse(ref initialImpulse);
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
            world = Matrix.CreateScale(Scale) * Matrix.CreateFromQuaternion(Orientation * ModelAdjustment) * Matrix.CreateTranslation(Position);

            wvp = world * view * projection;
            wit = Matrix.Transpose(Matrix.Invert(world));

            List<RenderEntry> results = new List<RenderEntry>();

            foreach (ModelMesh mesh in VisualModel.Meshes)
            {
                foreach (ModelMeshPart mmp in mesh.MeshParts)
                {
                    RenderEntry re = new RenderEntry();

                    re.VertexBuffer = mmp.VertexBuffer;
                    re.NumVertices = mmp.NumVertices;
                    re.IndexBuffer = mmp.IndexBuffer;
                    re.VertexOffset = mmp.VertexOffset;
                    re.StartIndex = mmp.StartIndex;
                    re.RenderOptions = (RenderOptions)(mmp.Tag);
                    re.PrimitiveCount = mmp.PrimitiveCount;

                    re.View = view;
                    re.Projection = projection;
                    re.ShadowCastingLightView = shadowCastingLightView;
                    re.ShadowCastingLightProjection = shadowCastingLightProjection;
                    re.ShadowMap = shadowMap;
                    re.CameraTransform = cameraTransform;
                    re.SceneObject = this;
                    re.Lights = lights;

                    re.DrawCallback = Draw;
                    re.Effect = mmp.Effect;

                    re.BlendState = BlendState.Additive;
                    re.DepthStencilState = DepthStencilState.DepthRead;
                    re.RasterizerState = RasterizerState.CullNone;

                    re.SortOrder = 180;

                    re.Pass = re.Effect.CurrentTechnique.Passes[0];
                    results.Add(re);

                    // Make additional copies of the render entry for multi-pass techniques:
                    for (int i = 1; i < re.Effect.CurrentTechnique.Passes.Count; i++)
                    {
                        RenderEntry reCopy = new RenderEntry(re);
                        reCopy.Pass = re.Effect.CurrentTechnique.Passes[i];
                        results.Add(reCopy);
                    }
                }
            }

            return results;
        }


        public void CollisionEventHandler(EntityCollidable sender, Collidable other, CollidablePairHandler pair)
        {
            // TODO: deal damage, go poof...
            // TODO: will removing this from the sim now cause problems with the state of the simulation? It's possible this method gets called twice per collision? like if it hits a seam?
            game.DespawnList.Add(this);
            Entity soEnt = SimulationObject as Entity;
            soEnt.CollisionInformation.Events.InitialCollisionDetected -= CollisionEventHandler;
        }

    }
}
