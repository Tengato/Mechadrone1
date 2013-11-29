using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using SlagformCommon;

namespace Mechadrone1.Gameplay.Prefabs
{
    class SimpleBot : TPPedestrian
    {

        public SimpleBot(Game1Manager owner) : base(owner)
        {
        }


        public override void RegisterUpdateHandlers()
        {
            base.RegisterUpdateHandlers();
            owner.BotControlUpdateStep += UpdateBotControl;
        }

        public void UpdateBotControl(object sender, UpdateStepEventArgs e)
        {
            // Just walk toward the player:
            Vector3 targetPosition = owner.GetGameObject("Suzanne").Position - Position;
            character.ViewDirection = BepuConverter.Convert(targetPosition);

            character.HorizontalMotionConstraint.MovementDirection = new BEPUutilities.Vector2(0.0f, 0.2f);

            desiredState = BipedStates.Neutral;
        }

    }
}
