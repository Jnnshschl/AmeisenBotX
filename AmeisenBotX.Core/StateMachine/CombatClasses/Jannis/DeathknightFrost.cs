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
    public class DeathknightFrost : BasicCombatClass
    {
        // author: Jannis Höschele

#pragma warning disable IDE0051
        private const string armyOfTheDeadSpell = "Army of the Dead";
        private const string bloodPlagueSpell = "Blood Plague";
        private const string bloodStrikeSpell = "Blood Strike";
        private const string deathCoilSpell = "Death Coil";
        private const string frostFeverSpell = "Frost Fever";
        private const string frostPresenceSpell = "Frost Presence";
        private const string hornOfWinterSpell = "Horn of Winter";
        private const string iceboundFortitudeSpell = "Icebound Fortitude";
        private const string icyTouchSpell = "Icy Touch";
        private const string mindFreezeSpell = "Mind Freeze";
        private const string obliterateSpell = "Obliterate";
        private const string plagueStrikeSpell = "Plague Strike";
        private const string runeStrikeSpell = "Rune Strike";
        private const string strangulateSpell = "Strangulate";
        private const string unbreakableArmorSpell = "Unbreakable Armor";
#pragma warning restore IDE0051

        public DeathknightFrost(WowInterface wowInterface, AmeisenBotStateMachine stateMachine) : base(wowInterface, stateMachine)
        {
            MyAuraManager.BuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { frostPresenceSpell, () => CastSpellIfPossibleDk(frostPresenceSpell, 0) },
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
            },
            Tree2 = new Dictionary<int, Talent>()
            {
                { 1, new Talent(2, 1, 3) },
                { 2, new Talent(2, 2, 2) },
                { 5, new Talent(2, 5, 2) },
                { 6, new Talent(2, 6, 3) },
                { 7, new Talent(2, 7, 5) },
                { 9, new Talent(2, 9, 3) },
                { 10, new Talent(2, 10, 5) },
                { 11, new Talent(2, 11, 2) },
                { 12, new Talent(2, 12, 2) },
                { 14, new Talent(2, 14, 3) },
                { 16, new Talent(2, 16, 1) },
                { 17, new Talent(2, 17, 2) },
                { 18, new Talent(2, 18, 3) },
                { 22, new Talent(2, 22, 3) },
                { 23, new Talent(2, 23, 3) },
                { 24, new Talent(2, 24, 1) },
                { 26, new Talent(2, 26, 1) },
                { 27, new Talent(2, 27, 3) },
                { 28, new Talent(2, 28, 5) },
                { 29, new Talent(2, 29, 1) },
            },
            Tree3 = new Dictionary<int, Talent>()
            {
                { 1, new Talent(3, 1, 2) },
                { 2, new Talent(3, 2, 3) },
                { 4, new Talent(3, 4, 2) },
                { 7, new Talent(3, 7, 3) },
                { 9, new Talent(3, 9, 5) },
            },
        };

        public override bool WalkBehindEnemy => false;

        public override void ExecuteCC()
        {
            if (!WowInterface.ObjectManager.Player.IsAutoAttacking && AutoAttackEvent.Run() && WowInterface.ObjectManager.Player.IsInMeleeRange(WowInterface.ObjectManager.Target))
            {
                WowInterface.HookManager.StartAutoAttack(WowInterface.ObjectManager.Target);
            }

            if ((WowInterface.ObjectManager.Player.HealthPercentage < 60
                    && CastSpellIfPossibleDk(iceboundFortitudeSpell, 0, true))
                || CastSpellIfPossibleDk(unbreakableArmorSpell, 0, false, false, true)
                || CastSpellIfPossibleDk(obliterateSpell, WowInterface.ObjectManager.TargetGuid, false, false, true, true)
                || CastSpellIfPossibleDk(bloodStrikeSpell, WowInterface.ObjectManager.TargetGuid, false, true)
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