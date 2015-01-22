using System;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using Manifracture;
using Microsoft.Xna.Framework.Content;

namespace Mechadrone1
{
    class BoxTriggerLevelLoader : Behavior
    {
        private bool mTriggerSet;
        private string mLevelName;

        public BoxTriggerLevelLoader(Actor owner)
            : base(owner)
        {
            mTriggerSet = false;
            mLevelName = String.Empty;
        }

        public override void Initialize(ContentManager contentLoader, ComponentManifest manifest)
        {
            if (manifest.Properties.ContainsKey(ManifestKeys.LEVEL_NAME))
                mLevelName = (string)(manifest.Properties[ManifestKeys.LEVEL_NAME]);

            Owner.ComponentsCreated += ComponentsCreatedHandler;
            GameResources.ActorManager.LevelLoaded += LevelLoadedHandler;
        }

        private void ComponentsCreatedHandler(object sender, EventArgs e)
        {
            DynamicCollisionComponent dcc = Owner.GetComponent<DynamicCollisionComponent>(ActorComponent.ComponentType.Physics);
            if (dcc == null)
                throw new LevelManifestException("Expected ActorComponent missing.");

            dcc.Entity.CollisionInformation.Events.InitialCollisionDetected += InitialCollisionDetectedHandler;
        }

        private void LevelLoadedHandler(object sender, EventArgs e)
        {
            mTriggerSet = true;
        }

        private void InitialCollisionDetectedHandler(EntityCollidable sender, Collidable other, CollidablePairHandler pair)
        {
            EntityCollidable otherEntityCollidable = other as EntityCollidable;
            if (otherEntityCollidable != null &&
                otherEntityCollidable.Entity != null &&
                otherEntityCollidable.Entity.Tag != null &&
                mTriggerSet &&
                GameResources.ActorManager.IsPlayer((int)(otherEntityCollidable.Entity.Tag)))
            {
                mTriggerSet = false;
                GameResources.LoadNewLevelDelegate(mLevelName);
            }
        }

        public override void Release()
        {
        }
    }
}
