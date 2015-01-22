using Microsoft.Xna.Framework.Content;
using Manifracture;
using System;
using System.IO;

namespace Mechadrone1
{
    class ArmedSingleWeapon : Behavior, IBipedWeaponEquippable
    {
        public BipedWeapon Weapon { get; private set; }

        public ArmedSingleWeapon(Actor owner)
            : base(owner)
        {
        }

        public override void Initialize(ContentManager contentLoader, ComponentManifest manifest)
        {
            string skillAssetName = (string)(manifest.Properties[ManifestKeys.SKILL_ASSET_NAME]);
            ComponentManifest weaponManifest = contentLoader.Load<ComponentManifest>(Path.Combine("skills", skillAssetName));
            Type weaponType = Type.GetType(weaponManifest.TypeFullName);
            object[] basicCtorParams = new object[] { Owner.Id };
            Weapon = Activator.CreateInstance(weaponType, basicCtorParams) as BipedWeapon;
            Weapon.Initialize(contentLoader, weaponManifest);
        }

        public override void Release()
        {
            Weapon.Release();
        }
    }
}
