using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
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

        private readonly string armyOfTheDeadSpell = "Army of the Dead";
        private readonly string bloodPlagueSpell = "Blood Plague";
        private readonly string bloodStrikeSpell = "Blood Strike";
        private readonly string deathCoilSpell = "Death Coil";
        private readonly string frostFeverSpell = "Frost Fever";
        private readonly string hornOfWinterSpell = "Horn of Winter";
        private readonly string iceboundFortitudeSpell = "Icebound Fortitude";
        private readonly string icyTouchSpell = "Icy Touch";
        private readonly string mindFreezeSpell = "Mind Freeze";
        private readonly string plagueStrikeSpell = "Plague Strike";
        private readonly string runeStrikeSpell = "Rune Strike";
        private readonly string scourgeStrikeSpell = "Scourge Strike";
        private readonly string strangulateSpell = "Strangulate";
        private readonly string summonGargoyleSpell = "Summon Gargoyle";
        private readonly string unholyPresenceSpell = "Unholy Presence";

        public DeathknightUnholy(WowInterface wowInterface) : base(wowInterface)
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
        }

        public override string Author => "Jannis";

        public override WowClass Class => WowClass.Deathknight;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "FCFS based CombatClass for the Unholy Deathknight spec.";

        public override string Displayname => "Deathknight Unholy";

        public override bool HandlesMovement => false;

        public override bool HandlesTargetSelection => false;

        public override bool IsMelee => true;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicStrengthComparator(new List<ArmorType>() { ArmorType.SHIEDLS });

        public override CombatClassRole Role => CombatClassRole.Dps;

        public override string Version => "1.0";

        private DateTime LastAutoAttackCheck { get; set; }

        public override void ExecuteCC()
        {
            if (DateTime.Now - LastAutoAttackCheck > TimeSpan.FromSeconds(4) && !WowInterface.ObjectManager.Player.IsAutoAttacking)
            {
                LastAutoAttackCheck = DateTime.Now;
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