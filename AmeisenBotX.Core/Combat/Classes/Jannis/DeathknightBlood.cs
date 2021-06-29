using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Fsm;
using AmeisenBotX.Core.Fsm.Utils.Auras.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Linq;

namespace AmeisenBotX.Core.Combat.Classes.Jannis
{
    public class DeathknightBlood : BasicCombatClass
    {
        public DeathknightBlood(WowInterface wowInterface, AmeisenBotFsm stateMachine) : base(wowInterface, stateMachine)
        {
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bloodPresenceSpell, () => TryCastSpellDk(bloodPresenceSpell, 0)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(hornOfWinterSpell, () => TryCastSpellDk(hornOfWinterSpell, 0, true)));

            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(frostFeverSpell, () => TryCastSpellDk(icyTouchSpell, WowInterface.Target.Guid, false, false, false, true)));
            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(bloodPlagueSpell, () => TryCastSpellDk(plagueStrikeSpell, WowInterface.Target.Guid, false, false, false, true)));

            InterruptManager.InterruptSpells = new()
            {
                { 0, (x) => TryCastSpellDk(mindFreezeSpell, x.Guid, true) },
                { 1, (x) => TryCastSpellDk(strangulateSpell, x.Guid, false, true) }
            };

            BloodBoilEvent = new(TimeSpan.FromSeconds(2));
        }

        public override string Description => "FCFS based CombatClass for the Blood Deathknight spec.";

        public override string Displayname => "Deathknight Blood";

        public override bool HandlesMovement => false;

        public override bool IsMelee => true;

        public override IItemComparator ItemComparator { get; set; } = new BasicStrengthComparator(new() { WowArmorType.SHIELDS });

        public override WowRole Role => WowRole.Dps;

        public override TalentTree Talents { get; } = new()
        {
            Tree1 = new()
            {
                { 2, new(1, 2, 3) },
                { 3, new(1, 3, 5) },
                { 4, new(1, 4, 5) },
                { 5, new(1, 5, 2) },
                { 6, new(1, 6, 2) },
                { 7, new(1, 7, 1) },
                { 8, new(1, 8, 5) },
                { 9, new(1, 9, 3) },
                { 13, new(1, 13, 3) },
                { 14, new(1, 14, 3) },
                { 16, new(1, 16, 3) },
                { 17, new(1, 17, 2) },
                { 18, new(1, 18, 3) },
                { 19, new(1, 19, 1) },
                { 21, new(1, 21, 2) },
                { 23, new(1, 23, 1) },
                { 24, new(1, 24, 3) },
                { 25, new(1, 25, 1) },
                { 26, new(1, 26, 3) },
                { 27, new(1, 27, 5) },
            },
            Tree2 = new()
            {
                { 1, new(2, 1, 3) },
                { 3, new(2, 3, 5) },
            },
            Tree3 = new()
            {
                { 3, new(3, 3, 5) },
                { 4, new(3, 4, 2) },
            },
        };

        public override bool UseAutoAttacks => true;

        public override string Version => "1.0";

        public override bool WalkBehindEnemy => false;

        public override WowClass WowClass => WowClass.Deathknight;

        private TimegatedEvent BloodBoilEvent { get; }

        public override void Execute()
        {
            base.Execute();

            if (SelectTarget(TargetProviderDps))
            {
                if (WowInterface.Target.TargetGuid != WowInterface.Player.Guid
                    && TryCastSpellDk(darkCommandSpell, WowInterface.Target.Guid))
                {
                    return;
                }

                if (WowInterface.Target.Position.GetDistance(WowInterface.Player.Position) > 6.0
                    && TryCastSpellDk(deathGripSpell, WowInterface.Target.Guid, false, false, true))
                {
                    return;
                }

                if (!WowInterface.Target.HasBuffByName(chainsOfIceSpell)
                    && WowInterface.Target.Position.GetDistance(WowInterface.Player.Position) > 2.0
                    && TryCastSpellDk(chainsOfIceSpell, WowInterface.Target.Guid, false, false, true))
                {
                    return;
                }

                if (TryCastSpellDk(empowerRuneWeapon, 0))
                {
                    return;
                }

                int nearEnemies = WowInterface.Objects.GetNearEnemies<WowUnit>(WowInterface.Db.GetReaction, WowInterface.Player.Position, 12.0f).Count();

                if ((WowInterface.Player.HealthPercentage < 70.0 && TryCastSpellDk(runeTapSpell, 0, false, false, true))
                    || (WowInterface.Player.HealthPercentage < 60.0 && (TryCastSpellDk(iceboundFortitudeSpell, 0, true) || TryCastSpellDk(antiMagicShellSpell, 0, true)))
                    || (WowInterface.Player.HealthPercentage < 50.0 && TryCastSpellDk(vampiricBloodSpell, 0, false, false, true))
                    || (nearEnemies > 2 && (TryCastAoeSpellDk(deathAndDecaySpell, 0) || (BloodBoilEvent.Run() && TryCastSpellDk(bloodBoilSpell, 0))))
                    || TryCastSpellDk(unbreakableArmorSpell, 0, false, false, true)
                    || TryCastSpellDk(deathStrike, WowInterface.Target.Guid, false, false, true, true)
                    || TryCastSpellDk(heartStrikeSpell, WowInterface.Target.Guid, false, false, true)
                    || TryCastSpellDk(deathCoilSpell, WowInterface.Target.Guid, true))
                {
                    return;
                }
            }
        }

        public override void OutOfCombatExecute()
        {
            base.OutOfCombatExecute();
        }
    }
}