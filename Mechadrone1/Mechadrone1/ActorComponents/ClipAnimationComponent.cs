using Skelemator;
using Microsoft.Xna.Framework;

namespace Mechadrone1
{
    class ClipAnimationComponent : AnimationComponent
    {
        public SkinClipPlayer SkinClipPlayer { get; private set; }

        public override ISkinnedSkeletonPoser AnimationPlayer { get { return SkinClipPlayer; } }

        public ClipAnimationComponent(Actor owner)
            : base(owner)
        {
            SkinClipPlayer = null;
        }

        protected override void CreateAnimationPlayer(AnimationPackage animationPackage)
        {
            if (animationPackage.SkinningData != null)
            {
                SkinClipPlayer = new SkinClipPlayer(animationPackage.SkinningData);
            }
            GameResources.ActorManager.AnimationUpdateStep += AnimationUpdateHandler;
        }

        private void AnimationUpdateHandler(object sender, UpdateStepEventArgs e)
        {
            SkinClipPlayer.Update(e.GameTime.ElapsedGameTime, true);
        }

        public override void Release()
        {
            base.Release();

            GameResources.ActorManager.AnimationUpdateStep -= AnimationUpdateHandler;
        }
    }
}
