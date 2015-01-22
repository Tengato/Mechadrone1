using System;
using Microsoft.Xna.Framework;
using Skelemator;

namespace Mechadrone1
{
    abstract class AnimationComponent : ActorComponent
    {
        public static Matrix[] BindPose { get; private set; } // Default pose

        public SkinningData Animations { get; private set; }

        public abstract ISkinnedSkeletonPoser AnimationPlayer { get; }
        public override ComponentType Category { get { return ComponentType.Animation; } }

        static AnimationComponent()
        {
            const int MAX_BONES = 72;
            BindPose = new Matrix[MAX_BONES];

            for (int i = 0; i < MAX_BONES; ++i)
            {
                BindPose[i] = Matrix.Identity;
            }
        }

        public AnimationComponent(Actor owner)
            : base(owner)
        {
            Animations = null;
            Owner.ComponentsCreated += ComponentsCreatedHandler;
        }

        public Matrix[] GetCurrentPose()
        {
            return (AnimationPlayer.IsActive ? AnimationPlayer.GetSkinTransforms() : BindPose);
        }

        protected virtual void ComponentsCreatedHandler(object sender, EventArgs e)
        {
            ModelRenderComponent modelRenderComponent = Owner.GetComponent<ModelRenderComponent>(ComponentType.Render);
            if (modelRenderComponent == null)
                throw new LevelManifestException("AnimationComponents expect to be accompanied by ModelRenderComponents.");

            AnimationPackage animationPackage = modelRenderComponent.VisualModel.Tag as AnimationPackage;
            if (animationPackage != null)
            {
                Animations = animationPackage.SkinningData;
                CreateAnimationPlayer(animationPackage);
            }
        }

        protected abstract void CreateAnimationPlayer(AnimationPackage animationPackage);

        public override void Release()
        {
        }
    }
}
