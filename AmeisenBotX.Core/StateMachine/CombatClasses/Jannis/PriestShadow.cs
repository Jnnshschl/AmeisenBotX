using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Statemachine.Enums;
using System;
using System.Collections.Generic;
using static AmeisenBotX.Core.Statemachine.Utils.AuraManager;

namespace AmeisenBotX.Core.Statemachine.CombatClasses.Jannis
{
    public class PriestShadow : BasicCombatClass
    {
        // author: Jannis Höschele

        private readonly int deadPartymembersCheckTime = 4;
        private readonly string devouringPlagueSpell = "Devouring Plague";
        private readonly string flashHealSpell = "Flash Heal";
        private readonly string hymnOfHopeSpell = "Hymn of Hope";
        private readonly string mindBlastSpell = "Mind Blast";
        private readonly string mindFlaySpell = "Mind Flay";
        private readonly string powerWordFortitudeSpell = "Power Word: Fortitude";
        private readonly string resurrectionSpell = "Resurrection";
        private readonly string shadowfiendSpell = "Shadowfiend";
        private readonly string shadowformSpell = "Shadowform";
        private readonly string shadowWordPainSpell = "Shadow Word: Pain";
        private readonly string vampiricEmbraceSpell = "Vampiric Embrace";
        private readonly string vampiricTouchSpell = "Vampiric Touch";

        public PriestShadow(WowInterface wowInterface) : base(wowInterface)
        {
            MyAuraManager.BuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { shadowformSpell, () => CastSpellIfPossible(shadowformSpell, WowInterface.ObjectManager.PlayerGuid, true) },
                { powerWordFortitudeSpell, () => CastSpellIfPossible(powerWordFortitudeSpell, WowInterface.ObjectManager.PlayerGuid, true) },
                { vampiricEmbraceSpell, () => CastSpellIfPossible(vampiricEmbraceSpell, WowInterface.ObjectManager.PlayerGuid, true) }
            };

            TargetAuraManager.DebuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { vampiricTouchSpell, () => CastSpellIfPossible(vampiricTouchSpell, WowInterface.ObjectManager.TargetGuid, true) },
                { devouringPlagueSpell, () => CastSpellIfPossible(devouringPlagueSpell, WowInterface.ObjectManager.TargetGuid, true) },
                { shadowWordPainSpell, () => CastSpellIfPossible(shadowWordPainSpell, WowInterface.ObjectManager.TargetGuid, true) },
                { mindBlastSpell, () => CastSpellIfPossible(mindBlastSpell, WowInterface.ObjectManager.TargetGuid, true) }
            };
        }

        public override string Author => "Jannis";

        public override WowClass Class => WowClass.Priest;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "FCFS based CombatClass for the Shadow Priest spec.";

        public override string Displayname => "Priest Shadow";

        public override bool HandlesMovement => false;

        public override bool HandlesTargetSelection => false;

        public override bool IsMelee => false;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicIntellectComparator(new List<ArmorType>() { ArmorType.SHIEDLS }, new List<WeaponType>() { WeaponType.ONEHANDED_SWORDS, WeaponType.ONEHANDED_MACES, WeaponType.ONEHANDED_AXES });

        public override CombatClassRole Role => CombatClassRole.Dps;

        public override string Version => "1.0";

        private DateTime LastDeadPartymembersCheck { get; set; }

        public override void ExecuteCC()
        {
            if (MyAuraManager.Tick()
                || TargetAuraManager.Tick())
            {
                return;
            }

            if (WowInterface.ObjectManager.Player.ManaPercentage < 30
                && CastSpellIfPossible(hymnOfHopeSpell, 0))
            {
                return;
            }

            if (WowInterface.ObjectManager.Player.ManaPercentage < 90
                && CastSpellIfPossible(shadowfiendSpell, WowInterface.ObjectManager.TargetGuid))
            {
                return;
            }

            if (WowInterface.ObjectManager.Player.HealthPercentage < 70
                && CastSpellIfPossible(flashHealSpell, WowInterface.ObjectManager.TargetGuid))
            {
                return;
            }

            if (!WowInterface.ObjectManager.Player.IsCasting
                && CastSpellIfPossible(mindFlaySpell, WowInterface.ObjectManager.TargetGuid, true))
            {
                return;
            }
        }

        public override void OutOfCombatExecute()
        {
            if (MyAuraManager.Tick())
            {
                return;
            }

            if (DateTime.Now - LastDeadPartymembersCheck > TimeSpan.FromSeconds(deadPartymembersCheckTime)
                && HandleDeadPartymembers(resurrectionSpell))
            {
                return;
            }
        }
    }
}