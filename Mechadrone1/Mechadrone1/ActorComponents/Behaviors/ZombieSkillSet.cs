using Microsoft.Xna.Framework.Content;
using Manifracture;
using System;
using System.IO;

namespace Mechadrone1
{
    class ZombieSkillSet : Behavior
    {
        public BipedWeapon RangedSkill { get; private set; }
        public BipedWeapon MeleeSkill { get; private set; }

        public ZombieSkillSet(Actor owner)
            : base(owner)
        {
            RangedSkill = null;
            MeleeSkill = null;
        }

        public override void Initialize(ContentManager contentLoader, ComponentManifest manifest)
        {
            string skillAssetName = (string)(manifest.Properties[ManifestKeys.RANGED_SKILL_ASSET_NAME]);
            ComponentManifest weaponManifest = contentLoader.Load<ComponentManifest>(Path.Combine("skills", skillAssetName));
            Type weaponType = Type.GetType(weaponManifest.TypeFullName);
            object[] basicCtorParams = new object[] { Owner.Id };
            RangedSkill = Activator.CreateInstance(weaponType, basicCtorParams) as BipedWeapon;
            RangedSkill.Initialize(contentLoader, weaponManifest);

            skillAssetName = (string)(manifest.Properties[ManifestKeys.MELEE_SKILL_ASSET_NAME]);
            weaponManifest = contentLoader.Load<ComponentManifest>(Path.Combine("skills", skillAssetName));
            weaponType = Type.GetType(weaponManifest.TypeFullName);
            MeleeSkill = Activator.CreateInstance(weaponType, basicCtorParams) as BipedWeapon;
            MeleeSkill.Initialize(contentLoader, weaponManifest);
        }

        public override void Release()
        {
            RangedSkill.Release();
            MeleeSkill.Release();
        }
    }
}
