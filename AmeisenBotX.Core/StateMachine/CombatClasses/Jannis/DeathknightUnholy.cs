using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Core.Statemachine.Enums;
using System.Collections.Generic;
using static AmeisenBotX.Core.Statemachine.Utils.AuraManager;
using static AmeisenBotX.Core.Statemachine.Utils.InterruptManager;

namespace AmeisenBotX.Core.Statemachine.CombatClasses.Jannis
{
    public class DeathknightUnholy : BasicCombatClass
    {
        public DeathknightUnholy(AmeisenBotStateMachine stateMachine) : base(stateMachine)
        {
            MyAuraManager.BuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { unholyPresenceSpell, () => TryCastSpellDk(unholyPresenceSpell, 0) },
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
        }

        public override string Description => "FCFS based CombatClass for the Unholy Deathknight spec.";

        public override string Displayname => "Deathknight Unholy";

        public override bool HandlesMovement => false;

        public override bool IsMelee => true;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicStrengthComparator(new List<ArmorType>() { ArmorType.SHIELDS });

        public override CombatClassRole Role => CombatClassRole.Dps;

        public override TalentTree Talents { get; } = new TalentTree()
        {
            Tree1 = new Dictionary<int, Talent>()
            {
                { 1, new Talent(1, 1, 2) },
                { 2, new Talent(1, 2, 3) },
                { 4, new Talent(1, 4, 5) },
                { 6, new Talent(1, 6, 2) },
                { 8, new Talent(1, 8, 5) },
            },
            Tree2 = new Dictionary<int, Talent>(),
            Tree3 = new Dictionary<int, Talent>()
            {
                { 1, new Talent(3, 1, 2) },
                { 2, new Talent(3, 2, 3) },
                { 4, new Talent(3, 4, 2) },
                { 7, new Talent(3, 7, 3) },
                { 8, new Talent(3, 8, 3) },
                { 9, new Talent(3, 9, 5) },
                { 12, new Talent(3, 12, 3) },
                { 13, new Talent(3, 13, 2) },
                { 14, new Talent(3, 14, 1) },
                { 15, new Talent(3, 15, 5) },
                { 16, new Talent(3, 16, 2) },
                { 20, new Talent(3, 20, 1) },
                { 21, new Talent(3, 21, 5) },
                { 25, new Talent(3, 25, 3) },
                { 26, new Talent(3, 26, 1) },
                { 27, new Talent(3, 27, 3) },
                { 28, new Talent(3, 28, 3) },
                { 29, new Talent(3, 29, 1) },
                { 30, new Talent(3, 30, 5) },
                { 31, new Talent(3, 31, 1) },
            },
        };

        public override bool UseAutoAttacks => true;

        public override string Version => "1.0";

        public override bool WalkBehindEnemy => false;

        public override WowClass WowClass => WowClass.Deathknight;

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

                if (!WowInterface.ObjectManager.Target.HasBuffByName(chainsOfIceSpell)
                    && WowInterface.ObjectManager.Target.Position.GetDistance(WowInterface.ObjectManager.Player.Position) > 2.0
                    && TryCastSpellDk(chainsOfIceSpell, WowInterface.ObjectManager.TargetGuid, false, false, true))
                {
                    return;
                }

                if (WowInterface.ObjectManager.Target.HasBuffByName(chainsOfIceSpell)
                    && TryCastSpellDk(chainsOfIceSpell, WowInterface.ObjectManager.TargetGuid, false, false, true))
                {
                    return;
                }

                if (TryCastSpellDk(empowerRuneWeapon, 0))
                {
                    return;
                }

                if ((WowInterface.ObjectManager.Player.HealthPercentage < 60
                        && TryCastSpellDk(iceboundFortitudeSpell, WowInterface.ObjectManager.TargetGuid, true))
                    || TryCastSpellDk(bloodStrikeSpell, WowInterface.ObjectManager.TargetGuid, false, true)
                    || TryCastSpellDk(scourgeStrikeSpell, WowInterface.ObjectManager.TargetGuid, false, false, true, true)
                    || TryCastSpellDk(deathCoilSpell, WowInterface.ObjectManager.TargetGuid, true)
                    || TryCastSpellDk(summonGargoyleSpell, WowInterface.ObjectManager.TargetGuid, true)
                    || (WowInterface.ObjectManager.Player.Runeenergy > 60
                        && TryCastSpellDk(runeStrikeSpell, WowInterface.ObjectManager.TargetGuid)))
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