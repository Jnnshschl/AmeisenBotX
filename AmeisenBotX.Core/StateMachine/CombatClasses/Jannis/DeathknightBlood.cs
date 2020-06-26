using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Statemachine.Enums;
using System.Collections.Generic;
using static AmeisenBotX.Core.Statemachine.Utils.AuraManager;
using static AmeisenBotX.Core.Statemachine.Utils.InterruptManager;

namespace AmeisenBotX.Core.Statemachine.CombatClasses.Jannis
{
    public class DeathknightBlood : BasicCombatClass
    {
        // author: Jannis Höschele

#pragma warning disable IDE0051
        private const string armyOfTheDeadSpell = "Army of the Dead";
        private const string bloodPlagueSpell = "Blood Plague";
        private const string bloodBoilSpell = "Blood Boil";
        private const string vampiricBloodSpell = "Vampiric Blood";
        private const string deathAndDecaySpell = "Death and Decay";
        private const string heartStrikeSpell = "Heart Strike";
        private const string deathCoilSpell = "Death Coil";
        private const string frostFeverSpell = "Frost Fever";
        private const string bloodPresenceSpell = "Blood Presence";
        private const string hornOfWinterSpell = "Horn of Winter";
        private const string iceboundFortitudeSpell = "Icebound Fortitude";
        private const string icyTouchSpell = "Icy Touch";
        private const string mindFreezeSpell = "Mind Freeze";
        private const string deathStrike = "Death Strike";
        private const string plagueStrikeSpell = "Plague Strike";
        private const string runeStrikeSpell = "Rune Strike";
        private const string strangulateSpell = "Strangulate";
        private const string runeTapSpell = "Rune Tap";
        private const string unbreakableArmorSpell = "Unbreakable Armor";
#pragma warning restore IDE0051

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
        }

        public override bool UseAutoAttacks => true;

        public override string Author => "Jannis";

        public override WowClass Class => WowClass.Deathknight;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "FCFS based CombatClass for the Frost Deathknight spec.";

        public override string Displayname => "Deathknight Frost";

        public override bool HandlesMovement => false;

        public override bool IsMelee => true;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicStrengthComparator(new List<ArmorType>() { ArmorType.SHIELDS });

        public override CombatClassRole Role => CombatClassRole.Dps;

        public override string Version => "1.0";

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

        public override bool WalkBehindEnemy => false;

        public override void ExecuteCC()
        {
            if (!WowInterface.ObjectManager.Player.IsAutoAttacking && AutoAttackEvent.Run() && WowInterface.ObjectManager.Player.IsInMeleeRange(WowInterface.ObjectManager.Target))
            {
                WowInterface.HookManager.StartAutoAttack(WowInterface.ObjectManager.Target);
            }

            int nearEnemies = WowInterface.ObjectManager.GetEnemiesTargetingPartymembers(WowInterface.ObjectManager.Player.Position, 12.0).Count;

            if ((WowInterface.ObjectManager.Player.HealthPercentage < 70
                    && CastSpellIfPossibleDk(runeTapSpell, 0, false, false, true))
                || (WowInterface.ObjectManager.Player.HealthPercentage < 60
                    && CastSpellIfPossibleDk(iceboundFortitudeSpell, 0, true))
                || (WowInterface.ObjectManager.Player.HealthPercentage < 50
                    && CastSpellIfPossibleDk(vampiricBloodSpell, 0, false, false, true))
                || (nearEnemies > 2
                    && CastSpellIfPossibleDk(bloodBoilSpell, 0) || CastSpellIfPossibleDk(deathAndDecaySpell, 0))
                || CastSpellIfPossibleDk(unbreakableArmorSpell, 0, false, false, true)
                || CastSpellIfPossibleDk(deathStrike, WowInterface.ObjectManager.TargetGuid, false, false, true, true)
                || CastSpellIfPossibleDk(heartStrikeSpell, WowInterface.ObjectManager.TargetGuid, false, false, true)
                || CastSpellIfPossibleDk(deathCoilSpell, WowInterface.ObjectManager.TargetGuid, true)
                || (WowInterface.ObjectManager.Player.Runeenergy > 60
                    && CastSpellIfPossibleDk(runeStrikeSpell, WowInterface.ObjectManager.TargetGuid)))
            {
                return;
            }
        }

        public override void OutOfCombatExecute()
        {
            MyAuraManager.Tick();
        }
    }
}