using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Manifracture;

namespace Mechadrone1
{
    abstract class ParamSetter
    {
        public enum Category
        {
            Common,
            Light,
            Skin,
            Shadow,
            Fog,
            EnvironmentMap,
            FringeMap,
            Particles,
            Billboard,
        }

        public abstract Category Domain { get; }

        public abstract void Set(Effect effect, RenderContext context, Matrix transform);
    }

    class WorldViewProjParamSetter : ParamSetter
    {
        public override Category Domain { get { return Category.Common; } }

        public override void Set(Effect effect, RenderContext context, Matrix transform)
        {
            EffectRegistry.Params[effect][EffectRegistry.WORLDVIEWPROJ_PARAM_NAME].SetValue(transform * context.VisibilityFrustum.Matrix);
        }
    }

    class CommonParamSetter : ParamSetter
    {
        public override Category Domain { get { return Category.Common; } }

        public override void Set(Effect effect, RenderContext context, Matrix transform)
        {
            EffectRegistry.Params[effect][EffectRegistry.WORLD_PARAM_NAME].SetValue(transform);
            EffectRegistry.Params[effect][EffectRegistry.WORLDVIEWPROJ_PARAM_NAME].SetValue(transform * context.VisibilityFrustum.Matrix);
            EffectRegistry.Params[effect][EffectRegistry.WORLDINVTRANSPOSE_PARAM_NAME].SetValue(Matrix.Transpose(Matrix.Invert(transform)));
            EffectRegistry.Params[effect][EffectRegistry.EYEPOSITION_PARAM_NAME].SetValue(context.EyePosition);
        }
    }

    class SkyboxWvpParamSetter : ParamSetter
    {
        public override Category Domain { get { return Category.Common; } }

        public override void Set(Effect effect, RenderContext context, Matrix transform)
        {
            EffectRegistry.Params[effect][EffectRegistry.WORLDVIEWPROJ_PARAM_NAME].SetValue(
                Matrix.CreateTranslation(context.EyePosition) * context.VisibilityFrustum.Matrix);
        }
    }

    class SkinParamSetter : ParamSetter
    {
        // Holding on to this reference should be okay since the scene graph nodes which own this object should
        // be released when the Actor is despawned.
        public AnimationComponent AnimationComponent { get; set; }

        public SkinParamSetter()
        {
            AnimationComponent = null;
        }

        public override Category Domain { get { return Category.Skin; } }

        public override void Set(Effect effect, RenderContext context, Matrix transform)
        {
            if (AnimationComponent != null)
            {
                EffectRegistry.Params[effect][EffectRegistry.POSEDBONES_PARAM_NAME].SetValue(AnimationComponent.GetCurrentPose());
                EffectRegistry.Params[effect][EffectRegistry.WEIGHTS_PER_VERT_PARAM_NAME].SetValue(AnimationComponent.Animations.WeightsPerVert);
            }
            else
            {
                EffectRegistry.Params[effect][EffectRegistry.POSEDBONES_PARAM_NAME].SetValue(AnimationComponent.BindPose);
                EffectRegistry.Params[effect][EffectRegistry.WEIGHTS_PER_VERT_PARAM_NAME].SetValue(4);
            }
        }
    }

    class ShadowParamSetter : ParamSetter
    {
        public override Category Domain { get { return Category.Shadow; } }

        public override void Set(Effect effect, RenderContext context, Matrix transform)
        {
            EffectRegistry.Params[effect][EffectRegistry.SHADOWTRANSFORM_PARAM_NAME].SetValue(transform * context.SceneResources.ShadowTransform);
            EffectRegistry.Params[effect][EffectRegistry.SHADOWMAP_PARAM_NAME].SetValue(context.SceneResources.ShadowMap);
        }
    }

    class EnvironmentMapParamSetter : ParamSetter
    {
        public override Category Domain { get { return Category.EnvironmentMap; } }

