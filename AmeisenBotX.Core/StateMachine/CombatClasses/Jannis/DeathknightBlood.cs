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
        public DeathknightBlood(WowInterface wowInterface, AmeisenBotStateMachine stateMachine) : base(wowInterface, stateMachine)
        {
            MyAuraManager.BuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { bloodPresenceSpell, () => CastSpellIfPossibleDk(bloodPresenceSpell, 0) },
                { hornOfWinterSpell, () => CastSpellIfPossibleDk(hornOfWinterSpell, 0, true) }
            };

            TargetAuraManager.DebuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { frostFeverSpell, () => CastSpellIfPossibleDk(icyTouchSpell, WowInterface.ObjectManager.TargetGuid, false, false, false, true) },
                { bloodPlagueSpell, () => CastSpellIfPossibleDk(plagueStrikeSpell, WowInterface.ObjectManager.TargetGuid, false, false, false, true) }
            };

            TargetInterruptManager.InterruptSpells = new SortedList<int, CastInterruptFunction>()
            {
                { 0, (x) => CastSpellIfPossibleDk(mindFreezeSpell, x.Guid, true) },
                { 1, (x) => CastSpellIfPossibleDk(strangulateSpell, x.Guid, false, true) }
            };

            BloodBoilEvent = new TimegatedEvent(TimeSpan.FromSeconds(2));
        }

        public override string Author => "Jannis";

        public override WowClass WowClass => WowClass.Deathknight;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

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

        private TimegatedEvent BloodBoilEvent { get; }

        public override void ExecuteCC()
        {
            if (SelectTarget(DpsTargetManager))
            {
                if (WowInterface.ObjectManager.Target.TargetGuid != WowInterface.ObjectManager.PlayerGuid
                    && CastSpellIfPossibleDk(darkCommandSpell, WowInterface.ObjectManager.TargetGuid))
                {
                    return;
                }

                if (WowInterface.ObjectManager.Target.Position.GetDistance(WowInterface.ObjectManager.Player.Position) > 6.0
                    && CastSpellIfPossibleDk(deathGripSpell, WowInterface.ObjectManager.TargetGuid, false, false, true))
                {
                    return;
                }

                if (!WowInterface.ObjectManager.Target.HasBuffByName(chainsOfIceSpell)
                    && WowInterface.ObjectManager.Target.Position.GetDistance(WowInterface.ObjectManager.Player.Position) > 2.0
                    && CastSpellIfPossibleDk(chainsOfIceSpell, WowInterface.ObjectManager.TargetGuid, false, false, true))
                {
                    return;
                }

                if (CastSpellIfPossibleDk(empowerRuneWeapon, 0))
                {
                    return;
                }

                int nearEnemies = WowInterface.ObjectManager.GetNearEnemies<WowUnit>(WowInterface.ObjectManager.Player.Position, 12.0).Count();

                if ((WowInterface.ObjectManager.Player.HealthPercentage < 70
                        && CastSpellIfPossibleDk(runeTapSpell, 0, false, false, true))
                    || (WowInterface.ObjectManager.Player.HealthPercentage < 60
                        && (CastSpellIfPossibleDk(iceboundFortitudeSpell, 0, true) || CastSpellIfPossibleDk(antiMagicShellSpell, 0, true)))
                    || (WowInterface.ObjectManager.Player.HealthPercentage < 50
                        && CastSpellIfPossibleDk(vampiricBloodSpell, 0, false, false, true))
                    || (nearEnemies > 2
                        && (CastSpellIfPossibleDkArea(deathAndDecaySpell, 0) || (BloodBoilEvent.Run() && CastSpellIfPossibleDk(bloodBoilSpell, 0))))
                    || CastSpellIfPossibleDk(unbreakableArmorSpell, 0, false, false, true)
                    || CastSpellIfPossibleDk(deathStrike, WowInterface.ObjectManager.TargetGuid, false, false, true, true)
                    || CastSpellIfPossibleDk(heartStrikeSpell, WowInterface.ObjectManager.TargetGuid, false, false, true)
                    || CastSpellIfPossibleDk(deathCoilSpell, WowInterface.ObjectManager.TargetGuid, true))
                {
                    return;
                }
            }
        }

        public override void OutOfCombatExecute()
        {
            MyAuraManager.Tick();
        }
    }
}