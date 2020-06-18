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
    public class PriestDiscipline : BasicCombatClass
    {
        // author: Jannis Höschele

#pragma warning disable IDE0051
        private const string bindingHealSpell = "Binding Heal";
        private const int deadPartymembersCheckTime = 4;
        private const string desperatePrayerSpell = "Desperate Prayer";
        private const string flashHealSpell = "Flash Heal";
        private const string greaterHealSpell = "Greater Heal";
        private const string hymnOfHopeSpell = "Hymn of Hope";
        private const string innerFireSpell = "Inner Fire";
        private const string penanceSpell = "Penance";
        private const string powerWordFortitudeSpell = "Power Word: Fortitude";
        private const string powerWordShieldSpell = "Power Word: Shield";
        private const string prayerOfHealingSpell = "Prayer of Healing";
        private const string prayerOfMendingSpell = "Prayer of Mending";
        private const string renewSpell = "Renew";
        private const string resurrectionSpell = "Resurrection";
        private const string weakenedSoulSpell = "Weakened Soul";
#pragma warning restore IDE0051

        public PriestDiscipline(WowInterface wowInterface, AmeisenBotStateMachine stateMachine) : base(wowInterface, stateMachine)
        {
            UseDefaultTargetSelection = false;

            MyAuraManager.BuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { powerWordFortitudeSpell, () => CastSpellIfPossible(powerWordFortitudeSpell, WowInterface.ObjectManager.PlayerGuid, true) },
                { innerFireSpell, () => CastSpellIfPossible(innerFireSpell, 0, true) }
            };

            SpellUsageHealDict = new Dictionary<int, string>()
            {
                { 0, flashHealSpell },
                { 3000, penanceSpell },
                { 5000, greaterHealSpell },
            };

            GroupAuraManager.SpellsToKeepActiveOnParty.Add((powerWordFortitudeSpell, (spellName, guid) => CastSpellIfPossible(spellName, guid, true)));
        }

        public override string Author => "Jannis";

        public override WowClass Class => WowClass.Priest;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "FCFS based CombatClass for the Discipline Priest spec.";

        public override string Displayname => "Priest Discipline";

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
                { 9, new Talent(1, 9, 3) },
                { 11, new Talent(1, 11, 3) },
                { 14, new Talent(1, 14, 5) },
                { 15, new Talent(1, 15, 1) },
                { 16, new Talent(1, 16, 2) },
                { 17, new Talent(1, 17, 3) },
                { 18, new Talent(1, 18, 3) },
                { 19, new Talent(1, 19, 1) },
                { 20, new Talent(1, 20, 3) },
                { 21, new Talent(1, 21, 2) },
                { 22, new Talent(1, 22, 3) },
                { 23, new Talent(1, 23, 2) },
                { 24, new Talent(1, 24, 3) },
                { 25, new Talent(1, 25, 1) },
                { 26, new Talent(1, 26, 2) },
                { 27, new Talent(1, 27, 5) },
                { 28, new Talent(1, 28, 1) },
            },
            Tree2 = new Dictionary<int, Talent>()
            {
                { 3, new Talent(2, 3, 5) },
                { 4, new Talent(2, 4, 5) },
                { 6, new Talent(2, 6, 1) },
                { 8, new Talent(2, 8, 3) },
            },
            Tree3 = new Dictionary<int, Talent>(),
        };

        public override void ExecuteCC()
        {
            if (!NeedToHealSomeone())
            {
                return;
            }
        }

        public override bool WalkBehindEnemy => false;

        public override bool UseAutoAttacks => false;

        private bool NeedToHealSomeone()
        {
            if (TargetManager.GetUnitToTarget(out List<WowUnit> unitsToHeal))
            {
                WowInterface.HookManager.TargetGuid(unitsToHeal.First().Guid);
                WowInterface.ObjectManager.UpdateObject(WowInterface.ObjectManager.Player);

                if (unitsToHeal.Count > 3
                    && CastSpellIfPossible(prayerOfHealingSpell, WowInterface.ObjectManager.TargetGuid, true))
                {
                    return true;
                }

                WowUnit target = WowInterface.ObjectManager.Target;
                if (target != null)
                {
                    WowInterface.ObjectManager.UpdateObject(target);

                    if (target.Guid != WowInterface.ObjectManager.PlayerGuid
                        && target.HealthPercentage < 70
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

                    if (WowInterface.ObjectManager.Player.HealthPercentage < 20
                        && CastSpellIfPossible(desperatePrayerSpell, 0))
                    {
                        return true;
                    }

                    List<string> targetBuffs = WowInterface.HookManager.GetAuras(WowLuaUnit.Target);

                    if ((target.HealthPercentage < 98 && target.HealthPercentage > 80
                            && !WowInterface.ObjectManager.Target.HasBuffByName(weakenedSoulSpell)
                            && !WowInterface.ObjectManager.Target.HasBuffByName(powerWordShieldSpell)
                            && CastSpellIfPossible(powerWordShieldSpell, WowInterface.ObjectManager.TargetGuid, true))
                        || (target.HealthPercentage < 90 && target.HealthPercentage > 80
                            && !WowInterface.ObjectManager.Target.HasBuffByName(renewSpell)
                            && CastSpellIfPossible(renewSpell, WowInterface.ObjectManager.TargetGuid, true)))
                    {
                        return true;
                    }

                    double healthDifference = target.MaxHealth - target.Health;
                    List<KeyValuePair<int, string>> spellsToTry = SpellUsageHealDict.Where(e => e.Key <= healthDifference).ToList();

                    foreach (KeyValuePair<int, string> keyValuePair in spellsToTry.OrderByDescending(e => e.Value))
                    {
                        if (CastSpellIfPossible(keyValuePair.Value, WowInterface.ObjectManager.TargetGuid, true))
                        {
                            return true;
                        }
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