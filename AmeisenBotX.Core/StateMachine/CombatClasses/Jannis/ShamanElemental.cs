using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.StateMachine.Enums;
using AmeisenBotX.Core.StateMachine.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using static AmeisenBotX.Core.StateMachine.Utils.AuraManager;
using static AmeisenBotX.Core.StateMachine.Utils.InterruptManager;

namespace AmeisenBotX.Core.StateMachine.CombatClasses.Jannis
{
    public class ShamanElemental : BasicCombatClass
    {
        // author: Jannis Höschele

        private readonly string ancestralSpiritSpell = "Ancestral Spirit";
        private readonly string chainLightningSpell = "Chain Lightning";
        private readonly int deadPartymembersCheckTime = 4;
        private readonly string elementalMasterySpell = "Elemental Mastery";
        private readonly string flameShockSpell = "Flame Shock";
        private readonly string flametoungueWeaponSpell = "Flametoungue Weapon";
        private readonly string heroismSpell = "Heroism";
        private readonly string hexSpell = "Hex";
        private readonly string lavaBurstSpell = "Lava Burst";
        private readonly string lesserHealingWaveSpell = "Lesser Healing Wave";
        private readonly string lightningBoltSpell = "Lightning Bolt";
        private readonly string lightningShieldSpell = "Lightning Shield";
        private readonly string thunderstormSpell = "Thunderstorm";
        private readonly string waterShieldSpell = "Water Shield";
        private readonly string windShearSpell = "Wind Shear";

        public ShamanElemental(WowInterface wowInterface) : base(wowInterface)
        {
            MyAuraManager.BuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { lightningShieldSpell, () => WowInterface.ObjectManager.Player.ManaPercentage > 0.8 && CastSpellIfPossible(lightningShieldSpell, true) },
                { waterShieldSpell, () => WowInterface.ObjectManager.Player.ManaPercentage < 0.2 && CastSpellIfPossible(waterShieldSpell, true) }
            };

            TargetAuraManager.DebuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { flameShockSpell, () => CastSpellIfPossible(flameShockSpell, true) }
            };

            TargetInterruptManager.InterruptSpells = new SortedList<int, CastInterruptFunction>()
            {
                { 0, () => CastSpellIfPossible(windShearSpell, true) },
                { 1, () => CastSpellIfPossible(hexSpell, true) }
            };
        }

        public override string Author => "Jannis";

        public override WowClass Class => WowClass.Shaman;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "FCFS based CombatClass for the Elemental Shaman spec.";

        public override string Displayname => "Shaman Elemental";

        public override bool HandlesMovement => false;

        public override bool HandlesTargetSelection => false;

        public override bool IsMelee => false;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicIntellectComparator();

        public override CombatClassRole Role => CombatClassRole.Dps;

        public override string Version => "1.0";

        private bool HexedTarget { get; set; }

        private DateTime LastDeadPartymembersCheck { get; set; }

        public override void Execute()
        {
            // we dont want to do anything if we are casting something...
            if (WowInterface.ObjectManager.Player.IsCasting)
            {
                return;
            }

            if (MyAuraManager.Tick()
                || TargetAuraManager.Tick()
                || TargetInterruptManager.Tick())
            {
                return;
            }

            if (WowInterface.ObjectManager.Player.HealthPercentage < 30
                && CastSpellIfPossible(hexSpell, true))
            {
                HexedTarget = true;
                return;
            }

            if (WowInterface.ObjectManager.Player.HealthPercentage < 30
                && (!WowInterface.CharacterManager.SpellBook.IsSpellKnown(hexSpell)
                || HexedTarget)
                && CastSpellIfPossible(lesserHealingWaveSpell, true))
            {
                return;
            }

            if (WowInterface.ObjectManager.Target != null)
            {
                if ((WowInterface.ObjectManager.Target.Position.GetDistance(WowInterface.ObjectManager.Player.Position) < 6
                        && CastSpellIfPossible(thunderstormSpell, true))
                    || (WowInterface.ObjectManager.Target.MaxHealth > 10000000
                        && WowInterface.ObjectManager.Target.HealthPercentage < 25
                        && CastSpellIfPossible(heroismSpell))
                    || CastSpellIfPossible(lavaBurstSpell, true)
                    || CastSpellIfPossible(elementalMasterySpell))
                {
                    return;
                }

                if ((WowInterface.ObjectManager.WowObjects.OfType<WowUnit>().Where(e => WowInterface.ObjectManager.Target.Position.GetDistance(e.Position) < 16).Count() > 2 && CastSpellIfPossible(chainLightningSpell, true))
                    || CastSpellIfPossible(lightningBoltSpell, true))
                {
                    return;
                }
            }
        }

        public override void OutOfCombatExecute()
        {
            if (MyAuraManager.Tick()
                || DateTime.Now - LastDeadPartymembersCheck > TimeSpan.FromSeconds(deadPartymembersCheckTime)
                    && HandleDeadPartymembers())
            {
                return;
            }

            if (HexedTarget)
            {
                HexedTarget = false;
            }
        }

        private bool HandleDeadPartymembers()
        {
            if (!Spells.ContainsKey(ancestralSpiritSpell))
            {
                Spells.Add(ancestralSpiritSpell, WowInterface.CharacterManager.SpellBook.GetSpellByName(ancestralSpiritSpell));
            }

            if (Spells[ancestralSpiritSpell] != null
                && !CooldownManager.IsSpellOnCooldown(ancestralSpiritSpell)
                && Spells[ancestralSpiritSpell].Costs < WowInterface.ObjectManager.Player.Mana)
            {
                IEnumerable<WowPlayer> players = WowInterface.ObjectManager.WowObjects.OfType<WowPlayer>();
                List<WowPlayer> groupPlayers = players.Where(e => e.IsDead && e.Health == 0 && WowInterface.ObjectManager.PartymemberGuids.Contains(e.Guid)).ToList();

                if (groupPlayers.Count > 0)
                {
                    HookManager.TargetGuid(groupPlayers.First().Guid);
                    HookManager.CastSpell(ancestralSpiritSpell);
                    CooldownManager.SetSpellCooldown(ancestralSpiritSpell, (int)HookManager.GetSpellCooldown(ancestralSpiritSpell));
                    return true;
                }
            }

            return false;
        }
    }
}