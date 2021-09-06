using AmeisenBotX.Wow.Objects.Enums;

using System.Drawing;

namespace AmeisenBotX.Utils
{
    public static class WowColorsDrawing
    {
        public static readonly SolidBrush dkPrimaryBrush = new(Color.FromArgb(255, 196, 30, 59));
        public static readonly SolidBrush dkSecondaryBrush = new(Color.FromArgb(255, 0, 209, 255));

        public static readonly SolidBrush druidPrimaryBrush = new(Color.FromArgb(255, 255, 125, 10));
        public static readonly SolidBrush druidSecondaryBrush = new(Color.FromArgb(255, 0, 0, 255));

        public static readonly SolidBrush hunterPrimaryBrush = new(Color.FromArgb(255, 171, 212, 115));
        public static readonly SolidBrush hunterSecondaryBrush = new(Color.FromArgb(255, 0, 0, 255));

        public static readonly SolidBrush magePrimaryBrush = new(Color.FromArgb(255, 105, 204, 240));
        public static readonly SolidBrush mageSecondaryBrush = new(Color.FromArgb(255, 0, 0, 255));

        public static readonly SolidBrush paladinPrimaryBrush = new(Color.FromArgb(255, 245, 140, 186));
        public static readonly SolidBrush paladinSecondaryBrush = new(Color.FromArgb(255, 0, 0, 255));

        public static readonly SolidBrush priestPrimaryBrush = new(Color.FromArgb(255, 255, 255, 255));
        public static readonly SolidBrush priestSecondaryBrush = new(Color.FromArgb(255, 0, 0, 255));

        public static readonly SolidBrush roguePrimaryBrush = new(Color.FromArgb(255, 255, 245, 105));
        public static readonly SolidBrush rogueSecondaryBrush = new(Color.FromArgb(255, 255, 255, 0));

        public static readonly SolidBrush shamanPrimaryBrush = new(Color.FromArgb(255, 0, 112, 222));
        public static readonly SolidBrush shamanSecondaryBrush = new(Color.FromArgb(255, 0, 0, 255));

        public static readonly SolidBrush unknownBrush = new(Color.FromArgb(255, 255, 255, 255));
        public static readonly SolidBrush warlockPrimaryBrush = new(Color.FromArgb(255, 148, 130, 201));
        public static readonly SolidBrush warlockSecondaryBrush = new(Color.FromArgb(255, 0, 0, 255));

        public static readonly SolidBrush warriorPrimaryBrush = new(Color.FromArgb(255, 199, 156, 110));
        public static readonly SolidBrush warriorSecondaryBrush = new(Color.FromArgb(255, 255, 0, 0));

        public static SolidBrush GetClassPrimaryBrush(WowClass wowClass)
        {
            return wowClass switch
            {
                WowClass.Deathknight => dkPrimaryBrush,
                WowClass.Druid => druidPrimaryBrush,
                WowClass.Hunter => hunterPrimaryBrush,
                WowClass.Mage => magePrimaryBrush,
                WowClass.Paladin => paladinPrimaryBrush,
                WowClass.Priest => priestPrimaryBrush,
                WowClass.Rogue => roguePrimaryBrush,
                WowClass.Shaman => shamanPrimaryBrush,
                WowClass.Warlock => warlockPrimaryBrush,
                WowClass.Warrior => warriorPrimaryBrush,
                _ => unknownBrush,
            };
        }
    }
}