        public override void Set(Effect effect, RenderContext context, Matrix transform)
        {
            EffectRegistry.Params[effect][EffectRegistry.ENVIROMAP_PARAM_NAME].SetValue(context.SceneResources.EnvironmentMap);
        }
    }

    class FringeMapParamSetter : ParamSetter
    {
        public override Category Domain { get { return Category.FringeMap; } }

        public override void Set(Effect effect, RenderContext context, Matrix transform)
        {
            EffectRegistry.Params[effect][EffectRegistry.FRINGEMAP_PARAM_NAME].SetValue(context.SceneResources.FringeMap);
        }
    }

    class DirLightParamSetter : ParamSetter
    {
        public override Category Domain { get { return Category.Light; } }

        public override void Set(Effect effect, RenderContext context, Matrix transform)
        {
            const int NUM_LIGHTS = 1;

            DirLight[] dirLight = new DirLight[NUM_LIGHTS];

            dirLight[0] = new DirLight();
            Actor caster = GameResources.ActorManager.GetActorById(context.SceneResources.ShadowCasterActorId);
            TransformComponent casterTransform = caster.GetComponent<TransformComponent>(ActorComponent.ComponentType.Transform);
            dirLight[0].Direction = Vector3.Transform(Vector3.Forward, casterTransform.Orientation);
            dirLight[0].Ambient = Vector4.Zero;
            dirLight[0].Diffuse = new Vector4(0.65f, 0.65f, 0.6f, 1.0f);
            dirLight[0].Specular = new Vector4(0.05f, 0.05f, 0.05f, 1.0f);
            dirLight[0].Energy = 1.0f;


            EffectRegistry.Params[effect][EffectRegistry.NUMLIGHTS_PARAM_NAME].SetValue(NUM_LIGHTS);

            for (int i = 0; i < NUM_LIGHTS; i++)
            {
                EffectRegistry.Params[effect][EffectRegistry.DIRLIGHT_STRUCT_NAME + EffectRegistry.AMBIENT_PARAM_NAME + i.ToString()].SetValue(dirLight[i].Ambient);
                EffectRegistry.Params[effect][EffectRegistry.DIRLIGHT_STRUCT_NAME + EffectRegistry.DIFFUSE_PARAM_NAME + i.ToString()].SetValue(dirLight[i].Diffuse);
                EffectRegistry.Params[effect][EffectRegistry.DIRLIGHT_STRUCT_NAME + EffectRegistry.SPECULAR_PARAM_NAME + i.ToString()].SetValue(dirLight[i].Specular);
                EffectRegistry.Params[effect][EffectRegistry.DIRLIGHT_STRUCT_NAME + EffectRegistry.DIRECTION_PARAM_NAME + i.ToString()].SetValue(dirLight[i].Direction);
                EffectRegistry.Params[effect][EffectRegistry.DIRLIGHT_STRUCT_NAME + EffectRegistry.ENERGY_PARAM_NAME + i.ToString()].SetValue(dirLight[i].Energy);
            }
        }
    }

    // TODO: P2: Some of these param setters don't need to run every time:
    class HDRLightParamSetter : ParamSetter
    {
        public override Category Domain { get { return Category.Light; } }

        public override void Set(Effect effect, RenderContext context, Matrix transform)
        {
            Actor hdrLightActor = GameResources.ActorManager.GetActorById(context.SceneResources.HDRLightActorId);
            HDRLightComponent hdr = hdrLightActor.GetComponent<HDRLightComponent>(ActorComponent.ComponentType.Light);
            EffectRegistry.Params[effect][EffectRegistry.IRRADIANCEMAP_PARAM_NAME].SetValue(hdr.IrradianceMap);
            EffectRegistry.Params[effect][EffectRegistry.SPECPREFILTER_PARAM_NAME].SetValue(hdr.SpecPrefilter);
            EffectRegistry.Params[effect][EffectRegistry.NUMSPECLEVELS_PARAM_NAME].SetValue(hdr.NumSpecLevels);
            EffectRegistry.Params[effect][EffectRegistry.SPECEXPFACTOR_PARAM_NAME].SetValue(hdr.SpecExponentFactor);
            EffectRegistry.Params[effect][EffectRegistry.AMBIENTLIGHT_PARAM_NAME].SetValue(hdr.AmbientLight);
        }
    }

