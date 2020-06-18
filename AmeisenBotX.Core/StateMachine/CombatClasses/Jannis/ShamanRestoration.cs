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
    public class ShamanRestoration : BasicCombatClass
    {
        // author: Jannis Höschele

#pragma warning disable IDE0051
        private const string ancestralSpiritSpell = "Ancestral Spirit";
        private const string chainHealSpell = "Chain Heal";
        private const int deadPartymembersCheckTime = 4;
        private const string earthlivingBuff = "Earthliving ";
        private const string earthlivingWeaponSpell = "Earthliving Weapon";
        private const string earthShieldSpell = "Earth Shield";
        private const string healingWaveSpell = "Healing Wave";
        private const string naturesSwiftnessSpell = "Nature's Swiftness";
        private const string riptideSpell = "Riptide";
        private const string tidalForceSpell = "Tidal Force";
        private const string waterShieldSpell = "Water Shield";
#pragma warning restore IDE0051

        public ShamanRestoration(WowInterface wowInterface, AmeisenBotStateMachine stateMachine) : base(wowInterface, stateMachine)
        {
            UseDefaultTargetSelection = false;

            MyAuraManager.BuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { waterShieldSpell, () => CastSpellIfPossible(waterShieldSpell, 0, true) }
            };

            SpellUsageHealDict = new Dictionary<int, string>()
            {
                { 0, riptideSpell },
                { 5000, healingWaveSpell },
            };
        }

        public override string Author => "Jannis";

        public override WowClass Class => WowClass.Shaman;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "FCFS based CombatClass for the Restoration Shaman spec.";

        public override string Displayname => "Shaman Restoration";

        public override bool HandlesMovement => false;

        public override bool IsMelee => false;

        public override bool UseAutoAttacks => false;

        public override bool WalkBehindEnemy => false;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicSpiritComparator(new List<ArmorType>() { ArmorType.SHIELDS });

        public override CombatClassRole Role => CombatClassRole.Heal;

        public override string Version => "1.0";

        private DateTime LastDeadPartymembersCheck { get; set; }

        private Dictionary<int, string> SpellUsageHealDict { get; }

        public override TalentTree Talents { get; } = new TalentTree()
        {
            Tree1 = new Dictionary<int, Talent>(),
            Tree2 = new Dictionary<int, Talent>()
            {
                { 3, new Talent(2, 3, 5) },
                { 5, new Talent(2, 5, 5) },
                { 7, new Talent(2, 7, 3) },
                { 8, new Talent(2, 8, 1) },
            },
            Tree3 = new Dictionary<int, Talent>()
            {
                { 1, new Talent(3, 1, 5) },
                { 5, new Talent(3, 5, 5) },
                { 6, new Talent(3, 6, 3) },
                { 7, new Talent(3, 7, 3) },
                { 8, new Talent(3, 8, 1) },
                { 9, new Talent(3, 9, 3) },
                { 10, new Talent(3, 10, 3) },
                { 11, new Talent(3, 11, 5) },
                { 12, new Talent(3, 12, 3) },
                { 13, new Talent(3, 13, 1) },
                { 15, new Talent(3, 15, 5) },
                { 17, new Talent(3, 17, 1) },
                { 19, new Talent(3, 19, 2) },
                { 20, new Talent(3, 20, 2) },
                { 21, new Talent(3, 21, 3) },
                { 22, new Talent(3, 22, 3) },
                { 23, new Talent(3, 23, 1) },
                { 24, new Talent(3, 24, 2) },
                { 25, new Talent(3, 25, 5) },
                { 26, new Talent(3, 26, 1) },
            },
        };

        public override void ExecuteCC()
        {
            if (NeedToHealSomeone())
            {
                return;
            }
        }

        private bool NeedToHealSomeone()
        {
            if (TargetManager.GetUnitToTarget(out List<WowUnit> unitsToHeal))
            {
                WowInterface.HookManager.TargetGuid(unitsToHeal.First().Guid);
                WowInterface.ObjectManager.UpdateObject(WowInterface.ObjectManager.Player);

                if (WowInterface.ObjectManager.Target != null)
                {
                    if (WowInterface.ObjectManager.Target.HealthPercentage < 25
                        && CastSpellIfPossible(earthShieldSpell, 0, true))
                    {
                        return true;
                    }

                    if (unitsToHeal.Count > 4
                        && CastSpellIfPossible(chainHealSpell, WowInterface.ObjectManager.TargetGuid, true))
                    {
                        return true;
                    }

                    if (unitsToHeal.Count > 6
                        && (CastSpellIfPossible(naturesSwiftnessSpell, 0, true)
                        || CastSpellIfPossible(tidalForceSpell, WowInterface.ObjectManager.TargetGuid, true)))
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
            }

            return false;
        }

        public override void OutOfCombatExecute()
        {
            if (MyAuraManager.Tick()
                || NeedToHealSomeone()
                || (DateTime.Now - LastDeadPartymembersCheck > TimeSpan.FromSeconds(deadPartymembersCheckTime)
                && HandleDeadPartymembers(ancestralSpiritSpell)))
            {
                return;
            }

            if (CheckForWeaponEnchantment(EquipmentSlot.INVSLOT_MAINHAND, earthlivingBuff, earthlivingWeaponSpell))
            {
                return;
            }
        }
    }
}