using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Manifracture;
using Microsoft.Xna.Framework.Graphics;

namespace Mechadrone1
{
    // A local player's PlayerView. Requires the ability to accept device input, and contains the information
    // needed to present graphics and audio.
    abstract class HumanView : PlayerView
    {
        protected abstract IEnumerable<KeyValuePair<Type, InputProcessor>> mInputProcessors { get; }
        protected ContentManager mContentLoader;
        protected InputMap mPlayerInputMap;
        public DrawSegment DrawSegment { get; set; }

        public HumanView(PlayerInfo playerInfo, CharacterInfo selectedCharacter, ContentManager contentLoader)
            : base(playerInfo, selectedCharacter)
        {
            mContentLoader = contentLoader;
            mPlayerInputMap = null;
            DrawSegment = null;
        }

        public override void Load()
        {
            base.Load();

            mPlayerInputMap = mContentLoader.Load<InputMap>("config\\DefaultInputMap");
            OnAssignInputMap();
        }

        protected abstract void OnAssignInputMap();
        protected abstract void PrepareInputProcessors(InputManager input);

        public void HandleInput(GameTime gameTime, InputManager input)
        {
            InputHandler inputConsumer = DrawSegment.GetInputConsumer(GameResources.PlaySession.LocalPlayers[PlayerId]);

            if (inputConsumer != null)
            {
                inputConsumer(gameTime, input);
                foreach (KeyValuePair<Type, InputProcessor> inputProc in mInputProcessors)
                {
                    inputProc.Value.HandleInput(gameTime, InputManager.NeutralInput);
                }
            }
            else
            {
                PrepareInputProcessors(input);
                foreach (KeyValuePair<Type, InputProcessor> inputProc in mInputProcessors)
                {
                    inputProc.Value.HandleInput(gameTime, input);
                }
            }
        }

        // TODO: P2: What is this code for?
        /*
        public void AddInputProcessor<T>() where T : InputProcessor, new()
        {
            PlayerIndex inputIndex = GameResources.PlaySession.LocalPlayers[PlayerId];

            Actor avatar = GameResources.ActorManager.GetActorById(ActorId);
            BipedControllerComponent bipedControl = avatar.GetComponent<BipedControllerComponent>(ActorComponent.ComponentType.Control);
            T inputProc;
            if (mInputProcessors.ContainsKey(typeof(T)))
            {
                inputProc = (T)(mInputProcessors[typeof(T)]);
            }
            else
            {
                inputProc = new T(inputIndex);
                mInputProcessors.Add(typeof(T), inputProc);
            }
            inputProc.ActiveInputMap = mContentLoader.Load<InputMap>("config\\DefaultInputMap");
            inputProc.SetControllerComponent(bipedControl);
        }
        */
    }
}