    class FogParamSetter : ParamSetter
    {
        public override Category Domain { get { return Category.Fog; } }

        public override void Set(Effect effect, RenderContext context, Matrix transform)
        {
            Actor fogActor = GameResources.ActorManager.GetActorById(context.SceneResources.FogActorId);
            FogComponent fog = fogActor.GetComponent<FogComponent>(ActorComponent.ComponentType.Fog);
            EffectRegistry.Params[effect][EffectRegistry.FOGCOLOR_PARAM_NAME].SetValue(fog.Color.ToVector3());
            EffectRegistry.Params[effect][EffectRegistry.FOGSTART_PARAM_NAME].SetValue(fog.Start);
            EffectRegistry.Params[effect][EffectRegistry.FOGEND_PARAM_NAME].SetValue(fog.End);
        }
    }

    delegate float SecondsTimerDelegate();

    class ParticlesParamSetter : ParamSetter
    {
        private SecondsTimerDelegate mTimer;

        public override Category Domain { get { return Category.Particles; } }

        public ParticlesParamSetter(SecondsTimerDelegate timer)
        {
            mTimer = timer;
        }

        public override void Set(Effect effect, RenderContext context, Matrix transform)
        {
            EffectRegistry.Params[effect][EffectRegistry.VIEWPROJ_PARAM_NAME].SetValue(context.VisibilityFrustum.Matrix);
            // We need to compute the M11 member of the projection matrix, which is -n / r, where n, r is the z, x coords of
            // the right edge of the frustum's near plane in camera space.
            float cosTheta = Vector3.Dot(context.VisibilityFrustum.Near.Normal, context.VisibilityFrustum.Right.Normal);
            float projXScale = 1.0f / (float)(Math.Tan(MathHelper.PiOver2 - Math.Acos(cosTheta)));
            EffectRegistry.Params[effect][EffectRegistry.PROJ_XSCALE_PARAM_NAME].SetValue(projXScale);
            Vector2 vpScale = new Vector2(0.5f / SharedResources.Game.GraphicsDevice.Viewport.AspectRatio, -0.5f);
            EffectRegistry.Params[effect][EffectRegistry.VIEWPORT_SCALE_PARAM_NAME].SetValue(vpScale);
            EffectRegistry.Params[effect][EffectRegistry.SECONDS_TIMER_VALUE_PARAM_NAME].SetValue(mTimer());
        }
    }

    class BillboardParamSetter : ParamSetter
    {
        private bool mIsOpaqueMode;
        private SecondsTimerDelegate mTimer;

        public override Category Domain { get { return Category.Billboard; } }

        public BillboardParamSetter(bool isOpaqueMode, SecondsTimerDelegate timer)
        {
            mIsOpaqueMode = isOpaqueMode;
            mTimer = timer;
        }

        public override void Set(Effect effect, RenderContext context, Matrix transform)
        {
            Actor hdrLightActor = GameResources.ActorManager.GetActorById(context.SceneResources.HDRLightActorId);
            HDRLightComponent hdr = hdrLightActor.GetComponent<HDRLightComponent>(ActorComponent.ComponentType.Light);
            EffectRegistry.Params[effect][EffectRegistry.IRRADIANCEMAP_PARAM_NAME].SetValue(hdr.IrradianceMap);
            EffectRegistry.Params[effect][EffectRegistry.ALPHA_TEST_DIRECTION_PARAM_NAME].SetValue(mIsOpaqueMode ? 1.0f : -1.0f);
            EffectRegistry.Params[effect][EffectRegistry.SECONDS_TIMER_VALUE_PARAM_NAME].SetValue(mTimer());
        }
    }
}
