namespace Manifracture
{
    public static class ManifestKeys
    {
        public const string RANGED_SKILL_ASSET_NAME = "RangedSkillAssetName";       // Skill asset name
        public const string MELEE_SKILL_ASSET_NAME = "MeleeSkillAssetName";         // Skill asset name
        public const string DAMAGE = "Damage";                                      // int
        public const string BEHAVIORS = "Behaviors";                                // List<ComponentManifest> - the collection of IBehaviors.
        public const string LEVEL_NAME = "LevelName";                               // Level asset name
        public const string SPEED = "Speed";                                        // float
        public const string WIDTH = "Width";                                        // float
        public const string LENGTH = "Length";                                      // float
        public const string HEIGHT = "Height";                                      // float
        public const string RADIUS = "Radius";                                      // float
        public const string MASS = "Mass";                                          // float
        public const string JUMP_SPEED = "JumpSpeed";                               // float
        public const string RUN_SPEED = "RunSpeed";                                 // float
        public const string MODEL_ADJUSTMENT = "ModelAdjustment";                   // Matrix
        public const string IS_STATIC = "IsStatic";                                 // bool
        public const string CLIPS_CAMERA = "ClipsCamera";                           // bool
        public const string RADIANCE = "Radiance";                                  // Vector3
        public const string NO_SOLVER = "NoSolver";                                 // bool
        public const string TRANSFORM_OFFSET = "TransformOffset";                   // Vector3
        public const string COLOR = "Color";                                        // Color
        public const string START = "Start";                                        // float
        public const string END = "End";                                            // float
        public const string IRRADIANCEMAP = "IrradianceMap";                        // Texture asset name
        public const string SPECPREFILTER = "SpecPrefilter";                        // Texture asset name
        public const string NUMSPECLEVELS = "NumSpecLevels";                        // int
        public const string SPECEXPONENTFACTOR = "SpecExponentFactor";              // float
        public const string AMBIENTLIGHT = "Ambient";                               // Vector3
        public const string VISUAL_MODEL = "VisualModel";                           // Model asset name
        public const string PARTICLE_SYSTEM_SETTINGS = "ParticleSystemSettings";    // Particle system settings asset name
        public const string TECHNIQUE_NAME = "Technique";                           // string
        public const string CASTS_SHADOW = "CastsShadow";                           // bool
        public const string SHAPE = "Shape";                                        // Shape
        public const string TEXTURE = "Texture";                                    // Texture asset name
        public const string MAT_SPEC_COLOR = "MaterialSpecularColor";               // Vector4 - xyz = rgb, w = exponent
        public const string BRIGHTNESS = "Brightness";                              // float
        public const string CONTRAST = "Contrast";                                  // float
        public const string TERRAIN = "Terrain";                                    // Terrain asset name
        public const string SCALE = "Scale";                                        // float
        public const string ORIENTATION = "Orientation";                            // Quaternion
        public const string POSITION = "Position";                                  // Vector3
        public const string LOOKAT = "LookAt";                                      // Vector3
        public const string RESOURCE_COST_TO_USE = "ResourceCostToUse";             // float
        public const string PROJECTILE_NAME = "ProjectileName";                     // string
        public const string REGENERATION_RATE = "RegenerationRate";                 // float
        public const string ITEM_NAME = "ItemName";                                 // Item asset name
        public const string EQUIPPED_BEHAVIOR_NAME = "EquippedBehaviorName";        // Asset name (type depends on SlotCategory)
        public const string VALUE = "Value";                                        // float
        public const string NAME = "Name";                                          // string
        public const string EQUIP_SLOT_CATEGORY = "EquipSlotCategory";              // SlotCategory
        public const string IS_RANDOMIZED = "IsRandomized";                         // bool
        public const string BOUNCINESS = "Bounciness";                              // float
        public const string STATIC_FRICTION = "StaticFriction";                     // float
        public const string KINETIC_FRICTION = "KineticFriction";                   // float
        public const string LOCK_ROTATION = "LockRotation";                         // bool
        public const string POWER = "Power";                                        // int
    }
}
