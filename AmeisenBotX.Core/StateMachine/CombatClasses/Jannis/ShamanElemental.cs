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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.StateMachine.CombatClasses.Jannis
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
        private readonly string hexSpell = "Hex";
        private readonly string lesserHealingWaveSpell = "Lesser Healing Wave";

        private readonly int buffCheckTime = 8;
        private readonly int debuffCheckTime = 1;
        private readonly int deadPartymembersCheckTime = 4;

        public ShamanElemental(ObjectManager objectManager, CharacterManager characterManager, HookManager hookManager)
        {
            ObjectManager = objectManager;
            CharacterManager = characterManager;
            HookManager = hookManager;
            CooldownManager = new CooldownManager(characterManager.SpellBook.Spells);

            Spells = new Dictionary<string, Spell>();
            CharacterManager.SpellBook.OnSpellBookUpdate += () =>
            {
                Spells.Clear();
                foreach (Spell spell in CharacterManager.SpellBook.Spells)
                {
                    Spells.Add(spell.Name, spell);
                }
            };
        }

        public bool HandlesMovement => false;

        public bool HandlesTargetSelection => false;

        public bool IsMelee => false;

        public IWowItemComparator ItemComparator { get; } = new BasicIntellectComparator();

        private CharacterManager CharacterManager { get; }

        private HookManager HookManager { get; }

        private DateTime LastBuffCheck { get; set; }

        private DateTime LastDebuffCheck { get; set; }

        private DateTime LastDeadPartymembersCheck { get; set; }

        private ObjectManager ObjectManager { get; }

        private CooldownManager CooldownManager { get; }

        private Dictionary<string, Spell> Spells { get; }

        private bool HexedTarget { get; set; }

        public string Displayname => "Shaman Elemental";

        public string Version => "1.0";

        public string Author => "Jannis";

        public string Description => "FCFS based CombatClass for the Elemental Shaman spec.";

        public WowClass Class => WowClass.Shaman;

        public CombatClassRole Role => CombatClassRole.Dps;

        public Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public void Execute()
        {
            // we dont want to do anything if we are casting something...
            if (ObjectManager.Player.IsCasting)
            {
                return;
            }

            if ((DateTime.Now - LastBuffCheck > TimeSpan.FromSeconds(buffCheckTime)
                    && HandleBuffing())
                || DateTime.Now - LastDebuffCheck > TimeSpan.FromSeconds(debuffCheckTime)
                    && HandleDebuffing())
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
                if (ObjectManager.Target.IsCasting
                    && CastSpellIfPossible(windShearSpell))
                {
                    return;
                }

                if ((ObjectManager.Target.Position.GetDistance2D(ObjectManager.Player.Position) < 6
                        && CastSpellIfPossible(thunderstormSpell, true))
                    || (ObjectManager.Target.MaxHealth > 1000000
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
            if ((DateTime.Now - LastBuffCheck > TimeSpan.FromSeconds(buffCheckTime)
                    && HandleBuffing())
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

        private bool HandleBuffing()
        {
            List<string> myBuffs = HookManager.GetBuffs(WowLuaUnit.Player);

            if (!ObjectManager.Player.IsInCombat)
            {
                HookManager.TargetGuid(ObjectManager.PlayerGuid);
            }

            if ((ObjectManager.Player.ManaPercentage > 80
                    && !myBuffs.Any(e => e.Equals(lightningShieldSpell, StringComparison.OrdinalIgnoreCase))
                    && CastSpellIfPossible(lightningShieldSpell, true))
                || (ObjectManager.Player.ManaPercentage < 25
                    && !myBuffs.Any(e => e.Equals(waterShieldSpell, StringComparison.OrdinalIgnoreCase))
                    && CastSpellIfPossible(waterShieldSpell, true)))
            // || (CharacterManager.Equipment.Equipment.TryGetValue(EquipmentSlot.INVSLOT_MAINHAND, out IWowItem mainhandItem)
            //     && !myBuffs.Any(e => e.Equals(mainhandItem.Name, StringComparison.OrdinalIgnoreCase))
            //     && CastSpellIfPossible(flametoungueWeaponSpell, true)))
            {
                return true;
            }

            LastBuffCheck = DateTime.Now;
            return false;
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
                return true;
            }

            return false;
        }
    }
}
