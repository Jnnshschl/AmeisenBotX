using AmeisenBotX.Core.Data.Enums;
using System.Windows.Media;

namespace AmeisenBotX.Utils
{
    public static class WowColors
    {
        public static readonly Brush dkPrimaryBrush = new SolidColorBrush(Color.FromRgb(196, 30, 59));
        public static readonly Brush dkSecondaryBrush = new SolidColorBrush(Color.FromRgb(0, 209, 255));

        public static readonly Brush druidPrimaryBrush = new SolidColorBrush(Color.FromRgb(255, 125, 10));
        public static readonly Brush druidSecondaryBrush = new SolidColorBrush(Color.FromRgb(0, 0, 255));

        public static readonly Brush hunterPrimaryBrush = new SolidColorBrush(Color.FromRgb(171, 212, 115));
        public static readonly Brush hunterSecondaryBrush = new SolidColorBrush(Color.FromRgb(0, 0, 255));

        public static readonly Brush magePrimaryBrush = new SolidColorBrush(Color.FromRgb(105, 204, 240));
        public static readonly Brush mageSecondaryBrush = new SolidColorBrush(Color.FromRgb(0, 0, 255));

        public static readonly Brush paladinPrimaryBrush = new SolidColorBrush(Color.FromRgb(245, 140, 186));
        public static readonly Brush paladinSecondaryBrush = new SolidColorBrush(Color.FromRgb(0, 0, 255));

        public static readonly Brush priestPrimaryBrush = new SolidColorBrush(Color.FromRgb(255, 255, 255));
        public static readonly Brush priestSecondaryBrush = new SolidColorBrush(Color.FromRgb(0, 0, 255));

        public static readonly Brush roguePrimaryBrush = new SolidColorBrush(Color.FromRgb(255, 245, 105));
        public static readonly Brush rogueSecondaryBrush = new SolidColorBrush(Color.FromRgb(255, 255, 0));

        public static readonly Brush shamanPrimaryBrush = new SolidColorBrush(Color.FromRgb(0, 112, 222));
        public static readonly Brush shamanSecondaryBrush = new SolidColorBrush(Color.FromRgb(0, 0, 255));

        public static readonly Brush warlockPrimaryBrush = new SolidColorBrush(Color.FromRgb(148, 130, 201));
        public static readonly Brush warlockSecondaryBrush = new SolidColorBrush(Color.FromRgb(0, 0, 255));

        public static readonly Brush warriorPrimaryBrush = new SolidColorBrush(Color.FromRgb(199, 156, 110));
        public static readonly Brush warriorSecondaryBrush = new SolidColorBrush(Color.FromRgb(255, 0, 0));

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
                _ => new SolidColorBrush(Colors.White),
            };
        }
    }
}