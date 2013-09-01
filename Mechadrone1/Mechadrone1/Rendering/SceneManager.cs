using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Skelemator;
using Mechadrone1.Gameplay;
using Manifracture;
using System;
using System.Linq;
using BEPUphysics;


namespace Mechadrone1.Rendering
{

    /// <summary>
    /// Organizes all objects and processes that are necessary for rendering a 3D scene.
    /// It shouldn't know details about the game, it just wants to know what to draw.
    /// </summary>
    class SceneManager
    {
        GraphicsDevice gd;
        IRenderableScene sceneModel;
        QuadTree quadTree;

        SpriteBatch spriteBatch;
        SamplerState pointTexFilter;

        Matrix projection;
        BoundingFrustum cameraFrustum;

        const int NUM_LIGHTS_PER_EFFECT = 3;

        // Wireframe stuff
        int numGridlines = 11;
        VertexPositionColor[] gridGeometry;
        VertexPositionColor[] markerGeometry;
        BasicEffect wireframeEffect;

        // Shadow map stuff
        const int SMAP_SIZE = 2048;
        Effect depthOnlyFx;
        EffectParameter doWorldViewProj;
        Effect depthOnlySkinFx;
        EffectParameter dosWorldViewProj;
        EffectParameter dosWeightsPerVert;
        EffectParameter dosPosedBones;

        BoundingSphere sceneBounds;
        Matrix lightView;
        Matrix lightFrustum;
        Matrix ndcToTextureCoords = new Matrix(0.5f, 0.0f, 0.0f, 0.0f,
                                               0.0f, -0.5f, 0.0f, 0.0f,
                                               0.0f, 0.0f, 1.0f, 0.0f,
                                               0.5f, 0.5f, 0.0f, 1.0f);
        RenderTarget2D smapRenderTarget;

        // Default pose
        Matrix[] bindPose = new Matrix[72];

        // Effect/param collections:
        // Keyed by hashed effect and then by hashed param name

        Dictionary<Effect, Dictionary<string, EffectParameter>> effectParams;
        List<Effect> loadedEffects;

        // TODO: Should I put these someplace else?
        const string EYEPOSITION_PARAM_NAME = "EyePosition";
        const string WORLD_PARAM_NAME = "World";
        const string WORLDVIEWPROJ_PARAM_NAME = "WorldViewProj";
        const string WORLDINVTRANSPOSE_PARAM_NAME = "WorldInvTranspose";
        const string NUMLIGHTS_PARAM_NAME = "NumLights";
        const string FOGSTART_PARAM_NAME = "FogStart";
        const string FOGEND_PARAM_NAME = "FogEnd";
        const string FOGCOLOR_PARAM_NAME = "FogColor";
        const string POSEDBONES_PARAM_NAME = "PosedBones";
        const string INVSHADOWMAPSIZE_PARAM_NAME = "InvShadowMapSize";
        const string SHADOWLIGHTINDEX_PARAM_NAME = "ShadowLightIndex";
        const string SHADOWTRANSFORM_PARAM_NAME = "ShadowTransform";
        const string SHADOWMAP_PARAM_NAME = "ShadowMap";
        const string ENVIROMAP_PARAM_NAME = "EnviroMap";

        string[] standardParamNames = new string[8] {
            EYEPOSITION_PARAM_NAME,
            WORLD_PARAM_NAME,
            WORLDVIEWPROJ_PARAM_NAME,
            WORLDINVTRANSPOSE_PARAM_NAME,
            NUMLIGHTS_PARAM_NAME,
            FOGSTART_PARAM_NAME,
            FOGEND_PARAM_NAME,
            FOGCOLOR_PARAM_NAME,
        };

        Dictionary<string, string[]> standardStructParamNames;

        const string DIRLIGHT_STRUCT_NAME = "DirLights";
        const string AMBIENT_PARAM_NAME = "Ambient";
        const string DIFFUSE_PARAM_NAME = "Diffuse";
        const string SPECULAR_PARAM_NAME = "Specular";
        const string DIRECTION_PARAM_NAME = "Direction";
        const string ENERGY_PARAM_NAME = "Energy";

