using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Skelemator;

namespace Mechadrone1
{
    static class EffectRegistry
    {
        private const int NUM_LIGHTS_PER_EFFECT = 3;
        // Param names
        public const string EYEPOSITION_PARAM_NAME = "gEyePosition";
        public const string WORLD_PARAM_NAME = "gWorld";
        public const string VIEWPROJ_PARAM_NAME = "gViewProj";
        public const string WORLDVIEWPROJ_PARAM_NAME = "gWorldViewProj";
        public const string WORLDINVTRANSPOSE_PARAM_NAME = "gWorldInvTranspose";
        public const string PROJ_XSCALE_PARAM_NAME = "gProjXScale";
        public const string VIEWPORT_SCALE_PARAM_NAME = "ViewportScale";
        public const string SECONDS_TIMER_VALUE_PARAM_NAME = "CurrentTime";
        public const string NUMLIGHTS_PARAM_NAME = "NumLights";
        public const string FOGSTART_PARAM_NAME = "gFogStart";
        public const string FOGEND_PARAM_NAME = "gFogEnd";
        public const string FOGCOLOR_PARAM_NAME = "gFogColor";
        public const string POSEDBONES_PARAM_NAME = "gPosedBones";
        public const string WEIGHTS_PER_VERT_PARAM_NAME = "gWeightsPerVert";
        public const string INVSHADOWMAPSIZE_PARAM_NAME = "gInvShadowMapSize";
        public const string SHADOWLIGHTINDEX_PARAM_NAME = "ShadowLightIndex";
        public const string SHADOWTRANSFORM_PARAM_NAME = "gShadowTransform";
        public const string SHADOWMAP_PARAM_NAME = "gShadowMap";
        public const string ENVIROMAP_PARAM_NAME = "gEnvironmentMap";
        public const string FRINGEMAP_PARAM_NAME = "gFringeMap";
        public const string IRRADIANCEMAP_PARAM_NAME = "gIrradianceMap";
        public const string SPECPREFILTER_PARAM_NAME = "gSpecPrefilter";
        public const string SPECEXPFACTOR_PARAM_NAME = "gSpecExpFactor";
        public const string NUMSPECLEVELS_PARAM_NAME = "gNumSpecLevels";
        public const string AMBIENTLIGHT_PARAM_NAME = "gAmbientLight";
        public const string TEXTURE_PARAM_NAME = "Texture";
        public const string MATERIAL_SPECULAR_COLOR_PARAM_NAME = "gMatSpecColor";
        public const string BRIGHTNESS_PARAM_NAME = "gBright";
        public const string CONTRAST_PARAM_NAME = "gContrast";
        public const string ALPHA_TEST_DIRECTION_PARAM_NAME = "AlphaTestDirection";

        // Struct param names
        public const string DIRLIGHT_STRUCT_NAME = "DirLights";
        public const string AMBIENT_PARAM_NAME = "Ambient";
        public const string DIFFUSE_PARAM_NAME = "Diffuse";
        public const string SPECULAR_PARAM_NAME = "Specular";
        public const string DIRECTION_PARAM_NAME = "Direction";
        public const string ENERGY_PARAM_NAME = "Energy";

        public static Dictionary<Effect, Dictionary<string, EffectParameter>> Params;
        // The parameters that must be present in all standard effect files.
        private readonly static string[] sStandardParamNames;
        // The standard parameters that are organized into structs.
        private readonly static Dictionary<string, string[]> sStandardStructParamNames;
        // The param names that make up the standard DirLight struct.
        private readonly static string[] sDirLightStructParamNames;

        // Effects that are not loaded with models and thus are maintained by this class.
        private static Effect sDepthOnlyFx;
        public static Effect DepthOnlyFx
        {
            get
            {
                return sDepthOnlyFx;
            }
            set
            {
                if (sDepthOnlyFx != null && Params.ContainsKey(sDepthOnlyFx))
                    Params.Remove(sDepthOnlyFx);

                sDepthOnlyFx = value;
                Dictionary<string, EffectParameter> extractedParams = new Dictionary<string, EffectParameter>();
                extractedParams.Add(WORLDVIEWPROJ_PARAM_NAME, sDepthOnlyFx.Parameters[WORLDVIEWPROJ_PARAM_NAME]);
                Params.Add(sDepthOnlyFx, extractedParams);
            }
        }

