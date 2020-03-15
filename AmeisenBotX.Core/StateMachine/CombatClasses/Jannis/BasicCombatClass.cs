using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Spells.Objects;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Statemachine.Enums;
using AmeisenBotX.Core.Statemachine.Utils;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using AmeisenBotX.Pathfinding.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using static AmeisenBotX.Core.Statemachine.Utils.AuraManager;

namespace AmeisenBotX.Core.Statemachine.CombatClasses.Jannis
{
    public abstract class BasicCombatClass : ICombatClass
    {
        protected BasicCombatClass(WowInterface wowInterface)
        {
            WowInterface = wowInterface;
            CooldownManager = new CooldownManager(WowInterface.CharacterManager.SpellBook.Spells);

            Spells = new Dictionary<string, Spell>();
            WowInterface.CharacterManager.SpellBook.OnSpellBookUpdate += () =>
            {
                Spells.Clear();
                foreach (Spell spell in WowInterface.CharacterManager.SpellBook.Spells)
                {
                    Spells.Add(spell.Name, spell);
                }
            };

            MyAuraManager = new AuraManager(
                null,
                null,
                TimeSpan.FromSeconds(1),
                () => { if (WowInterface.ObjectManager.Player != null) { return WowInterface.ObjectManager.Player.Auras.Select(e => e.Name).ToList(); } else { return null; } },
                () => { if (WowInterface.ObjectManager.Player != null) { return WowInterface.ObjectManager.Player.Auras.Select(e => e.Name).ToList(); } else { return null; } },
                null,
                DispellDebuffsFunction);

            TargetAuraManager = new AuraManager(
                null,
                null,
                TimeSpan.FromSeconds(1),
                () => { if (WowInterface.ObjectManager.Target != null) { return WowInterface.ObjectManager.Target.Auras.Select(e => e.Name).ToList(); } else { return null; } },
                () => { if (WowInterface.ObjectManager.Target != null) { return WowInterface.ObjectManager.Target.Auras.Select(e => e.Name).ToList(); } else { return null; } },
                DispellBuffsFunction,
                null);

            TargetInterruptManager = new InterruptManager(WowInterface.ObjectManager.Target, null);
        }

        public abstract string Author { get; }

        public abstract WowClass Class { get; }

        public abstract Dictionary<string, dynamic> Configureables { get; set; }

        public CooldownManager CooldownManager { get; internal set; }

        public abstract string Description { get; }

        public DispellBuffsFunction DispellBuffsFunction { get; internal set; }

        public DispellDebuffsFunction DispellDebuffsFunction { get; internal set; }

        public abstract string Displayname { get; }

        public abstract bool HandlesMovement { get; }

        public abstract bool HandlesTargetSelection { get; }

        public abstract bool IsMelee { get; }

        public abstract IWowItemComparator ItemComparator { get; set; }

        public AuraManager MyAuraManager { get; internal set; }

        public abstract CombatClassRole Role { get; }

        public Dictionary<string, Spell> Spells { get; internal set; }

        public AuraManager TargetAuraManager { get; internal set; }

        public InterruptManager TargetInterruptManager { get; internal set; }

        public abstract string Version { get; }

        public WowInterface WowInterface { get; internal set; }

        public abstract void Execute();

        public abstract void OutOfCombatExecute();

        internal bool CastSpellIfPossible(string spellName, ulong guid, bool needsResource = false, int currentResourceAmount = 0)
        {
            if (!PrepareCast(spellName))
            {
                return false;
            }

            WowUnit target = null;
            if (guid != 0)
            {
                target = WowInterface.ObjectManager.GetWowObjectByGuid<WowUnit>(guid);

                if (target == null)
                {
                    return false;
                }
            }

            if (currentResourceAmount == 0)
            {
                currentResourceAmount = WowInterface.ObjectManager.Player.Class switch
                {
                    WowClass.Deathknight => WowInterface.ObjectManager.Player.Runeenergy,
                    WowClass.Rogue => WowInterface.ObjectManager.Player.Energy,
                    WowClass.Warrior => WowInterface.ObjectManager.Player.Rage,
                    _ => WowInterface.ObjectManager.Player.Mana,
                };
            }

            if (Spells[spellName] != null
                && !CooldownManager.IsSpellOnCooldown(spellName)
                && (!needsResource || Spells[spellName].Costs < currentResourceAmount)
                && (target == null || IsInRange(Spells[spellName], target.Position)))
            {
                if (guid != 0 && WowInterface.ObjectManager.TargetGuid != guid)
                {
                    WowInterface.HookManager.TargetGuid(guid);
                }

                CastSpell(spellName);
                return true;
            }

            return false;
        }