        string[] dirLightStructParamNames = new string[5] {
            AMBIENT_PARAM_NAME,
            DIFFUSE_PARAM_NAME,
            SPECULAR_PARAM_NAME,
            DIRECTION_PARAM_NAME,
            ENERGY_PARAM_NAME,
        };

        public bool GridEnabled { get; set; }


        public SceneManager(GraphicsDevice graphicsDevice)
        {
            gd = graphicsDevice;
        }


        public void LoadContent(IRenderableScene scene, ContentManager content)
        {
            sceneModel = scene;
            quadTree = new QuadTree(scene.WorldBounds);

            cameraFrustum = new BoundingFrustum(Matrix.Identity);

            projection = Matrix.CreatePerspectiveFieldOfView(sceneModel.FieldOfView, gd.Viewport.AspectRatio, 0.1f, 1000.0f);

            spriteBatch = new SpriteBatch(gd);
            pointTexFilter = new SamplerState();
            pointTexFilter.Filter = TextureFilter.Point;

            gridGeometry = new VertexPositionColor[numGridlines * 4 + 6];
            markerGeometry = new VertexPositionColor[24];

            int gridRadius = (numGridlines - 1) / 2;

            for (int i = 0; i < numGridlines; i++)
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

            gridGeometry[numGridlines * 4].Position = Vector3.Zero;
            gridGeometry[numGridlines * 4].Color = Color.Red;
            gridGeometry[numGridlines * 4 + 1].Position = Vector3.Right;
            gridGeometry[numGridlines * 4 + 1].Color = Color.Red;
            gridGeometry[numGridlines * 4 + 2].Position = Vector3.Zero;
            gridGeometry[numGridlines * 4 + 2].Color = Color.Green;
            gridGeometry[numGridlines * 4 + 3].Position = Vector3.Up;
            gridGeometry[numGridlines * 4 + 3].Color = Color.Green;
            gridGeometry[numGridlines * 4 + 4].Position = Vector3.Zero;
            gridGeometry[numGridlines * 4 + 4].Color = Color.Blue;
            gridGeometry[numGridlines * 4 + 5].Position = Vector3.Backward;
            gridGeometry[numGridlines * 4 + 5].Color = Color.Blue;

            for (int i = 0; i < bindPose.Length; i++)
            {
                bindPose[i] = Matrix.Identity;
            }

            standardStructParamNames = new Dictionary<string, string[]>();
            standardStructParamNames.Add((DIRLIGHT_STRUCT_NAME), dirLightStructParamNames);

            wireframeEffect = new BasicEffect(gd);

            depthOnlySkinFx = content.Load<Effect>("shaders\\DepthOnlySkin");
            depthOnlyFx = content.Load<Effect>("shaders\\DepthOnly");

            doWorldViewProj = depthOnlyFx.Parameters["WorldViewProj"];
            dosWorldViewProj = depthOnlySkinFx.Parameters["WorldViewProj"];
            dosWeightsPerVert = depthOnlySkinFx.Parameters["WeightsPerVert"];
            dosPosedBones = depthOnlySkinFx.Parameters["PosedBones"];

            smapRenderTarget = new RenderTarget2D(gd,
                                                  SMAP_SIZE,
                                                  SMAP_SIZE,
                                                  false,
                                                  SurfaceFormat.Single,
                                                  DepthFormat.Depth24);

            // Build the effect/param collections.
            loadedEffects = new List<Effect>();
            effectParams = new Dictionary<Effect, Dictionary<string, EffectParameter>>();

            RegisterEffectParams(sceneModel.Substrate.Effect, (RenderOptions)(sceneModel.Substrate.Tag));

            foreach (GameObject go in sceneModel.GameObjects.FindAll(obj => obj.VisualModel != null))
            {
                foreach (ModelMesh mm in go.VisualModel.Meshes)
                {
                    foreach (ModelMeshPart mmp in mm.MeshParts)
                    {
                        RegisterEffectParams(mmp.Effect, (RenderOptions)(mmp.Tag));
                    }
                }

                go.JoinQuadTree(quadTree);
            }

            SetFog(sceneModel.Fog);
        }


