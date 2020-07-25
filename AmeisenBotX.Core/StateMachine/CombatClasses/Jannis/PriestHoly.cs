using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Statemachine.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using static AmeisenBotX.Core.Statemachine.Utils.AuraManager;

namespace AmeisenBotX.Core.Statemachine.CombatClasses.Jannis
{
    public class PriestHoly : BasicCombatClass
    {
        // author: Jannis Höschele

#pragma warning disable IDE0051
        private const string bindingHealSpell = "Binding Heal";
        private const int deadPartymembersCheckTime = 4;
        private const string flashHealSpell = "Flash Heal";
        private const string healSpell = "Heal";
        private const string greaterHealSpell = "Greater Heal";
        private const string guardianSpiritSpell = "Guardian Spirit";
        private const string hymnOfHopeSpell = "Hymn of Hope";
        private const string innerFireSpell = "Inner Fire";
        private const string powerWordFortitudeSpell = "Power Word: Fortitude";
        private const string prayerOfHealingSpell = "Prayer of Healing";
        private const string prayerOfMendingSpell = "Prayer of Mending";
        private const string renewSpell = "Renew";
        private const string resurrectionSpell = "Resurrection";
        private const string smiteSpell = "Smite";
        private const string shadowWordPainSpell = "Shadow Word: Pain";
#pragma warning restore IDE0051

        public PriestHoly(WowInterface wowInterface, AmeisenBotStateMachine stateMachine) : base(wowInterface, stateMachine)
        {
            UseDefaultTargetSelection = false;

            MyAuraManager.BuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { powerWordFortitudeSpell, () => CastSpellIfPossible(powerWordFortitudeSpell, WowInterface.ObjectManager.PlayerGuid, true) },
                { innerFireSpell, () => CastSpellIfPossible(innerFireSpell, 0, true) }
            };

            SpellUsageHealDict = new Dictionary<int, string>()
            {
                { 0, healSpell },
                { 300, flashHealSpell },
                { 5000, greaterHealSpell },
            };

            GroupAuraManager.SpellsToKeepActiveOnParty.Add((powerWordFortitudeSpell, (spellName, guid) => CastSpellIfPossible(spellName, guid, true)));
        }

        public override string Author => "Jannis";

        public override WowClass Class => WowClass.Priest;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "FCFS based CombatClass for the Holy Priest spec.";

        public override string Displayname => "Priest Holy";

        public override bool HandlesMovement => false;

        public override bool IsMelee => false;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicSpiritComparator(new List<ArmorType>() { ArmorType.SHIELDS }, new List<WeaponType>() { WeaponType.ONEHANDED_SWORDS, WeaponType.ONEHANDED_MACES, WeaponType.ONEHANDED_AXES });

        public override CombatClassRole Role => CombatClassRole.Heal;

        public override string Version => "1.0";

        private DateTime LastDeadPartymembersCheck { get; set; }

        private Dictionary<int, string> SpellUsageHealDict { get; }

        public override TalentTree Talents { get; } = new TalentTree()
        {
            Tree1 = new Dictionary<int, Talent>()
            {
                { 2, new Talent(1, 2, 5) },
                { 4, new Talent(1, 4, 3) },
                { 5, new Talent(1, 5, 2) },
                { 7, new Talent(1, 7, 3) },
                { 8, new Talent(1, 8, 1) },
            },
            Tree2 = new Dictionary<int, Talent>()
            {
                { 1, new Talent(2, 1, 2) },
                { 2, new Talent(2, 2, 3) },
                { 3, new Talent(2, 3, 5) },
                { 5, new Talent(2, 5, 5) },
                { 8, new Talent(2, 8, 3) },
                { 9, new Talent(2, 9, 2) },
                { 10, new Talent(2, 10, 3) },
                { 12, new Talent(2, 12, 2) },
                { 13, new Talent(2, 13, 1) },
                { 14, new Talent(2, 14, 5) },
                { 15, new Talent(2, 15, 2) },
                { 16, new Talent(2, 16, 5) },
                { 17, new Talent(2, 17, 3) },
                { 22, new Talent(2, 22, 3) },
                { 23, new Talent(2, 23, 3) },
                { 24, new Talent(2, 24, 1) },
                { 25, new Talent(2, 25, 3) },
                { 26, new Talent(2, 26, 5) },
                { 27, new Talent(2, 27, 1) },
            },
            Tree3 = new Dictionary<int, Talent>(),
        };

        public override bool UseAutoAttacks => false;

        public override bool WalkBehindEnemy => false;

        public override void ExecuteCC()
        {
            if (!NeedToHealSomeone())
            {
                if (WowInterface.ObjectManager.Player.ManaPercentage > 40)
                {
                    List<WowUnit> nearEnemies = WowInterface.ObjectManager.GetEnemiesInCombatWithUs(WowInterface.ObjectManager.Player.Position, 40.0)
                        .OrderBy(e => e.HealthPercentage)
                        .ToList();

                    if (nearEnemies.Count > 0)
                    {
                        if (WowInterface.ObjectManager.Target.HasBuffByName(shadowWordPainSpell)
                            && CastSpellIfPossible(shadowWordPainSpell, nearEnemies.First().Guid, true))
                        {
                            return;
                        }

                        if (CastSpellIfPossible(smiteSpell, nearEnemies.First().Guid, true))
                        {
                            return;
                        }
                    }
                }
            }
        }

        private bool NeedToHealSomeone()
        {
            if (TargetManager.GetUnitToTarget(out List<WowUnit> unitsToHeal))
            {
                if (unitsToHeal.Count == 0)
                {
                    return false;
                }

                if (unitsToHeal.Count > 3
                    && CastSpellIfPossible(prayerOfHealingSpell, WowInterface.ObjectManager.TargetGuid, true))
                {
                    return true;
                }

                if (WowInterface.ObjectManager.Target.HealthPercentage < 25
                    && CastSpellIfPossible(guardianSpiritSpell, WowInterface.ObjectManager.TargetGuid, true))
                {
                    return true;
                }

                if (WowInterface.ObjectManager.Target.HealthPercentage < 70
                    && WowInterface.ObjectManager.Player.HealthPercentage < 70
                    && CastSpellIfPossible(bindingHealSpell, WowInterface.ObjectManager.TargetGuid, true))
                {
                    return true;
                }

                if (WowInterface.ObjectManager.Player.ManaPercentage < 50
                    && CastSpellIfPossible(hymnOfHopeSpell, 0))
                {
                    return true;
                }

                double healthDifference = WowInterface.ObjectManager.Target.MaxHealth - WowInterface.ObjectManager.Target.Health;
                List<KeyValuePair<int, string>> spellsToTry = SpellUsageHealDict.Where(e => e.Key <= healthDifference).ToList();

                foreach (KeyValuePair<int, string> keyValuePair in spellsToTry.OrderByDescending(e => e.Value))
                {
                    if (CastSpellIfPossible(keyValuePair.Value, WowInterface.ObjectManager.TargetGuid, true))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public override void OutOfCombatExecute()
        {
            if (MyAuraManager.Tick()
                || GroupAuraManager.Tick()
                || NeedToHealSomeone()
                || (DateTime.Now - LastDeadPartymembersCheck > TimeSpan.FromSeconds(deadPartymembersCheckTime)
                && HandleDeadPartymembers(resurrectionSpell)))
            {
                return;
            }
        }
    }
}