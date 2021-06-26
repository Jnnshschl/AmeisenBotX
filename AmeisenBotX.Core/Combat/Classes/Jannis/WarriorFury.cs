using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Fsm;
using AmeisenBotX.Core.Fsm.Utils.Auras.Objects;
using System;
using System.Linq;

namespace AmeisenBotX.Core.Combat.Classes.Jannis
{
    public class WarriorFury : BasicCombatClass
    {
        public WarriorFury(WowInterface wowInterface, AmeisenBotFsm stateMachine) : base(wowInterface, stateMachine)
        {
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(battleShoutSpell, () => TryCastSpell(battleShoutSpell, 0, true)));

            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(hamstringSpell, () => WowInterface.Target?.Type == WowObjectType.Player && TryCastSpell(hamstringSpell, WowInterface.TargetGuid, true)));
            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(rendSpell, () => WowInterface.Target?.Type == WowObjectType.Player && WowInterface.Player.Rage > 75 && TryCastSpell(rendSpell, WowInterface.TargetGuid, true)));

            InterruptManager.InterruptSpells = new()
            {
                { 0, (x) => TryCastSpellWarrior(intimidatingShoutSpell, berserkerStanceSpell, x.Guid, true) },
                { 1, (x) => TryCastSpellWarrior(intimidatingShoutSpell, battleStanceSpell, x.Guid, true) }
            };

            HeroicStrikeEvent = new(TimeSpan.FromSeconds(2));
        }

        public override string Description => "FCFS based CombatClass for the Fury Warrior spec.";

        public override string Displayname => "Warrior Fury";

        public override bool HandlesMovement => false;

        public override bool IsMelee => true;

        public override IItemComparator ItemComparator { get; set; } = new BasicStrengthComparator(new() { WowArmorType.SHIELDS }, new() { WowWeaponType.ONEHANDED_SWORDS, WowWeaponType.ONEHANDED_MACES, WowWeaponType.ONEHANDED_AXES });

        public override WowRole Role => WowRole.Dps;

        public override TalentTree Talents { get; } = new()
        {
            Tree1 = new()
            {
                { 1, new(1, 1, 3) },
                { 3, new(1, 3, 2) },
                { 5, new(1, 5, 2) },
                { 6, new(1, 6, 3) },
                { 9, new(1, 9, 2) },
                { 10, new(1, 10, 3) },
                { 11, new(1, 11, 3) },
            },
            Tree2 = new()
            {
                { 1, new(2, 1, 3) },
                { 3, new(2, 3, 5) },
                { 5, new(2, 5, 5) },
                { 6, new(2, 6, 3) },
                { 10, new(2, 10, 5) },
                { 13, new(2, 13, 3) },
                { 14, new(2, 14, 1) },
                { 16, new(2, 16, 1) },
                { 17, new(2, 17, 5) },
                { 18, new(2, 18, 3) },
                { 19, new(2, 19, 1) },
                { 20, new(2, 20, 2) },
                { 22, new(2, 22, 5) },
                { 23, new(2, 23, 1) },
                { 24, new(2, 24, 1) },
                { 25, new(2, 25, 3) },
                { 26, new(2, 26, 5) },
                { 27, new(2, 27, 1) },
            },
            Tree3 = new(),
        };

        public override bool UseAutoAttacks => true;

        public override string Version => "1.0";

        public override bool WalkBehindEnemy => false;

        public override WowClass WowClass => WowClass.Warrior;

        private TimegatedEvent HeroicStrikeEvent { get; }

        public override void Execute()
        {
            base.Execute();

            if (SelectTarget(TargetProviderDps))
            {
                if (WowInterface.Target != null)
                {
                    double distanceToTarget = WowInterface.Target.Position.GetDistance(WowInterface.Player.Position);

                    if ((WowInterface.Player.IsDazed
                        || WowInterface.Player.IsConfused
                        || WowInterface.Player.IsPossessed
                        || WowInterface.Player.IsFleeing)
                        && TryCastSpell(heroicFurySpell, 0))
                    {
                        return;
                    }

                    if (distanceToTarget > 4.0)
                    {
                        if (TryCastSpellWarrior(chargeSpell, battleStanceSpell, WowInterface.Target.Guid, true)
                            || (TryCastSpell(berserkerRageSpell, WowInterface.Target.Guid, true) && TryCastSpellWarrior(interceptSpell, berserkerStanceSpell, WowInterface.Target.Guid, true)))
                        {
                            return;
                        }
                    }
                    else
                    {
                        if (HeroicStrikeEvent.Ready && !WowInterface.Player.HasBuffByName(recklessnessSpell))
                        {
                            if ((WowInterface.Player.Rage > 50 && WowInterface.ObjectManager.GetNearEnemies<WowUnit>(WowInterface.Player.Position, 8.0f).Count() > 2 && TryCastSpellWarrior(cleaveSpell, berserkerStanceSpell, 0, true))
                                || (WowInterface.Player.Rage > 50 && TryCastSpellWarrior(heroicStrikeSpell, berserkerStanceSpell, WowInterface.TargetGuid, true)))
                            {
                                HeroicStrikeEvent.Run();
                                return;
                            }
                        }

                        if (TryCastSpellWarrior(bloodthirstSpell, berserkerStanceSpell, WowInterface.Target.Guid, true)
                            || TryCastSpellWarrior(whirlwindSpell, berserkerStanceSpell, WowInterface.Target.Guid, true))
                        {
                            return;
                        }

                        // dont prevent BT or WW with GCD
                        if (CooldownManager.GetSpellCooldown(bloodthirstSpell) <= 1200
                            || CooldownManager.GetSpellCooldown(whirlwindSpell) <= 1200)
                        {
                            return;
                        }

                        if (WowInterface.Player.HasBuffByName($"{slamSpell}!")
                           && TryCastSpell(slamSpell, WowInterface.Target.Guid, true))
                        {
                            return;
                        }

                        if (TryCastSpell(berserkerRageSpell, 0))
                        {
                            return;
                        }

                        if (TryCastSpell(bloodrageSpell, WowInterface.Target.Guid, true, WowInterface.Player.Health))
                        {
                            return;
                        }

                        if (TryCastSpell(recklessnessSpell, WowInterface.Target.Guid, true))
                        {
                            return;
                        }

                        if (TryCastSpell(deathWishSpell, WowInterface.Target.Guid, true))
                        {
                            return;
                        }

                        if (WowInterface.Player.Rage > 25
                           && WowInterface.Target.HealthPercentage < 20
                           && TryCastSpellWarrior(executeSpell, berserkerStanceSpell, WowInterface.Target.Guid, true))
                        {
                            return;
                        }
                    }
                }
            }
        }

        public override void OutOfCombatExecute()
        {
            base.OutOfCombatExecute();
        }
    }
}