        private void RegisterEffectParams(Effect fx, RenderOptions options)
        {
            Dictionary<string, EffectParameter> standardParams = new Dictionary<string, EffectParameter>();

            loadedEffects.Add(fx);

            for (int i = 0; i < standardParamNames.Length; i++)
            {
                standardParams.Add(standardParamNames[i], fx.Parameters[standardParamNames[i]]);
            }

            foreach (KeyValuePair<string, string[]> kvp in standardStructParamNames)
            {
                // Dirlight is special because it's an array.
                if (kvp.Key == DIRLIGHT_STRUCT_NAME)
                {
                    for (int i = 0; i < NUM_LIGHTS_PER_EFFECT; i++)
                    {
                        for (int j = 0; j < kvp.Value.Length; j++)
                        {
                            // Stick a digit on the end of the param name to keep it unique when we flatten it.
                            string lightParamName = DIRLIGHT_STRUCT_NAME + kvp.Value[j] + i.ToString();
                            standardParams.Add(lightParamName, fx.Parameters[kvp.Key].Elements[i].StructureMembers[kvp.Value[j]]);
                        }
                    }
                }
                else
                {
                    for (int j = 0; j < kvp.Value.Length; j++)
                    {
                        standardParams.Add(kvp.Value[j], fx.Parameters[kvp.Value[j]]);
                    }
                }
            }

            if (options.HasFlag(RenderOptions.RequiresSkeletalPose))
            {
                standardParams.Add(POSEDBONES_PARAM_NAME, fx.Parameters[POSEDBONES_PARAM_NAME]);
            }

            if (options.HasFlag(RenderOptions.RequiresShadowMap))
            {
                fx.Parameters[INVSHADOWMAPSIZE_PARAM_NAME].SetValue(1.0f / (float)SMAP_SIZE);
                fx.Parameters[SHADOWLIGHTINDEX_PARAM_NAME].SetValue(0);
                standardParams.Add(SHADOWTRANSFORM_PARAM_NAME, fx.Parameters[SHADOWTRANSFORM_PARAM_NAME]);
                standardParams.Add(SHADOWMAP_PARAM_NAME, fx.Parameters[SHADOWMAP_PARAM_NAME]);
            }

            effectParams.Add(fx, standardParams);
        }


        private void SetFog(FogDesc fogInfo)
        {
            foreach (Effect fx in loadedEffects)
            {
                Dictionary<string, EffectParameter> fxeps = effectParams[fx];

                fxeps[FOGSTART_PARAM_NAME].SetValue(fogInfo.StartDistance);
                fxeps[FOGEND_PARAM_NAME].SetValue(fogInfo.EndDistance);
                fxeps[FOGCOLOR_PARAM_NAME].SetValue(fogInfo.Color.ToVector4());
            }
        }