        private static Effect sDepthOnlySkinFx;
        public static Effect DepthOnlySkinFx
        {
            get
            {
                return sDepthOnlySkinFx;
            }
            set
            {
                if (sDepthOnlySkinFx != null && Params.ContainsKey(sDepthOnlySkinFx))
                    Params.Remove(sDepthOnlySkinFx);

                sDepthOnlySkinFx = value;
                Dictionary<string, EffectParameter> extractedParams = new Dictionary<string, EffectParameter>();
                extractedParams.Add(WORLDVIEWPROJ_PARAM_NAME, sDepthOnlySkinFx.Parameters[WORLDVIEWPROJ_PARAM_NAME]);
                extractedParams.Add(WEIGHTS_PER_VERT_PARAM_NAME, sDepthOnlySkinFx.Parameters[WEIGHTS_PER_VERT_PARAM_NAME]);
                extractedParams.Add(POSEDBONES_PARAM_NAME, sDepthOnlySkinFx.Parameters[POSEDBONES_PARAM_NAME]);
                Params.Add(sDepthOnlySkinFx, extractedParams);
            }
        }

        private static Effect sSkyboxFx;
        public static Effect SkyboxFx
        {
            get
            {
                return sSkyboxFx;
            }
            set
            {
                if (sSkyboxFx != null && Params.ContainsKey(sSkyboxFx))
                    Params.Remove(sSkyboxFx);

                sSkyboxFx = value;
                Dictionary<string, EffectParameter> extractedParams = new Dictionary<string, EffectParameter>();
                extractedParams.Add(WORLDVIEWPROJ_PARAM_NAME, sSkyboxFx.Parameters[WORLDVIEWPROJ_PARAM_NAME]);
                extractedParams.Add(ENVIROMAP_PARAM_NAME, sSkyboxFx.Parameters[ENVIROMAP_PARAM_NAME]);
                Params.Add(sSkyboxFx, extractedParams);
            }
        }

        static EffectRegistry()
        {
            Params = new Dictionary<Effect, Dictionary<string, EffectParameter>>();
            sStandardParamNames = new string[] {
                EYEPOSITION_PARAM_NAME,
                WORLD_PARAM_NAME,
                WORLDVIEWPROJ_PARAM_NAME,
                WORLDINVTRANSPOSE_PARAM_NAME,
                FOGSTART_PARAM_NAME,
                FOGEND_PARAM_NAME,
                FOGCOLOR_PARAM_NAME,
            };
            sDirLightStructParamNames = new string[] {
                AMBIENT_PARAM_NAME,
                DIFFUSE_PARAM_NAME,
                SPECULAR_PARAM_NAME,
                DIRECTION_PARAM_NAME,
                ENERGY_PARAM_NAME,
            };
            sStandardStructParamNames = new Dictionary<string, string[]>();
            //sStandardStructParamNames.Add(DIRLIGHT_STRUCT_NAME, sDirLightStructParamNames);

            sDepthOnlyFx = null;
            sDepthOnlySkinFx = null;
            sSkyboxFx = null;
        }

