using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Statemachine.Enums;
using System;
using System.Collections.Generic;
using static AmeisenBotX.Core.Statemachine.Utils.AuraManager;
using static AmeisenBotX.Core.Statemachine.Utils.InterruptManager;

namespace AmeisenBotX.Core.Statemachine.CombatClasses.Jannis
{
    public class DeathknightUnholy : BasicCombatClass
    {
        // author: Jannis Höschele

#pragma warning disable IDE0051
        private const string armyOfTheDeadSpell = "Army of the Dead";
        private const string bloodPlagueSpell = "Blood Plague";
        private const string bloodStrikeSpell = "Blood Strike";
        private const string deathCoilSpell = "Death Coil";
        private const string frostFeverSpell = "Frost Fever";
        private const string hornOfWinterSpell = "Horn of Winter";
        private const string iceboundFortitudeSpell = "Icebound Fortitude";
        private const string icyTouchSpell = "Icy Touch";
        private const string mindFreezeSpell = "Mind Freeze";
        private const string plagueStrikeSpell = "Plague Strike";
        private const string runeStrikeSpell = "Rune Strike";
        private const string scourgeStrikeSpell = "Scourge Strike";
        private const string strangulateSpell = "Strangulate";
        private const string summonGargoyleSpell = "Summon Gargoyle";
        private const string unholyPresenceSpell = "Unholy Presence";
#pragma warning restore IDE0051

        public DeathknightUnholy(WowInterface wowInterface, AmeisenBotStateMachine stateMachine) : base(wowInterface, stateMachine)
        {
            MyAuraManager.BuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { unholyPresenceSpell, () => CastSpellIfPossibleDk(unholyPresenceSpell, 0) },
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

            AutoAttackEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(1000));
        }

        public override string Author => "Jannis";

        public override WowClass Class => WowClass.Deathknight;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "FCFS based CombatClass for the Unholy Deathknight spec.";

        public override string Displayname => "Deathknight Unholy";

        public override bool HandlesMovement => false;

        public override bool IsMelee => true;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicStrengthComparator(new List<ArmorType>() { ArmorType.SHIELDS });

        public override CombatClassRole Role => CombatClassRole.Dps;

        public override string Version => "1.0";

        private TimegatedEvent AutoAttackEvent { get; set; }

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
            Tree2 = new Dictionary<int, Talent>()
            {
            },
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

        public override void ExecuteCC()
        {
            if (!WowInterface.ObjectManager.Player.IsAutoAttacking && AutoAttackEvent.Run() && WowInterface.ObjectManager.Player.IsInMeleeRange(WowInterface.ObjectManager.Target))
            {
                WowInterface.HookManager.StartAutoAttack(WowInterface.ObjectManager.Target);
            }

            if (MyAuraManager.Tick()
                || TargetAuraManager.Tick()
                || TargetInterruptManager.Tick()
                || (WowInterface.ObjectManager.Player.HealthPercentage < 60
                    && CastSpellIfPossibleDk(iceboundFortitudeSpell, WowInterface.ObjectManager.TargetGuid, true))
                || CastSpellIfPossibleDk(bloodStrikeSpell, WowInterface.ObjectManager.TargetGuid, false, true)
                || CastSpellIfPossibleDk(scourgeStrikeSpell, WowInterface.ObjectManager.TargetGuid, false, false, true, true)
                || CastSpellIfPossibleDk(deathCoilSpell, WowInterface.ObjectManager.TargetGuid, true)
                || CastSpellIfPossibleDk(summonGargoyleSpell, WowInterface.ObjectManager.TargetGuid, true)
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