        private void ConvertToGPUBox(Vector3[] verts, ref VertexPositionColor[] gpuBox)
        {
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


        /// <summary>
        /// Draw the 3D game scene
        /// </summary>
        public void DrawScene(PlayerIndex player)
        {
            ICamera camera = sceneModel.GetCamera(player);

            cameraFrustum.Matrix = camera.View * projection;
            BoundingBox cameraRect = BoundingBox.CreateFromPoints(cameraFrustum.GetCorners());

            List<GameObject> visibleObjects = quadTree.Search(cameraRect, cameraFrustum);

            // TODO: Make some refactorizations to declutter this method.
            // Build the shadow map. The main light will cast the shadow.

            RayCastResult rcr;
            Ray cameraRay;

            cameraRay.Direction = Vector3.Down;//Vector3.Lerp(, camera.Transform.Down, 0.15f);
            cameraRay.Position = camera.Transform.Translation;

            if (sceneModel.SimSpace.RayCast(cameraRay, 90.0f, out rcr))
                sceneBounds.Center = rcr.HitData.Location;
            else
                sceneBounds.Center = cameraRay.Position + Vector3.Normalize(cameraRay.Direction) * 90.0f;

            sceneBounds.Center += new Vector3(camera.Transform.Forward.X, 0.0f, camera.Transform.Forward.Z) * 50.0f;
            // sceneBounds.Radius = (sceneBounds.Center - cameraRay.Position).Length();
            sceneBounds.Radius = 50.0f;

            float texelsPerWorldUnit = (float)SMAP_SIZE / (sceneBounds.Radius * 2.0f);

            Matrix lightViewAtOrigin = Matrix.CreateScale(texelsPerWorldUnit) *
                Matrix.CreateLookAt(Vector3.Zero, -sceneModel.ShadowCastingLight.Direction, Vector3.Up);
            Matrix lightViewAtOriginInv = Matrix.Invert(lightViewAtOrigin);

            sceneBounds.Center = Vector3.Transform(sceneBounds.Center, lightViewAtOrigin);

            sceneBounds.Center.X = (float)(Math.Floor(sceneBounds.Center.X));
            sceneBounds.Center.Y = (float)(Math.Floor(sceneBounds.Center.Y));

            sceneBounds.Center = Vector3.Transform(sceneBounds.Center, lightViewAtOriginInv);

            BoundingBox sbCenterMarker;
            sbCenterMarker.Min = sceneBounds.Center - new Vector3(0.3f, 0.3f, 0.3f);
            sbCenterMarker.Max = sceneBounds.Center + new Vector3(0.3f, 0.3f, 0.3f);
            ConvertToGPUBox(sbCenterMarker.GetCorners(), ref markerGeometry);

            lightView = Matrix.CreateLookAt(sceneBounds.Center - sceneModel.ShadowCastingLight.Direction * sceneBounds.Radius * 2.0f, sceneBounds.Center, Vector3.Up);
            lightFrustum = Matrix.CreateOrthographic(sceneBounds.Radius * 2.0f,
                                                     sceneBounds.Radius * 2.0f,
                                                     0.0f,
                                                     sceneBounds.Radius * 6.0f);

            gd.SetRenderTarget(smapRenderTarget);
            gd.Clear(Color.White);
            gd.BlendState = BlendState.Opaque;
            gd.DepthStencilState = DepthStencilState.Default;
            gd.RasterizerState = RasterizerState.CullCounterClockwise;

            Matrix world;
            Matrix wvp;
            Matrix wit;

            // Draw objects that cast shadows:
            foreach (GameObject go in visibleObjects.FindAll(obj => obj.CastsShadow == true))
            {
                world = Matrix.CreateScale(go.Scale) * Matrix.CreateFromQuaternion(go.Orientation) * Matrix.CreateTranslation(go.Position);
                wvp = world * lightView * lightFrustum;
                doWorldViewProj.SetValue(wvp);
                dosWorldViewProj.SetValue(wvp);

                Matrix[] bones;

                if (go.Animations != null)
                {
                    if (go.AnimationPlayer != null && go.AnimationPlayer.CurrentClip != null)
                    {
                        bones = go.AnimationPlayer.GetSkinTransforms();
                    }
                    else
                    {
                        bones = bindPose;
                    }

                    dosWeightsPerVert.SetValue(go.Animations.WeightsPerVert);
                    dosPosedBones.SetValue(bones);
                }

                foreach (ModelMesh mesh in go.VisualModel.Meshes)
                {
                    foreach (ModelMeshPart mmp in mesh.MeshParts)
                    {

                        gd.SetVertexBuffer(mmp.VertexBuffer);
                        gd.Indices = mmp.IndexBuffer;

                        // TODO: let's put these fx into separate techniques instead maybe?
                        Effect fxToUse;
                        if (((RenderOptions)(mmp.Tag)).HasFlag(RenderOptions.RequiresSkeletalPose) && go.Animations != null)
                        {
                            fxToUse = depthOnlySkinFx;
                        }
                        else
                        {
                            fxToUse = depthOnlyFx;
                        }

                        for (int i = 0; i < fxToUse.CurrentTechnique.Passes.Count; i++)
                        {
                            fxToUse.CurrentTechnique.Passes[i].Apply();

                            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, mmp.VertexOffset, 0,
                                mmp.NumVertices, mmp.StartIndex, mmp.PrimitiveCount);
                        }
                    }
                }
            }

            // TODO: Add optimizations like sort by material, z-prepass etc
            // Change state to draw camera view and draw objects:
            gd.SetRenderTarget(null);

            gd.Clear(Color.CornflowerBlue);
            gd.BlendState = BlendState.Opaque;
            gd.DepthStencilState = DepthStencilState.Default;
            gd.RasterizerState = RasterizerState.CullCounterClockwise;

            // Draw terrain:
            world = Matrix.CreateTranslation(sceneModel.Substrate.Position);
            wvp = world * camera.View * projection;
            wit = Matrix.Transpose(Matrix.Invert(world));

            AddLightingToEffect(sceneModel.TerrainLights, sceneModel.Substrate.Effect);

            // TODO: Does this belong here?
            effectParams[sceneModel.Substrate.Effect][SHADOWTRANSFORM_PARAM_NAME].SetValue(world * lightView *
                lightFrustum * ndcToTextureCoords);
            effectParams[sceneModel.Substrate.Effect][SHADOWMAP_PARAM_NAME].SetValue(smapRenderTarget);
            effectParams[sceneModel.Substrate.Effect][WORLD_PARAM_NAME].SetValue(world);
            effectParams[sceneModel.Substrate.Effect][WORLDVIEWPROJ_PARAM_NAME].SetValue(wvp);
            effectParams[sceneModel.Substrate.Effect][WORLDINVTRANSPOSE_PARAM_NAME].SetValue(wit);
            effectParams[sceneModel.Substrate.Effect][EYEPOSITION_PARAM_NAME].SetValue(camera.Transform.Translation);

            gd.SetVertexBuffer(sceneModel.Substrate.Vertices);
            gd.Indices = sceneModel.Substrate.Indices;

            for (int i = 0; i < sceneModel.Substrate.Effect.CurrentTechnique.Passes.Count; i++)
            {
                sceneModel.Substrate.Effect.CurrentTechnique.Passes[i].Apply();

                int numXSectors = (sceneModel.Substrate.VertexCountAlongXAxis - 1) / (sceneModel.Substrate.SectorSize - 1);
                int numZSectors = (sceneModel.Substrate.VertexCountAlongZAxis - 1) / (sceneModel.Substrate.SectorSize - 1);

                int numVertices = (sceneModel.Substrate.SectorSize + 1) * (sceneModel.Substrate.SectorSize + 1);
                int primitiveCount = 2 * sceneModel.Substrate.SectorSize * sceneModel.Substrate.SectorSize;

                for (int j = 0; j < numXSectors; j++)
                    for (int k = 0; k < numZSectors; k++)
                    {
                        int vertOffset = j * sceneModel.Substrate.SectorSize +
                            k * sceneModel.Substrate.SectorSize * sceneModel.Substrate.VertexCountAlongXAxis;
                        gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, vertOffset, 0, numVertices, 0, primitiveCount);
                    }
            }

