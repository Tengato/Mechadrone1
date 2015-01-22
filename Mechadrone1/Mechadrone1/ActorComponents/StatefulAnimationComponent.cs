using System;
using Skelemator;
using System.Collections.Generic;

namespace Mechadrone1
{
    class StatefulAnimationComponent : AnimationComponent
    {
        public AnimationStateMachine AnimationStateMachine { get; private set; }

        public override ISkinnedSkeletonPoser AnimationPlayer { get { return AnimationStateMachine; } }

        public StatefulAnimationComponent(Actor owner)
            : base(owner)
        {
            AnimationStateMachine = null;
        }

        protected override void CreateAnimationPlayer(AnimationPackage animationPackage)
        {
            AnimationStateMachine = new AnimationStateMachine(animationPackage);
            GameResources.ActorManager.AnimationUpdateStep += AnimationUpdateHandler;
        }

        private void AnimationUpdateHandler(object sender, UpdateStepEventArgs e)
        {
            AnimationStateMachine.Update(e.GameTime);
        }

        public override void Release()
        {
            base.Release();

            GameResources.ActorManager.AnimationUpdateStep -= AnimationUpdateHandler;
        }
    }
}
