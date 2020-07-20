using AmeisenBotX.Core.Data.Enums;
using System.Drawing;

namespace AmeisenBotX.Utils
{
    public static class WowColorsDrawing
    {
        public static readonly Brush dkPrimaryBrush = new SolidBrush(Color.FromArgb(255, 196, 30, 59));
        public static readonly Brush dkSecondaryBrush = new SolidBrush(Color.FromArgb(255, 0, 209, 255));

        public static readonly Brush druidPrimaryBrush = new SolidBrush(Color.FromArgb(255, 255, 125, 10));
        public static readonly Brush druidSecondaryBrush = new SolidBrush(Color.FromArgb(255, 0, 0, 255));

        public static readonly Brush hunterPrimaryBrush = new SolidBrush(Color.FromArgb(255, 171, 212, 115));
        public static readonly Brush hunterSecondaryBrush = new SolidBrush(Color.FromArgb(255, 0, 0, 255));

        public static readonly Brush magePrimaryBrush = new SolidBrush(Color.FromArgb(255, 105, 204, 240));
        public static readonly Brush mageSecondaryBrush = new SolidBrush(Color.FromArgb(255, 0, 0, 255));

        public static readonly Brush paladinPrimaryBrush = new SolidBrush(Color.FromArgb(255, 245, 140, 186));
        public static readonly Brush paladinSecondaryBrush = new SolidBrush(Color.FromArgb(255, 0, 0, 255));

        public static readonly Brush priestPrimaryBrush = new SolidBrush(Color.FromArgb(255, 255, 255, 255));
        public static readonly Brush priestSecondaryBrush = new SolidBrush(Color.FromArgb(255, 0, 0, 255));

        public static readonly Brush roguePrimaryBrush = new SolidBrush(Color.FromArgb(255, 255, 245, 105));
        public static readonly Brush rogueSecondaryBrush = new SolidBrush(Color.FromArgb(255, 255, 255, 0));

        public static readonly Brush shamanPrimaryBrush = new SolidBrush(Color.FromArgb(255, 0, 112, 222));
        public static readonly Brush shamanSecondaryBrush = new SolidBrush(Color.FromArgb(255, 0, 0, 255));

        public static readonly Brush warlockPrimaryBrush = new SolidBrush(Color.FromArgb(255, 148, 130, 201));
        public static readonly Brush warlockSecondaryBrush = new SolidBrush(Color.FromArgb(255, 0, 0, 255));

        public static readonly Brush warriorPrimaryBrush = new SolidBrush(Color.FromArgb(255, 199, 156, 110));
        public static readonly Brush warriorSecondaryBrush = new SolidBrush(Color.FromArgb(255, 255, 0, 0));

        public static Brush GetClassPrimaryBrush(WowClass wowClass)
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
                _ => new SolidBrush(Color.White),
            };
        }
    }
}