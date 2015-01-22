using System.Collections.Generic;
using System;
using Microsoft.Xna.Framework;
using SlagformCommon;

namespace Mechadrone1
{
    class Memory
    {
        // Key is actor id.
        private Dictionary<int, AggroRecord> mFoes;

        public Memory()
        {
            mFoes = new Dictionary<int, AggroRecord>();
        }

        public int GetLargestThreat()
        {
            float mostEnmity = Single.Epsilon;
            int largestThreat = Actor.INVALID_ACTOR_ID;
            foreach (KeyValuePair<int, AggroRecord> foe in mFoes)
            {
                if (foe.Value.Enmity >= mostEnmity)
                {
                    mostEnmity = foe.Value.Enmity;
                    largestThreat = foe.Key;
                }
            }

            return largestThreat;
        }

        public void SpotFoe(int actorId)
        {
            AggroRecord aggro;
            if (!mFoes.ContainsKey(actorId))
            {
                aggro = new AggroRecord();
                aggro.Enmity = AggroRecord.NEW_FOE_ENMITY;
                mFoes.Add(actorId, aggro);
            }
            else
            {
                aggro = mFoes[actorId];
            }

            Actor foe = GameResources.ActorManager.GetActorById(actorId);
            BipedControllerComponent bcc = foe.GetComponent<BipedControllerComponent>(ActorComponent.ComponentType.Control);

            aggro.PositionLastSensed = BepuConverter.Convert(bcc.Controller.Body.Position);
            if (aggro.TimeLastVisible < GameResources.ActorManager.CurrentTime.Subtract(SharedResources.Game.TargetElapsedTime))
            {
                // They reappeared just now.
                aggro.TimeBecameVisible = GameResources.ActorManager.CurrentTime;
            }

            aggro.TimeLastSensed = GameResources.ActorManager.CurrentTime;
            aggro.TimeLastVisible = GameResources.ActorManager.CurrentTime;
        }

        // For non-vision memory input.
        public void SenseFoe(int actorId)
        {
            AggroRecord aggro;
            if (!mFoes.ContainsKey(actorId))
            {
                aggro = new AggroRecord();
                aggro.Enmity = AggroRecord.NEW_FOE_ENMITY;
                mFoes.Add(actorId, aggro);
            }
            else
            {
                aggro = mFoes[actorId];
            }

            Actor foe = GameResources.ActorManager.GetActorById(actorId);
            BipedControllerComponent bcc = foe.GetComponent<BipedControllerComponent>(ActorComponent.ComponentType.Control);

            aggro.PositionLastSensed = BepuConverter.Convert(bcc.Controller.Body.Position);

            aggro.TimeLastSensed = GameResources.ActorManager.CurrentTime;
        }

        public void Fade(GameTime gameTime)
        {
            float enmityDecrease = AggroRecord.ENMITY_FADE_PER_SEC * (float)(gameTime.ElapsedGameTime.TotalSeconds);
            foreach (KeyValuePair<int, AggroRecord> foe in mFoes)
            {
                foe.Value.Enmity -= Math.Min(enmityDecrease, foe.Value.Enmity);
                if (GameResources.ActorManager.CurrentTime > foe.Value.TimeLastSensed &&
                    foe.Value.Enmity <= 0.0f)
                    mFoes.Remove(foe.Key);
            }
        }
    }
}
