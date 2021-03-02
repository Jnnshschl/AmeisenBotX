using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Fsm;
using AmeisenBotX.Core.Fsm.Utils.Auras.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Combat.Classes.Jannis
{
    public class ShamanRestoration : BasicCombatClass
    {
        public ShamanRestoration(WowInterface wowInterface, AmeisenBotFsm stateMachine) : base(wowInterface, stateMachine)
        {
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(waterShieldSpell, () => TryCastSpell(waterShieldSpell, 0, true)));

            SpellUsageHealDict = new Dictionary<int, string>()
            {
                { 0, riptideSpell },
                { 5000, healingWaveSpell },
            };
        }

        public override string Description => "FCFS based CombatClass for the Restoration Shaman spec.";

        public override string Displayname => "Shaman Restoration";

        public override bool HandlesMovement => false;

        public override bool IsMelee => false;

        public override IItemComparator ItemComparator { get; set; } = new BasicSpiritComparator(new() { WowArmorType.SHIELDS });

        public override WowRole Role => WowRole.Heal;

        public override TalentTree Talents { get; } = new()
        {
            Tree1 = new(),
            Tree2 = new()
            {
                { 3, new(2, 3, 5) },
                { 5, new(2, 5, 5) },
                { 7, new(2, 7, 3) },
                { 8, new(2, 8, 1) },
            },
            Tree3 = new()
            {
                { 1, new(3, 1, 5) },
                { 5, new(3, 5, 5) },
                { 6, new(3, 6, 3) },
                { 7, new(3, 7, 3) },
                { 8, new(3, 8, 1) },
                { 9, new(3, 9, 3) },
                { 10, new(3, 10, 3) },
                { 11, new(3, 11, 5) },
                { 12, new(3, 12, 3) },
                { 13, new(3, 13, 1) },
                { 15, new(3, 15, 5) },
                { 17, new(3, 17, 1) },
                { 19, new(3, 19, 2) },
                { 20, new(3, 20, 2) },
                { 21, new(3, 21, 3) },
                { 22, new(3, 22, 3) },
                { 23, new(3, 23, 1) },
                { 24, new(3, 24, 2) },
                { 25, new(3, 25, 5) },
                { 26, new(3, 26, 1) },
            },
        };

        public override bool UseAutoAttacks => false;

        public override string Version => "1.0";

        public override bool WalkBehindEnemy => false;

        public override WowClass WowClass => WowClass.Shaman;

        private DateTime LastDeadPartymembersCheck { get; set; }

        private Dictionary<int, string> SpellUsageHealDict { get; }

        public override void Execute()
        {
            base.Execute();

            if (NeedToHealSomeone())
            {
                return;
            }

            if (SelectTarget(TargetManagerDps))
            {
                if (WowInterface.Target.HasBuffByName(flameShockSpell)
                    && TryCastSpell(flameShockSpell, WowInterface.TargetGuid, true))
                {
                    return;
                }

                if (TryCastSpell(lightningBoltSpell, WowInterface.TargetGuid, true))
                {
                    return;
                }
            }
        }

        public override void OutOfCombatExecute()
        {
            base.OutOfCombatExecute();

            if (HandleDeadPartymembers(ancestralSpiritSpell))
            {
                return;
            }

            if (CheckForWeaponEnchantment(WowEquipmentSlot.INVSLOT_MAINHAND, earthlivingBuff, earthlivingWeaponSpell))
            {
                return;
            }
        }

        private bool NeedToHealSomeone()
        {
            if (TargetManagerHeal.GetUnitToTarget(out IEnumerable<WowUnit> unitsToHeal))
            {
                WowInterface.HookManager.WowTargetGuid(unitsToHeal.First().Guid);

                if (WowInterface.Target != null)
                {
                    if (WowInterface.Target.HealthPercentage < 25
                        && TryCastSpell(earthShieldSpell, 0, true))
                    {
                        return true;
                    }

                    if (unitsToHeal.Count() > 4
                        && TryCastSpell(chainHealSpell, WowInterface.TargetGuid, true))
                    {
                        return true;
                    }

                    if (unitsToHeal.Count() > 6
                        && (TryCastSpell(naturesSwiftnessSpell, 0, true)
                        || TryCastSpell(tidalForceSpell, WowInterface.TargetGuid, true)))
                    {
                        return true;
                    }

                    double healthDifference = WowInterface.Target.MaxHealth - WowInterface.Target.Health;
                    List<KeyValuePair<int, string>> spellsToTry = SpellUsageHealDict.Where(e => e.Key <= healthDifference).ToList();

                    foreach (KeyValuePair<int, string> keyValuePair in spellsToTry.OrderByDescending(e => e.Value))
                    {
                        if (TryCastSpell(keyValuePair.Value, WowInterface.TargetGuid, true))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}