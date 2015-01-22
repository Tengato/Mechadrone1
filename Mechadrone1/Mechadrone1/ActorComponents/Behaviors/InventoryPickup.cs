using Microsoft.Xna.Framework.Content;
using Manifracture;
using System;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using System.IO;

namespace Mechadrone1
{
    class InventoryPickup : Behavior
    {
        private bool mTriggerSet;
        public Item Item { get; set; }

        public InventoryPickup(Actor owner)
            : base(owner)
        {
            mTriggerSet = true;
            Item = null;
        }

        public override void Initialize(ContentManager contentLoader, ComponentManifest manifest)
        {
            if (manifest.Properties.ContainsKey(ManifestKeys.ITEM_NAME))
                LoadItem((string)(manifest.Properties[ManifestKeys.ITEM_NAME]), contentLoader);

            Owner.ComponentsCreated += ComponentsCreatedHandler;
        }

        private void LoadItem(string assetName, ContentManager contentLoader)
        {
            ComponentManifest itemManifest = contentLoader.Load<ComponentManifest>(Path.Combine("items", assetName));
            Type itemType = Type.GetType(itemManifest.TypeFullName);
            Item = Activator.CreateInstance(itemType) as Item;
            Item.Initialize(contentLoader, itemManifest);
        }

        private void ComponentsCreatedHandler(object sender, EventArgs e)
        {
            DynamicCollisionComponent dcc = Owner.GetComponent<DynamicCollisionComponent>(ActorComponent.ComponentType.Physics);
            if (dcc == null)
                throw new LevelManifestException("Expected ActorComponent missing.");

            dcc.Entity.CollisionInformation.CollisionRules.Group = GameResources.ActorManager.PickupsCollisionGroup;
            dcc.Entity.CollisionInformation.Events.InitialCollisionDetected += InitialCollisionDetectedHandler;
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
                PlayerView finder = GameResources.ActorManager.GetPlayerViewOfAvatar((int)(otherEntityCollidable.Entity.Tag));
                finder.AvatarDesc.ObtainItem(Item);
                mTriggerSet = false;
                Owner.Despawn();
            }
        }

        public override void Release()
        {
        }
    }
}