            // Draw objects:
            foreach (GameObject go in visibleObjects)
            {
                world = Matrix.CreateScale(go.Scale) * Matrix.CreateFromQuaternion(go.Orientation) * Matrix.CreateTranslation(go.Position);
                wvp = world * camera.View * projection;
                wit = Matrix.Transpose(Matrix.Invert(world));


                Matrix[] bones = bindPose;

                if (go.Animations != null && go.AnimationPlayer != null && go.AnimationPlayer.CurrentClip != null)
                {
                    bones = go.AnimationPlayer.GetSkinTransforms();
                }

                foreach (ModelMesh mesh in go.VisualModel.Meshes)
                {
                    AddLightingToObject(sceneModel.GetObjectLights(mesh, world, camera.Transform.Translation), mesh);

                    foreach (ModelMeshPart mmp in mesh.MeshParts)
                    {
                        if (((RenderOptions)(mmp.Tag)).HasFlag(RenderOptions.RequiresSkeletalPose))
                        {
                            effectParams[mmp.Effect][POSEDBONES_PARAM_NAME].SetValue(bones);
                        }

                        if (((RenderOptions)(mmp.Tag)).HasFlag(RenderOptions.RequiresShadowMap))
                        {
                            effectParams[mmp.Effect][SHADOWTRANSFORM_PARAM_NAME].SetValue(world * lightView * lightFrustum * ndcToTextureCoords);
                            effectParams[mmp.Effect][SHADOWMAP_PARAM_NAME].SetValue(smapRenderTarget);
                        }

                        effectParams[mmp.Effect][WORLD_PARAM_NAME].SetValue(world);
                        effectParams[mmp.Effect][WORLDVIEWPROJ_PARAM_NAME].SetValue(wvp);
                        effectParams[mmp.Effect][WORLDINVTRANSPOSE_PARAM_NAME].SetValue(wit);
                        effectParams[mmp.Effect][EYEPOSITION_PARAM_NAME].SetValue(camera.Transform.Translation);

                        gd.SetVertexBuffer(mmp.VertexBuffer);
                        gd.Indices = mmp.IndexBuffer;

                        for (int i = 0; i < mmp.Effect.CurrentTechnique.Passes.Count; i++)
                        {
                            mmp.Effect.CurrentTechnique.Passes[i].Apply();

                            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, mmp.VertexOffset, 0,
                                mmp.NumVertices, mmp.StartIndex, mmp.PrimitiveCount);

                        }
                    }
                }
            }


