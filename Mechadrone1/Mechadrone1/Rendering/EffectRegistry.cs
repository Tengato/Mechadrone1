using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Skelemator;
using Manifracture;

namespace Mechadrone1.Rendering
{
    static class EffectRegistry
    {
        public static Dictionary<Effect, Dictionary<string, EffectParameter>> Params;

        public static List<Model> RegisteredModels = new List<Model>();

        const int NUM_LIGHTS_PER_EFFECT = 3;

        public const string EYEPOSITION_PARAM_NAME = "EyePosition";
        public const string WORLD_PARAM_NAME = "World";
        public const string WORLDVIEWPROJ_PARAM_NAME = "WorldViewProj";
        public const string WORLDINVTRANSPOSE_PARAM_NAME = "WorldInvTranspose";
        public const string NUMLIGHTS_PARAM_NAME = "NumLights";
        public const string FOGSTART_PARAM_NAME = "FogStart";
        public const string FOGEND_PARAM_NAME = "FogEnd";
        public const string FOGCOLOR_PARAM_NAME = "FogColor";
        public const string POSEDBONES_PARAM_NAME = "PosedBones";
        public const string WEIGHTS_PER_VERT_PARAM_NAME = "WeightsPerVert";
        public const string INVSHADOWMAPSIZE_PARAM_NAME = "InvShadowMapSize";
        public const string SHADOWLIGHTINDEX_PARAM_NAME = "ShadowLightIndex";
        public const string SHADOWTRANSFORM_PARAM_NAME = "ShadowTransform";
        public const string SHADOWMAP_PARAM_NAME = "ShadowMap";
        public const string ENVIROMAP_PARAM_NAME = "EnviroMap";

        readonly static string[] StandardParamNames = new string[8] {
            EYEPOSITION_PARAM_NAME,
            WORLD_PARAM_NAME,
            WORLDVIEWPROJ_PARAM_NAME,
            WORLDINVTRANSPOSE_PARAM_NAME,
            NUMLIGHTS_PARAM_NAME,
            FOGSTART_PARAM_NAME,
            FOGEND_PARAM_NAME,
            FOGCOLOR_PARAM_NAME,
        };

        static Dictionary<string, string[]> StandardStructParamNames;

        public const string DIRLIGHT_STRUCT_NAME = "DirLights";
        public const string AMBIENT_PARAM_NAME = "Ambient";
        public const string DIFFUSE_PARAM_NAME = "Diffuse";
        public const string SPECULAR_PARAM_NAME = "Specular";
        public const string DIRECTION_PARAM_NAME = "Direction";
        public const string ENERGY_PARAM_NAME = "Energy";

        readonly static string[] DirLightStructParamNames = new string[5] {
            AMBIENT_PARAM_NAME,
            DIFFUSE_PARAM_NAME,
            SPECULAR_PARAM_NAME,
            DIRECTION_PARAM_NAME,
            ENERGY_PARAM_NAME,
        };

        public static EffectParameter DOWorldViewProj;
        public static EffectParameter DOSWorldViewProj;
        public static EffectParameter DOSWeightsPerVert;
        public static EffectParameter DOSPosedBones;

        private static Effect depthOnlyFx;
        public static Effect DepthOnlyFx
        {
            get
            {
                return depthOnlyFx;
            }
            set
            {
                depthOnlyFx = value;
                DOWorldViewProj = depthOnlyFx.Parameters["WorldViewProj"];
            }
        }

        private static Effect depthOnlySkinFx;
        public static Effect DepthOnlySkinFx
        {
            get
            {
                return depthOnlySkinFx;
            }
            set
            {
                depthOnlySkinFx = value;
                DOSWorldViewProj = depthOnlySkinFx.Parameters["WorldViewProj"];
                DOSWeightsPerVert = depthOnlySkinFx.Parameters["WeightsPerVert"];
                DOSPosedBones = depthOnlySkinFx.Parameters["PosedBones"];
            }
        }


        static EffectRegistry()
        {
            Params = new Dictionary<Effect,Dictionary<string, EffectParameter>>();
            StandardStructParamNames = new Dictionary<string, string[]>();
            StandardStructParamNames.Add(DIRLIGHT_STRUCT_NAME, DirLightStructParamNames);
        }

        public static void Add(Effect fx, RenderOptions options)
        {
            Dictionary<string, EffectParameter> standardParams = new Dictionary<string, EffectParameter>();

            for (int i = 0; i < StandardParamNames.Length; i++)
            {
                standardParams.Add(StandardParamNames[i], fx.Parameters[StandardParamNames[i]]);
            }

            foreach (KeyValuePair<string, string[]> kvp in StandardStructParamNames)
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

            if ((options & RenderOptions.RequiresSkeletalPose) > 0)
            {
                standardParams.Add(POSEDBONES_PARAM_NAME, fx.Parameters[POSEDBONES_PARAM_NAME]);
                standardParams.Add(WEIGHTS_PER_VERT_PARAM_NAME, fx.Parameters[WEIGHTS_PER_VERT_PARAM_NAME]);
            }

            if ((options & RenderOptions.RequiresShadowMap) > 0)
            {
                fx.Parameters[SHADOWLIGHTINDEX_PARAM_NAME].SetValue(0);
                fx.Parameters[INVSHADOWMAPSIZE_PARAM_NAME].SetValue(1.0f / (float)(SceneManager.SMAP_SIZE));
                standardParams.Add(SHADOWTRANSFORM_PARAM_NAME, fx.Parameters[SHADOWTRANSFORM_PARAM_NAME]);
                standardParams.Add(SHADOWMAP_PARAM_NAME, fx.Parameters[SHADOWMAP_PARAM_NAME]);
            }

            Params.Add(fx, standardParams);
        }


        public static void SetFog(FogDesc fogInfo)
        {
            foreach (Effect fx in Params.Keys)
            {
                Dictionary<string, EffectParameter> fxParams = Params[fx];

                fxParams[FOGSTART_PARAM_NAME].SetValue(fogInfo.StartDistance);
                fxParams[FOGEND_PARAM_NAME].SetValue(fogInfo.EndDistance);
                fxParams[FOGCOLOR_PARAM_NAME].SetValue(fogInfo.Color.ToVector4());
            }
        }


        public static void SetLighting(List<DirectLight> lights, Effect fx)
        {
            Dictionary<string, EffectParameter> fxParams = Params[fx];

            fxParams[NUMLIGHTS_PARAM_NAME].SetValue(lights.Count);
            for (int i = 0; i < lights.Count; i++)
            {
                fxParams[DIRLIGHT_STRUCT_NAME + AMBIENT_PARAM_NAME + i.ToString()].SetValue(lights[i].Ambient);
                fxParams[DIRLIGHT_STRUCT_NAME + DIFFUSE_PARAM_NAME + i.ToString()].SetValue(lights[i].Diffuse);
                fxParams[DIRLIGHT_STRUCT_NAME + SPECULAR_PARAM_NAME + i.ToString()].SetValue(lights[i].Specular);
                fxParams[DIRLIGHT_STRUCT_NAME + DIRECTION_PARAM_NAME + i.ToString()].SetValue(lights[i].Direction);
                fxParams[DIRLIGHT_STRUCT_NAME + ENERGY_PARAM_NAME + i.ToString()].SetValue(lights[i].Energy);
            }
        }

        public static void ClearRegistry()
        {
            Params.Clear();
            RegisteredModels.Clear();
        }

    }
}