        public static void Add(Effect fx, RenderOptions options)
        {
            if (Params.ContainsKey(fx))
                return;

            Dictionary<string, EffectParameter> extractedParams = new Dictionary<string, EffectParameter>();

            if (!(options.HasFlag(RenderOptions.NoStandardParams)))
            {
                for (int i = 0; i < sStandardParamNames.Length; i++)
                {
                    extractedParams.Add(sStandardParamNames[i], fx.Parameters[sStandardParamNames[i]]);
                }

                foreach (KeyValuePair<string, string[]> kvp in sStandardStructParamNames)
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
                                extractedParams.Add(lightParamName, fx.Parameters[kvp.Key].Elements[i].StructureMembers[kvp.Value[j]]);
                            }
                        }
                    }
                    else
                    {
                        for (int j = 0; j < kvp.Value.Length; j++)
                        {
                            extractedParams.Add(kvp.Value[j], fx.Parameters[kvp.Value[j]]);
                        }
                    }
                }
            }

            if ((options & RenderOptions.RequiresSkeletalPose) > 0)
            {
                extractedParams.Add(POSEDBONES_PARAM_NAME, fx.Parameters[POSEDBONES_PARAM_NAME]);
                extractedParams.Add(WEIGHTS_PER_VERT_PARAM_NAME, fx.Parameters[WEIGHTS_PER_VERT_PARAM_NAME]);
            }

            if ((options & RenderOptions.RequiresShadowMap) > 0)
            {
                fx.Parameters[INVSHADOWMAPSIZE_PARAM_NAME].SetValue(1.0f / (float)(SceneResources.SMAP_SIZE));
                extractedParams.Add(SHADOWTRANSFORM_PARAM_NAME, fx.Parameters[SHADOWTRANSFORM_PARAM_NAME]);
                extractedParams.Add(SHADOWMAP_PARAM_NAME, fx.Parameters[SHADOWMAP_PARAM_NAME]);
            }

            if ((options & RenderOptions.RequiresEnviroMap) > 0)
            {
                extractedParams.Add(ENVIROMAP_PARAM_NAME, fx.Parameters[ENVIROMAP_PARAM_NAME]);
            }

            if ((options & RenderOptions.RequiresHDRLighting) > 0)
            {
                extractedParams.Add(IRRADIANCEMAP_PARAM_NAME, fx.Parameters[IRRADIANCEMAP_PARAM_NAME]);
                extractedParams.Add(SPECPREFILTER_PARAM_NAME, fx.Parameters[SPECPREFILTER_PARAM_NAME]);
                extractedParams.Add(NUMSPECLEVELS_PARAM_NAME, fx.Parameters[NUMSPECLEVELS_PARAM_NAME]);
                extractedParams.Add(SPECEXPFACTOR_PARAM_NAME, fx.Parameters[SPECEXPFACTOR_PARAM_NAME]);
                extractedParams.Add(AMBIENTLIGHT_PARAM_NAME, fx.Parameters[AMBIENTLIGHT_PARAM_NAME]);
            }

            if (options.HasFlag(RenderOptions.RequiresFringeMap))
            {
                extractedParams.Add(FRINGEMAP_PARAM_NAME, fx.Parameters[FRINGEMAP_PARAM_NAME]);
            }

            if (options.HasFlag(RenderOptions.ParticleParams))
            {
                extractedParams.Add(VIEWPROJ_PARAM_NAME, fx.Parameters[VIEWPROJ_PARAM_NAME]);
                extractedParams.Add(PROJ_XSCALE_PARAM_NAME, fx.Parameters[PROJ_XSCALE_PARAM_NAME]);
                extractedParams.Add(VIEWPORT_SCALE_PARAM_NAME, fx.Parameters[VIEWPORT_SCALE_PARAM_NAME]);
                extractedParams.Add(SECONDS_TIMER_VALUE_PARAM_NAME, fx.Parameters[SECONDS_TIMER_VALUE_PARAM_NAME]);
            }

            if (options.HasFlag(RenderOptions.BillboardParams))
            {
                extractedParams.Add(IRRADIANCEMAP_PARAM_NAME, fx.Parameters[IRRADIANCEMAP_PARAM_NAME]);
                extractedParams.Add(ALPHA_TEST_DIRECTION_PARAM_NAME, fx.Parameters[ALPHA_TEST_DIRECTION_PARAM_NAME]);
                extractedParams.Add(SECONDS_TIMER_VALUE_PARAM_NAME, fx.Parameters[SECONDS_TIMER_VALUE_PARAM_NAME]);
            }

            Params.Add(fx, extractedParams);
        }


        //public static void SetFog(FogDesc fogInfo)
        //{
        //    foreach (Effect fx in Params.Keys)
        //    {
        //        Dictionary<string, EffectParameter> fxParams = Params[fx];

        //        fxParams[FOGSTART_PARAM_NAME].SetValue(fogInfo.StartDistance);
        //        fxParams[FOGEND_PARAM_NAME].SetValue(fogInfo.EndDistance);
        //        fxParams[FOGCOLOR_PARAM_NAME].SetValue(fogInfo.Color.ToVector4());
        //    }
        //}


        //public static void SetLighting(List<DirectLight> lights, Effect fx)
        //{
        //    Dictionary<string, EffectParameter> fxParams = Params[fx];

        //    fxParams[NUMLIGHTS_PARAM_NAME].SetValue(lights.Count);
        //    for (int i = 0; i < lights.Count; i++)
        //    {
        //        fxParams[DIRLIGHT_STRUCT_NAME + AMBIENT_PARAM_NAME + i.ToString()].SetValue(lights[i].Ambient);
        //        fxParams[DIRLIGHT_STRUCT_NAME + DIFFUSE_PARAM_NAME + i.ToString()].SetValue(lights[i].Diffuse);
        //        fxParams[DIRLIGHT_STRUCT_NAME + SPECULAR_PARAM_NAME + i.ToString()].SetValue(lights[i].Specular);
        //        fxParams[DIRLIGHT_STRUCT_NAME + DIRECTION_PARAM_NAME + i.ToString()].SetValue(lights[i].Direction);
        //        fxParams[DIRLIGHT_STRUCT_NAME + ENERGY_PARAM_NAME + i.ToString()].SetValue(lights[i].Energy);
        //    }
        //}

        public static void ClearRegistry()
        {
            Params.Clear();
        }

    }
}
