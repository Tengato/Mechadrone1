using Microsoft.Xna.Framework;
using System;
using Manifracture;
using SlagformCommon;

namespace Mechadrone1
{
    class TPBipedPaletteInputProcessor : InputProcessor
    {
        private BipedControllerComponent mBipedControl;
        private BipedSkillPalette mSkillPalette;
        private BinaryControlActions[] mSkillActions;

        public TPBipedPaletteInputProcessor(PlayerIndex inputIndex)
            : base(inputIndex)
        {
            mBipedControl = null;
            mSkillPalette = null;
            mSkillActions = new BinaryControlActions[6];
            mSkillActions[0] = BinaryControlActions.Skill1;
            mSkillActions[1] = BinaryControlActions.Skill2;
            mSkillActions[2] = BinaryControlActions.Skill3;
            mSkillActions[3] = BinaryControlActions.Skill4;
            mSkillActions[4] = BinaryControlActions.Skill5;
            mSkillActions[5] = BinaryControlActions.Skill6;
        }

        public void SetControllerComponent(BipedControllerComponent bipedController)
        {
            mBipedControl = bipedController;
            if (mBipedControl != null && mSkillPalette == null)
                mBipedControl.Owner.ActorDespawning += ActorDespawningHandler;
        }

        public void SetSkillPalette(BipedSkillPalette skillPalette)
        {
            mSkillPalette = skillPalette;
            if (mSkillPalette != null && mBipedControl == null)
                mBipedControl.Owner.ActorDespawning += ActorDespawningHandler;
        }

        public override void HandleInput(GameTime gameTime, InputManager input)
        {
            if (mSkillPalette != null)
                ControlActions(input);
        }

        private void ControlActions(InputManager input)
        {
            for (int s = 0; s < 6; ++s)
            {
                if (mSkillPalette.Skills[s] != null)
                    mSkillPalette.Skills[s].UpdateInputState(input.CheckForBinaryInput(ActiveInputMap, mSkillActions[s], InputIndex), mBipedControl);
            }
        }

        private void ActorDespawningHandler(object sender, EventArgs e)
        {
            mBipedControl = null;
            mSkillPalette = null;
        }
    }
}