            // Draw grid
            gd.DepthStencilState = DepthStencilState.None;
            wireframeEffect.World = Matrix.Identity;
            wireframeEffect.View = camera.View;
            wireframeEffect.Projection = projection;
            wireframeEffect.VertexColorEnabled = true;

            foreach (EffectPass ep in wireframeEffect.CurrentTechnique.Passes)
            {
                ep.Apply();

                gd.DrawUserPrimitives(PrimitiveType.LineList, gridGeometry, gridGeometry.Length - 6, 3);
                gd.DrawUserPrimitives(PrimitiveType.LineList, markerGeometry, 0, 12);
            }


            //if (showSmap)
            //{
            //    gd.SamplerStates[0] = pointTexFilter;
            //    spriteBatch.Begin(0, BlendState.Opaque, SamplerState.PointClamp, null, null);
            //    spriteBatch.Draw(smapRenderTarget, new Rectangle(120, 0, 640, 640), Color.White);
            //    spriteBatch.End();
            //    return;
            //}

            //gd.Textures[0] = null;
            //gd.SamplerStates[0] = SamplerState.LinearWrap;

        }


        private void AddLightingToObject(List<DirectLight> lights, ModelMesh mesh)
        {
            foreach (ModelMeshPart mmp in mesh.MeshParts)
            {
                AddLightingToEffect(lights, mmp.Effect);
            }
        }


        private void AddLightingToEffect(List<DirectLight> lights, Effect fx)
        {
            Dictionary<string, EffectParameter> terrainEPs = effectParams[fx];
            terrainEPs[NUMLIGHTS_PARAM_NAME].SetValue(lights.Count);
            for (int i = 0; i < lights.Count; i++)
            {
                terrainEPs[DIRLIGHT_STRUCT_NAME + AMBIENT_PARAM_NAME + i.ToString()].SetValue(lights[i].Ambient);
                terrainEPs[DIRLIGHT_STRUCT_NAME + DIFFUSE_PARAM_NAME + i.ToString()].SetValue(lights[i].Diffuse);
                terrainEPs[DIRLIGHT_STRUCT_NAME + SPECULAR_PARAM_NAME + i.ToString()].SetValue(lights[i].Specular);
                terrainEPs[DIRLIGHT_STRUCT_NAME + DIRECTION_PARAM_NAME + i.ToString()].SetValue(lights[i].Direction);
                terrainEPs[DIRLIGHT_STRUCT_NAME + ENERGY_PARAM_NAME + i.ToString()].SetValue(lights[i].Energy);
            }
        }

    }


    public enum DrawMode
    {
        Alpha = 0,
        Additive = 1,
        AlphaAndGlow = 2,
        AdditiveAndGlow = 3,
    }

}