        internal bool CastSpellIfPossibleDk(string spellName, ulong guid, bool needsRuneenergy = false, bool needsBloodrune = false, bool needsFrostrune = false, bool needsUnholyrune = false)
        {
            if (!PrepareCast(spellName))
            {
                return false;
            }

            WowUnit target = null;
            if (guid != 0)
            {
                target = WowInterface.ObjectManager.GetWowObjectByGuid<WowUnit>(guid);

                if (target == null)
                {
                    return false;
                }
            }

            if (Spells[spellName] != null
                && !CooldownManager.IsSpellOnCooldown(spellName)
                && (!needsRuneenergy || Spells[spellName].Costs < WowInterface.ObjectManager.Player.Runeenergy)
                && (!needsBloodrune || (WowInterface.HookManager.IsRuneReady(0) || WowInterface.HookManager.IsRuneReady(1)))
                && (!needsFrostrune || (WowInterface.HookManager.IsRuneReady(2) || WowInterface.HookManager.IsRuneReady(3)))
                && (!needsUnholyrune || (WowInterface.HookManager.IsRuneReady(4) || WowInterface.HookManager.IsRuneReady(5)))
                && (target == null || IsInRange(Spells[spellName], target.Position)))
            {
                if (guid != 0 && WowInterface.ObjectManager.Target.Guid != guid)
                {
                    WowInterface.HookManager.TargetGuid(guid);
                }

                CastSpell(spellName);
                return true;
            }

            return false;
        }

        internal bool CastSpellIfPossibleRogue(string spellName, ulong guid, bool needsEnergy = false, bool needsCombopoints = false, int requiredCombopoints = 1)
        {
            if (!PrepareCast(spellName))
            {
                return false;
            }

            WowUnit target = null;
            if (guid != 0)
            {
                target = WowInterface.ObjectManager.GetWowObjectByGuid<WowUnit>(guid);

                if (target == null)
                {
                    return false;
                }
            }

            if (Spells[spellName] != null
                && !CooldownManager.IsSpellOnCooldown(spellName)
                && (!needsEnergy || Spells[spellName].Costs < WowInterface.ObjectManager.Player.Energy)
                && (!needsCombopoints || WowInterface.ObjectManager.Player.ComboPoints >= requiredCombopoints)
                && (target == null || IsInRange(Spells[spellName], target.Position)))
            {
                if (guid != 0 && WowInterface.ObjectManager.TargetGuid != guid)
                {
                    WowInterface.HookManager.TargetGuid(guid);
                }

                CastSpell(spellName);
                return true;
            }

            return false;
        }

        private void CastSpell(string spellName)
        {
            WowInterface.HookManager.CastSpell(spellName);
            CooldownManager.SetSpellCooldown(spellName, (int)WowInterface.HookManager.GetSpellCooldown(spellName));
            AmeisenLogger.Instance.Log("CombatClass", $"[{Displayname}]: Casting Spell \"{spellName}\" on \"{WowInterface.ObjectManager.Target?.Name}\"", LogLevel.Verbose);
        }

        private bool IsInRange(Spell spell, Vector3 position)
        {
            if ((spell.MinRange == 0 && spell.MaxRange == 0) || spell.MaxRange == 0)
            {
                return true;
            }

            double distance = WowInterface.ObjectManager.Player.Position.GetDistance(position);
            return distance > spell.MinRange && distance < spell.MaxRange;
        }

        private bool PrepareCast(string spellName)
        {
            if (!Spells.ContainsKey(spellName))
            {
                Spell spell = WowInterface.CharacterManager.SpellBook.GetSpellByName(spellName);

                if (spell != null)
                {
                    Spells.Add(spellName, spell);
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        public override string ToString() => $"[{Class}] [{Role}] {Displayname}";
    }
}