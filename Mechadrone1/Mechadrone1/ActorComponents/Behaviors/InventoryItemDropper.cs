using Microsoft.Xna.Framework.Content;
using Manifracture;
using System;
using Microsoft.Xna.Framework;

namespace Mechadrone1
{
    class InventoryItemDropper : Behavior
    {
        private const float ITEM_POP_SPEED = 30.0f;

        public InventoryItemDropper(Actor owner)
            : base(owner)
        {
            Owner.BecameAvatar += BecameAvatarHandler;
        }

        private void BecameAvatarHandler(object sender, EventArgs e)
        {
            PlayerView pv = GameResources.ActorManager.GetPlayerViewOfAvatar(Owner.Id);
            pv.AvatarDesc.ItemDropped += ItemDroppedHandler;
        }

        public override void Release()
        {
            PlayerView pv = GameResources.ActorManager.GetPlayerViewOfAvatar(Owner.Id);
            if (pv != null)
                pv.AvatarDesc.ItemDropped -= ItemDroppedHandler;
        }

        private void ItemDroppedHandler(object sender, ItemEventArgs e)
        {
            Actor dropped = GameResources.ActorManager.SpawnTemplate("GenericPickup");
            TransformComponent droppedXform = dropped.GetComponent<TransformComponent>(ActorComponent.ComponentType.Transform);
            TransformComponent ownerXform = Owner.GetComponent<TransformComponent>(ActorComponent.ComponentType.Transform);
            droppedXform.Transform = Matrix.CreateTranslation(5.0f, 3.0f, -3.0f) * ownerXform.Transform;
            InventoryPickup ip = dropped.GetBehavior<InventoryPickup>();
            ip.Item = e.Item;

            DynamicCollisionComponent collisionComponent = dropped.GetComponent<DynamicCollisionComponent>(ActorComponent.ComponentType.Physics);
            BEPUutilities.Vector3 forward = collisionComponent.Entity.OrientationMatrix.Forward * ITEM_POP_SPEED;
            collisionComponent.Entity.ApplyLinearImpulse(ref forward);
        }
    }
}
