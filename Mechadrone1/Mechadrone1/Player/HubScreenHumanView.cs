using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Manifracture;
using System;
using System.Collections.Generic;

namespace Mechadrone1
{
    class HubScreenHumanView : HumanView
    {
        private Dictionary<Type, InputProcessor> mInputProcs;

        protected override IEnumerable<KeyValuePair<Type, InputProcessor>> mInputProcessors
        {
            get { return mInputProcs; }
        }

        public HubScreenHumanView(PlayerInfo playerInfo, CharacterInfo selectedCharacter, ContentManager contentLoader)
            : base(playerInfo, selectedCharacter, contentLoader)
        {
            mInputProcs = new Dictionary<Type, InputProcessor>();
        }

        public override void Load()
        {
            base.Load();

            if (DrawSegment.MainWindow.UIElements.Count == 0)
                DrawSegment.MainWindow.UIElements.Add(UIElementDepth.STANDARD_3D_PERSP, new Standard3dPerspective());

            DrawSegment.MainWindow.UIElements.Add(UIElementDepth.DEBUG_POSITION, new PositionWidget(ActorId));
        }

        protected override void OnAssignInputMap()
        {
            foreach (KeyValuePair<Type, InputProcessor> ip in mInputProcs)
            {
                ip.Value.ActiveInputMap = mPlayerInputMap;
            }
        }

        public override void AssignAvatar(int actorId)
        {
            base.AssignAvatar(actorId);

            PlayerIndex inputIndex = GameResources.PlaySession.LocalPlayers[PlayerId];

            // Biped
            Actor avatar = GameResources.ActorManager.GetActorById(actorId);
            BipedControllerComponent bipedControl = avatar.GetComponent<BipedControllerComponent>(ActorComponent.ComponentType.Control);
            TPBipedFixedControlsInputProcessor bipedProc;
            if (mInputProcs.ContainsKey(typeof(TPBipedFixedControlsInputProcessor)))
            {
                bipedProc = (TPBipedFixedControlsInputProcessor)(mInputProcs[typeof(TPBipedFixedControlsInputProcessor)]);
            }
            else
            {
                bipedProc = new TPBipedFixedControlsInputProcessor(inputIndex, DrawSegment.MainWindow.Camera);
                mInputProcs.Add(typeof(TPBipedFixedControlsInputProcessor), bipedProc);
            }
            bipedProc.ActiveInputMap = mPlayerInputMap;
            bipedProc.SetControllerComponent(bipedControl);
        }

        protected override void PrepareInputProcessors(InputManager input) { }
    }
}
