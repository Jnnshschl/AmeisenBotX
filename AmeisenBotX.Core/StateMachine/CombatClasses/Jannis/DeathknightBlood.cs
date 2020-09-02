using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObjects;
using AmeisenBotX.Core.Statemachine.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using static AmeisenBotX.Core.Statemachine.Utils.AuraManager;
using static AmeisenBotX.Core.Statemachine.Utils.InterruptManager;

namespace AmeisenBotX.Core.Statemachine.CombatClasses.Jannis
{
    public class DeathknightBlood : BasicCombatClass
    {
        public DeathknightBlood(AmeisenBotStateMachine stateMachine) : base(stateMachine)
        {
            MyAuraManager.BuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { bloodPresenceSpell, () => TryCastSpellDk(bloodPresenceSpell, 0) },
                { hornOfWinterSpell, () => TryCastSpellDk(hornOfWinterSpell, 0, true) }
            };

            TargetAuraManager.DebuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { frostFeverSpell, () => TryCastSpellDk(icyTouchSpell, WowInterface.ObjectManager.TargetGuid, false, false, false, true) },
                { bloodPlagueSpell, () => TryCastSpellDk(plagueStrikeSpell, WowInterface.ObjectManager.TargetGuid, false, false, false, true) }
            };

            TargetInterruptManager.InterruptSpells = new SortedList<int, CastInterruptFunction>()
            {
                { 0, (x) => TryCastSpellDk(mindFreezeSpell, x.Guid, true) },
                { 1, (x) => TryCastSpellDk(strangulateSpell, x.Guid, false, true) }
            };

            BloodBoilEvent = new TimegatedEvent(TimeSpan.FromSeconds(2));
        }

        public override string Description => "FCFS based CombatClass for the Blood Deathknight spec.";

        public override string Displayname => "Deathknight Blood";

        public override bool HandlesMovement => false;

        public override bool IsMelee => true;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicStrengthComparator(new List<ArmorType>() { ArmorType.SHIELDS });

        public override CombatClassRole Role => CombatClassRole.Dps;

        public override TalentTree Talents { get; } = new TalentTree()
        {
            Tree1 = new Dictionary<int, Talent>()
            {
                { 2, new Talent(1, 2, 3) },
                { 3, new Talent(1, 3, 5) },
                { 4, new Talent(1, 4, 5) },
                { 5, new Talent(1, 5, 2) },
                { 6, new Talent(1, 6, 2) },
                { 7, new Talent(1, 7, 1) },
                { 8, new Talent(1, 8, 5) },
                { 9, new Talent(1, 9, 3) },
                { 13, new Talent(1, 13, 3) },
                { 14, new Talent(1, 14, 3) },
                { 16, new Talent(1, 16, 3) },
                { 17, new Talent(1, 17, 2) },
                { 18, new Talent(1, 18, 3) },
                { 19, new Talent(1, 19, 1) },
                { 21, new Talent(1, 21, 2) },
                { 23, new Talent(1, 23, 1) },
                { 24, new Talent(1, 24, 3) },
                { 25, new Talent(1, 25, 1) },
                { 26, new Talent(1, 26, 3) },
                { 27, new Talent(1, 27, 5) },
            },
            Tree2 = new Dictionary<int, Talent>()
            {
                { 1, new Talent(2, 1, 3) },
                { 3, new Talent(2, 3, 5) },
            },
            Tree3 = new Dictionary<int, Talent>()
            {
                { 3, new Talent(3, 3, 5) },
                { 4, new Talent(3, 4, 2) },
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

            if (SelectTarget(TargetManagerDps))
            {
                if (WowInterface.ObjectManager.Target.TargetGuid != WowInterface.ObjectManager.PlayerGuid
                    && TryCastSpellDk(darkCommandSpell, WowInterface.ObjectManager.TargetGuid))
                {
                    return;
                }

                if (WowInterface.ObjectManager.Target.Position.GetDistance(WowInterface.ObjectManager.Player.Position) > 6.0
                    && TryCastSpellDk(deathGripSpell, WowInterface.ObjectManager.TargetGuid, false, false, true))
                {
                    return;
                }

                if (!WowInterface.ObjectManager.Target.HasBuffByName(chainsOfIceSpell)
                    && WowInterface.ObjectManager.Target.Position.GetDistance(WowInterface.ObjectManager.Player.Position) > 2.0
                    && TryCastSpellDk(chainsOfIceSpell, WowInterface.ObjectManager.TargetGuid, false, false, true))
                {
                    return;
                }

                if (TryCastSpellDk(empowerRuneWeapon, 0))
                {
                    return;
                }

                int nearEnemies = WowInterface.ObjectManager.GetNearEnemies<WowUnit>(WowInterface.ObjectManager.Player.Position, 12.0).Count();

                if ((WowInterface.ObjectManager.Player.HealthPercentage < 70
                        && TryCastSpellDk(runeTapSpell, 0, false, false, true))
                    || (WowInterface.ObjectManager.Player.HealthPercentage < 60
                        && (TryCastSpellDk(iceboundFortitudeSpell, 0, true) || TryCastSpellDk(antiMagicShellSpell, 0, true)))
                    || (WowInterface.ObjectManager.Player.HealthPercentage < 50
                        && TryCastSpellDk(vampiricBloodSpell, 0, false, false, true))
                    || (nearEnemies > 2
                        && (TryCastAoeSpellDk(deathAndDecaySpell, 0) || (BloodBoilEvent.Run() && TryCastSpellDk(bloodBoilSpell, 0))))
                    || TryCastSpellDk(unbreakableArmorSpell, 0, false, false, true)
                    || TryCastSpellDk(deathStrike, WowInterface.ObjectManager.TargetGuid, false, false, true, true)
                    || TryCastSpellDk(heartStrikeSpell, WowInterface.ObjectManager.TargetGuid, false, false, true)
                    || TryCastSpellDk(deathCoilSpell, WowInterface.ObjectManager.TargetGuid, true))
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