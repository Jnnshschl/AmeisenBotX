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

#pragma warning disable IDE0051
        private const int deadPartymembersCheckTime = 4;
        private const string devouringPlagueSpell = "Devouring Plague";
        private const string flashHealSpell = "Flash Heal";
        private const string hymnOfHopeSpell = "Hymn of Hope";
        private const string mindBlastSpell = "Mind Blast";
        private const string mindFlaySpell = "Mind Flay";
        private const string powerWordFortitudeSpell = "Power Word: Fortitude";
        private const string resurrectionSpell = "Resurrection";
        private const string shadowfiendSpell = "Shadowfiend";
        private const string shadowformSpell = "Shadowform";
        private const string shadowWordPainSpell = "Shadow Word: Pain";
        private const string vampiricEmbraceSpell = "Vampiric Embrace";
        private const string vampiricTouchSpell = "Vampiric Touch";
#pragma warning restore IDE0051

        public PriestShadow(WowInterface wowInterface, AmeisenBotStateMachine stateMachine) : base(wowInterface, stateMachine)
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

            GroupAuraManager.SpellsToKeepActiveOnParty.Add((powerWordFortitudeSpell, (spellName, guid) => CastSpellIfPossible(spellName, guid, true)));
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
            if (TargetAuraManager.Tick())
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
            if (MyAuraManager.Tick()
                || GroupAuraManager.Tick()
                || GroupAuraManager.Tick())
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