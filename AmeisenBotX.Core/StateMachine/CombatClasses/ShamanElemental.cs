using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Inventory.Objects;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Hook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.StateMachine.CombatClasses
{
    public class ShamanElemental : ICombatClass
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

        private readonly int buffCheckTime = 8;
        private readonly int debuffCheckTime = 1;
        private readonly int deadPartymembersCheckTime = 4;

        public ShamanElemental(ObjectManager objectManager, CharacterManager characterManager, HookManager hookManager)
        {
            ObjectManager = objectManager;
            CharacterManager = characterManager;
            HookManager = hookManager;
        }

        public bool HandlesMovement => false;

        public bool HandlesTargetSelection => false;

        public bool IsMelee => false;

        private CharacterManager CharacterManager { get; }

        private HookManager HookManager { get; }

        private DateTime LastBuffCheck { get; set; }

        private DateTime LastDebuffCheck { get; set; }

        private DateTime LastDeadPartymembersCheck { get; set; }

        private ObjectManager ObjectManager { get; }

        public void Execute()
        {
            if (DateTime.Now - LastBuffCheck > TimeSpan.FromSeconds(buffCheckTime)
                && HandleBuffing())
            {
                return;
            }

            if (DateTime.Now - LastDebuffCheck > TimeSpan.FromSeconds(debuffCheckTime)
                && HandleDebuffing())
            {
                return;
            }

            //// if (ObjectManager.Player.HealthPercentage < 70
            ////     && IsSpellKnown(flashHealSpell)
            ////     && !IsOnCooldown(flashHealSpell))
            //// {
            ////     HookManager.CastSpell(flashHealSpell);
            ////     return;
            //// }

            WowUnit target = ObjectManager.WowObjects.OfType<WowUnit>().FirstOrDefault(e => e.Guid == ObjectManager.TargetGuid);

            if (target != null)
            {
                if (target.Position.GetDistance2D(ObjectManager.Player.Position) < 8
                    && CastSpellIfPossible(thunderstormSpell, true))
                {
                    return;
                }

                if (target.MaxHealth > 300000
                    && target.HealthPercentage < 16
                    && CastSpellIfPossible(heroismSpell))
                {
                    return;
                }
            }

            if (CastSpellIfPossible(lavaBurstSpell, true))
            {
                return;
            }

            if (CastSpellIfPossible(elementalMasterySpell))
            {
                return;
            }

            if (CastSpellIfPossible(lightningBoltSpell, true))
            {
                return;
            }
        }

        private bool HandleDebuffing()
        {
            List<string> targetDebuffs = HookManager.GetDebuffs(WowLuaUnit.Target);

            if (!targetDebuffs.Any(e => e.Equals(flameShockSpell, StringComparison.OrdinalIgnoreCase))
                && CastSpellIfPossible(flameShockSpell, true))
            {
                return true;
            }

            LastDebuffCheck = DateTime.Now;
            return false;
        }

        public void OutOfCombatExecute()
        {
            if (DateTime.Now - LastBuffCheck > TimeSpan.FromSeconds(buffCheckTime))
            {
                HandleBuffing();
            }

            if (DateTime.Now - LastDeadPartymembersCheck > TimeSpan.FromSeconds(deadPartymembersCheckTime))
            {
                HandleDeadPartymembers();
            }
        }

        private bool HandleBuffing()
        {
            List<string> myBuffs = HookManager.GetBuffs(WowLuaUnit.Player);

            if (!ObjectManager.Player.IsInCombat)
            {
                HookManager.TargetGuid(ObjectManager.PlayerGuid);
            }

            if (ObjectManager.Player.ManaPercentage > 80
                && !myBuffs.Any(e => e.Equals(lightningShieldSpell, StringComparison.OrdinalIgnoreCase))
                && CastSpellIfPossible(lightningShieldSpell, true))
            {
                return true;
            }

            if (ObjectManager.Player.ManaPercentage < 25
                && !myBuffs.Any(e => e.Equals(waterShieldSpell, StringComparison.OrdinalIgnoreCase))
                && CastSpellIfPossible(waterShieldSpell, true))
            {
                return true;
            }

            if (CharacterManager.Equipment.Equipment.TryGetValue(EquipmentSlot.INVSLOT_MAINHAND, out IWowItem mainhandItem)
                && !myBuffs.Any(e => e.Equals(mainhandItem.Name, StringComparison.OrdinalIgnoreCase))
                && CastSpellIfPossible(flametoungueWeaponSpell, true))
            {
                return true;
            }

            LastBuffCheck = DateTime.Now;
            return false;
        }

        private void HandleDeadPartymembers()
        {
            if (IsSpellKnown(ancestralSpiritSpell)
                && HasEnoughMana(ancestralSpiritSpell)
                && !IsOnCooldown(ancestralSpiritSpell))
            {
                IEnumerable<WowPlayer> players = ObjectManager.WowObjects.OfType<WowPlayer>();
                List<WowPlayer> groupPlayers = players.Where(e => e.IsDead && e.Health == 0 && ObjectManager.PartymemberGuids.Contains(e.Guid)).ToList();

                if (groupPlayers.Count > 0)
                {
                    HookManager.TargetGuid(groupPlayers.First().Guid);
                    HookManager.CastSpell(ancestralSpiritSpell);
                }
            }
        }

        private bool CastSpellIfPossible(string spellname, bool needsMana = false)
        {
            if (IsSpellKnown(spellname)
                && (needsMana && HasEnoughMana(spellname))
                && !IsOnCooldown(spellname))
            {
                HookManager.CastSpell(spellname);
                return true;
            }

            return false;
        }

        private bool HasEnoughMana(string spellName)
            => CharacterManager.SpellBook.Spells
            .OrderByDescending(e => e.Rank)
            .FirstOrDefault(e => e.Name.Equals(spellName))
            ?.Costs <= ObjectManager.Player.Mana;

        private bool IsOnCooldown(string spellName)
            => HookManager.GetSpellCooldown(spellName) > 0;

        private bool IsSpellKnown(string spellName)
            => CharacterManager.SpellBook.Spells
            .Any(e => e.Name.Equals(spellName));
    }
}
