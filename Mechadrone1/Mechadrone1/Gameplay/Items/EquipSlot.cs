namespace Mechadrone1
{
    abstract class EquipSlot
    {
        public const int SKILL1 = 0;
        public const int SKILL2 = 1;
        public const int SKILL3 = 2;
        public const int SKILL4 = 3;
        public const int SKILL5 = 4;
        public const int SKILL6 = 5;
        public const int CLASS = 6;
        public const int PERK1 = 7;
        public const int PERK2 = 8;
        public const int PERK3 = 9;
        public const int COUNT = 10;
        public const int NUM_SKILL_SLOTS = 6;
        public const int NUM_PERK_SLOTS = 3;

        public static string[] Names;

        static EquipSlot()
        {
            Names = new string[COUNT];
            Names[SKILL1] = "Skill1";
            Names[SKILL2] = "Skill2";
            Names[SKILL3] = "Skill3";
            Names[SKILL4] = "Skill4";
            Names[SKILL5] = "Skill5";
            Names[SKILL6] = "Skill6";
            Names[CLASS] = "Class";
            Names[PERK1] = "Perk1";
            Names[PERK2] = "Perk2";
            Names[PERK3] = "Perk3";
        }
    }
}
