using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Inventory.Objects;
using AmeisenBotX.Core.Character.Spells.Objects;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.StateMachine.Enums;
using AmeisenBotX.Core.StateMachine.Utils;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AmeisenBotX.Core.StateMachine.Utils.AuraManager;
using static AmeisenBotX.Core.StateMachine.Utils.InterruptManager;

namespace AmeisenBotX.Core.StateMachine.CombatClasses.Jannis
{
    public class ShamanElemental : BasicCombatClass
    {
        // author: Jannis Höschele

        private readonly string flameShockSpell = "Flame Shock";
        private readonly string lavaBurstSpell = "Lava Burst";
        private readonly string lightningBoltSpell = "Lightning Bolt";
        private readonly string chainLightningSpell = "Chain Lightning";
        private readonly string windShearSpell = "Wind Shear";
        private readonly string thunderstormSpell = "Thunderstorm";
        private readonly string lightningShieldSpell = "Lightning Shield";
        private readonly string waterShieldSpell = "Water Shield";
        private readonly string flametoungueWeaponSpell = "Flametoungue Weapon";
        private readonly string elementalMasterySpell = "Elemental Mastery";
        private readonly string heroismSpell = "Heroism";
        private readonly string ancestralSpiritSpell = "Ancestral Spirit";
        private readonly string hexSpell = "Hex";
        private readonly string lesserHealingWaveSpell = "Lesser Healing Wave";

        private readonly int deadPartymembersCheckTime = 4;

        public ShamanElemental(ObjectManager objectManager, CharacterManager characterManager, HookManager hookManager) : base(objectManager, characterManager, hookManager)
        {
            BuffsToKeepOnMe = new Dictionary<string, CastFunction>()
            {
                { lightningShieldSpell, () => ObjectManager.Player.ManaPercentage > 0.8 && CastSpellIfPossible(lightningShieldSpell, true) },
                { waterShieldSpell, () => ObjectManager.Player.ManaPercentage < 0.2 && CastSpellIfPossible(waterShieldSpell, true) }
            };

            DebuffsToKeepOnTarget = new Dictionary<string, CastFunction>()
            {
                { flameShockSpell, () => CastSpellIfPossible(flameShockSpell, true) }
            };

            InterruptSpells = new SortedList<int, CastInterruptFunction>()
            {
                { 0, () => CastSpellIfPossible(windShearSpell, true) },
                { 1, () => CastSpellIfPossible(hexSpell, true) }
            };
        }

        public override bool HandlesMovement => false;

        public override bool HandlesTargetSelection => false;

        public override bool IsMelee => false;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicIntellectComparator();

        private DateTime LastDeadPartymembersCheck { get; set; }

        private bool HexedTarget { get; set; }

        public override string Displayname => "Shaman Elemental";

        public override string Version => "1.0";

        public override string Author => "Jannis";

        public override string Description => "FCFS based CombatClass for the Elemental Shaman spec.";

        public override WowClass Class => WowClass.Shaman;

        public override CombatClassRole Role => CombatClassRole.Dps;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public override void Execute()
        {
            // we dont want to do anything if we are casting something...
            if (ObjectManager.Player.IsCasting)
            {
                return;
            }

            if (MyAuraManager.Tick()
                || TargetAuraManager.Tick()
                || TargetInterruptManager.Tick())
            {
                return;
            }

            if (ObjectManager.Player.HealthPercentage < 30
                && CastSpellIfPossible(hexSpell, true))
            {
                HexedTarget = true;
                return;
            }

            if (ObjectManager.Player.HealthPercentage < 30
                && (!CharacterManager.SpellBook.IsSpellKnown(hexSpell)
                || HexedTarget)
                && CastSpellIfPossible(lesserHealingWaveSpell, true))
            {
                return;
            }

            if (ObjectManager.Target != null)
            {
                if ((ObjectManager.Target.Position.GetDistance2D(ObjectManager.Player.Position) < 6
                        && CastSpellIfPossible(thunderstormSpell, true))
                    || (ObjectManager.Target.MaxHealth > 10000000
                        && ObjectManager.Target.HealthPercentage < 25
                        && CastSpellIfPossible(heroismSpell))
                    || CastSpellIfPossible(lavaBurstSpell, true)
                    || CastSpellIfPossible(elementalMasterySpell))
                {
                    return;
                }

                if ((ObjectManager.WowObjects.OfType<WowUnit>().Where(e => ObjectManager.Target.Position.GetDistance(e.Position) < 16).Count() > 2 && CastSpellIfPossible(chainLightningSpell, true))
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
                Spells.Add(ancestralSpiritSpell, CharacterManager.SpellBook.GetSpellByName(ancestralSpiritSpell));
            }

            if (Spells[ancestralSpiritSpell] != null
                && !CooldownManager.IsSpellOnCooldown(ancestralSpiritSpell)
                && Spells[ancestralSpiritSpell].Costs < ObjectManager.Player.Mana)
            {
                IEnumerable<WowPlayer> players = ObjectManager.WowObjects.OfType<WowPlayer>();
                List<WowPlayer> groupPlayers = players.Where(e => e.IsDead && e.Health == 0 && ObjectManager.PartymemberGuids.Contains(e.Guid)).ToList();

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

        private bool CastSpellIfPossible(string spellName, bool needsMana = false)
        {
            AmeisenLogger.Instance.Log($"[{Displayname}]: Trying to cast \"{spellName}\" on \"{ObjectManager.Target?.Name}\"", LogLevel.Verbose);

            if (!Spells.ContainsKey(spellName))
            {
                Spells.Add(spellName, CharacterManager.SpellBook.GetSpellByName(spellName));
            }

            if (Spells[spellName] != null
                && !CooldownManager.IsSpellOnCooldown(spellName)
                && (!needsMana || Spells[spellName].Costs < ObjectManager.Player.Mana))
            {
                HookManager.CastSpell(spellName);
                CooldownManager.SetSpellCooldown(spellName, (int)HookManager.GetSpellCooldown(spellName));
                AmeisenLogger.Instance.Log($"[{Displayname}]: Casting Spell \"{spellName}\" on \"{ObjectManager.Target?.Name}\"", LogLevel.Verbose);
                return true;
            }

            return false;
        }
    }
}
