using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Statemachine.Enums;
using System;
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

            AutoAttackEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(4000));
        }

        public override string Author => "Jannis";

        public override WowClass Class => WowClass.Deathknight;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "FCFS based CombatClass for the Frost Deathknight spec.";

        public override string Displayname => "Deathknight Frost";

        public override bool HandlesMovement => false;

        public override bool IsMelee => true;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicStrengthComparator(new List<ArmorType>() { ArmorType.SHIEDLS });

        public override CombatClassRole Role => CombatClassRole.Dps;

        public override string Version => "1.0";

        private TimegatedEvent AutoAttackEvent { get; set; }

        public override void ExecuteCC()
        {
            if (!WowInterface.ObjectManager.Player.IsAutoAttacking && AutoAttackEvent.Run())
            {
                WowInterface.HookManager.StartAutoAttack(WowInterface.ObjectManager.Target);
            }

            if (MyAuraManager.Tick()
                || TargetAuraManager.Tick()
                || TargetInterruptManager.Tick()
                || (WowInterface.ObjectManager.Player.HealthPercentage < 60
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