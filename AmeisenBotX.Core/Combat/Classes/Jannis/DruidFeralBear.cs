﻿using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Fsm;
using AmeisenBotX.Core.Fsm.Utils.Auras.Objects;
using System.Linq;

namespace AmeisenBotX.Core.Combat.Classes.Jannis
{
    public class DruidFeralBear : BasicCombatClass
    {
        public DruidFeralBear(WowInterface wowInterface, AmeisenBotFsm stateMachine) : base(wowInterface, stateMachine)
        {
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(markOfTheWildSpell, () => TryCastSpell(markOfTheWildSpell, WowInterface.Player.Guid, true, 0, true)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(direBearFormSpell, () => TryCastSpell(direBearFormSpell, 0, true)));

            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(mangleBearSpell, () => TryCastSpell(mangleBearSpell, WowInterface.Target.Guid, true)));

            InterruptManager.InterruptSpells = new()
            {
                { 0, (x) => TryCastSpell(bashSpell, x.Guid, true) },
            };

            GroupAuraManager.SpellsToKeepActiveOnParty.Add((markOfTheWildSpell, (spellName, guid) => TryCastSpell(spellName, guid, true)));
        }

        public override string Description => "FCFS based CombatClass for the Feral (Bear) Druid spec.";

        public override string Displayname => "Druid Feral Bear";

        public override bool HandlesMovement => false;

        public override bool IsMelee => true;

        public override IItemComparator ItemComparator { get; set; } = new BasicArmorComparator(new() { WowArmorType.SHIELDS }, new() { WowWeaponType.ONEHANDED_SWORDS, WowWeaponType.ONEHANDED_MACES, WowWeaponType.ONEHANDED_AXES });

        public override WowRole Role => WowRole.Tank;

        public override TalentTree Talents { get; } = new()
        {
            Tree1 = new(),
            Tree2 = new()
            {
                { 1, new(2, 1, 5) },
                { 3, new(2, 3, 1) },
                { 4, new(2, 4, 2) },
                { 5, new(2, 5, 3) },
                { 6, new(2, 6, 2) },
                { 7, new(2, 7, 1) },
                { 8, new(2, 8, 3) },
                { 10, new(2, 10, 3) },
                { 11, new(2, 11, 2) },
                { 12, new(2, 12, 2) },
                { 13, new(2, 13, 1) },
                { 14, new(2, 14, 1) },
                { 16, new(2, 16, 3) },
                { 17, new(2, 17, 5) },
                { 18, new(2, 18, 3) },
                { 19, new(2, 19, 1) },
                { 20, new(2, 20, 2) },
                { 22, new(2, 22, 3) },
                { 24, new(2, 24, 3) },
                { 25, new(2, 25, 3) },
                { 26, new(2, 26, 1) },
                { 27, new(2, 27, 3) },
                { 28, new(2, 28, 5) },
                { 29, new(2, 29, 1) },
                { 30, new(2, 30, 1) },
            },
            Tree3 = new()
            {
                { 1, new(3, 1, 2) },
                { 3, new(3, 3, 3) },
                { 4, new(3, 4, 5) },
                { 8, new(3, 8, 1) },
            },
        };

        public override bool UseAutoAttacks => true;

        public override string Version => "1.0";

        public override bool WalkBehindEnemy => false;

        public override WowClass WowClass => WowClass.Druid;

        public override void Execute()
        {
            base.Execute();

            if (SelectTarget(TargetProviderDps))
            {
                double distanceToTarget = WowInterface.Target.Position.GetDistance(WowInterface.Player.Position);

                if (distanceToTarget > 9.0
                    && TryCastSpell(feralChargeBearSpell, WowInterface.Target.Guid, true))
                {
                    return;
                }

                if (WowInterface.Player.HealthPercentage < 40
                    && TryCastSpell(survivalInstinctsSpell, 0, true))
                {
                    return;
                }

                if (WowInterface.Target.TargetGuid != WowInterface.Player.Guid
                    && TryCastSpell(growlSpell, 0, true))
                {
                    return;
                }

                if (TryCastSpell(berserkSpell, 0))
                {
                    return;
                }

                if (NeedToHealMySelf())
                {
                    return;
                }

                int nearEnemies = WowInterface.Objects.GetNearEnemies<WowUnit>(WowInterface.NewWowInterface, WowInterface.Player.Position, 10).Count();

                if ((WowInterface.Player.HealthPercentage > 80
                        && TryCastSpell(enrageSpell, 0, true))
                    || (WowInterface.Player.HealthPercentage < 70
                        && TryCastSpell(barkskinSpell, 0, true))
                    || (WowInterface.Player.HealthPercentage < 75
                        && TryCastSpell(frenziedRegenerationSpell, 0, true))
                    || (nearEnemies > 2 && TryCastSpell(challengingRoarSpell, 0, true))
                    || TryCastSpell(lacerateSpell, WowInterface.Target.Guid, true)
                    || (nearEnemies > 2 && TryCastSpell(swipeSpell, 0, true))
                    || TryCastSpell(mangleBearSpell, WowInterface.Target.Guid, true))
                {
                    return;
                }
            }
        }

        public override void OutOfCombatExecute()
        {
            base.OutOfCombatExecute();

            if (NeedToHealMySelf())
            {
                return;
            }
        }

        private bool NeedToHealMySelf()
        {
            if (WowInterface.Player.HealthPercentage < 60
                && !WowInterface.Player.HasBuffByName(rejuvenationSpell)
                && TryCastSpell(rejuvenationSpell, 0, true))
            {
                return true;
            }

            if (WowInterface.Player.HealthPercentage < 40
                && TryCastSpell(healingTouchSpell, 0, true))
            {
                return true;
            }

            return false;
        }
    }
}