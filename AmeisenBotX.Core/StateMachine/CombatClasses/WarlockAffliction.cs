using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Character.Spells.Objects;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.StateMachine.CombatClasses.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.StateMachine.CombatClasses
{
    class WarlockAffliction : ICombatClass
    {
        // author: Jannis Höschele

        private readonly string corruptionSpell = "Corruption";
        private readonly string curseOfAgonySpell = "Curse of Agony";
        private readonly string unstableAfflictionSpell = "Unstable Affliction";
        private readonly string hauntSpell = "Haunt";
        private readonly string lifeTapSpell = "Life Tap";
        private readonly string drainSoulSpell = "Drain Soul";
        private readonly string shadowBoltSpell = "Shadow Bolt";
        private readonly string fearSpell = "Fear";
        private readonly string howlOfTerrorSpell = "Howl of Terror";
        private readonly string demonSkinSpell = "Demon Skin";
        private readonly string demonArmorSpell = "Demon Armor";
        private readonly string felArmorSpell = "Fel Armor";
        private readonly string deathCoilSpell = "Death Coil";
        private readonly string sumonImpSpell = "Summon Imp";

        private readonly int buffCheckTime = 8;
        private readonly int debuffCheckTime = 1;

        public WarlockAffliction(ObjectManager objectManager, CharacterManager characterManager, HookManager hookManager)
        {
            ObjectManager = objectManager;
            CharacterManager = characterManager;
            HookManager = hookManager;
            CooldownManager = new CooldownManager(characterManager.SpellBook.Spells);

            Spells = new Dictionary<string, Spell>();
            CharacterManager.SpellBook.OnSpellBookUpdate += () => { Spells.Clear(); };
        }

        public bool HandlesMovement => false;

        public bool HandlesTargetSelection => false;

        public bool IsMelee => false;

        private CharacterManager CharacterManager { get; }

        private HookManager HookManager { get; }

        private DateTime LastBuffCheck { get; set; }

        private DateTime LastDebuffCheck { get; set; }

        private ObjectManager ObjectManager { get; }

        private CooldownManager CooldownManager { get; }

        private Dictionary<string, Spell> Spells { get; }

        public void Execute()
        {
            // we dont want to do anything if we are casting something...
            if (ObjectManager.Player.CurrentlyCastingSpellId > 0
                || ObjectManager.Player.CurrentlyChannelingSpellId > 0)
            {
                return;
            }

            if ((DateTime.Now - LastDebuffCheck > TimeSpan.FromSeconds(debuffCheckTime)
                    && HandleDebuffing())
                || ObjectManager.Player.ManaPercentage < 90
                    && ObjectManager.Player.HealthPercentage > 60
                    && CastSpellIfPossible(lifeTapSpell)
                || (ObjectManager.Player.HealthPercentage < 80
                    && CastSpellIfPossible(deathCoilSpell, true)))
            {
                return;
            }

            WowUnit target = ObjectManager.WowObjects.OfType<WowUnit>().FirstOrDefault(e => e.Guid == ObjectManager.TargetGuid);

            if (target != null)
            {
                if (target.GetType() == typeof(WowPlayer))
                {
                    if ((ObjectManager.Player.Position.GetDistance(target.Position) < 6
                            && CastSpellIfPossible(howlOfTerrorSpell, true))
                        || (ObjectManager.Player.Position.GetDistance(target.Position) < 12
                            && CastSpellIfPossible(fearSpell, true))
                        || (ObjectManager.Player.CurrentlyCastingSpellId == 0
                            && ObjectManager.Player.CurrentlyCastingSpellId == 0
                            && target.HealthPercentage < 0.25
                            && CastSpellIfPossible(drainSoulSpell, true)))
                    {
                        return;
                    }
                }
            }

            if (CastSpellIfPossible(shadowBoltSpell, true))
            {
                return;
            }
        }

        public void OutOfCombatExecute()
        {
            if (DateTime.Now - LastBuffCheck > TimeSpan.FromSeconds(buffCheckTime))
            {
                HandleBuffing();

                if (ObjectManager.PetGuid == 0
                    && CastSpellIfPossible(sumonImpSpell, true))
                {
                    return;
                }
            }
        }

        private bool HandleBuffing()
        {
            List<string> myBuffs = HookManager.GetBuffs(WowLuaUnit.Player);

            if (!ObjectManager.Player.IsInCombat)
            {
                HookManager.TargetGuid(ObjectManager.PlayerGuid);
            }

            if (Spells.ContainsKey(demonSkinSpell)
                && Spells[demonSkinSpell] != null)
            {
                if ((!myBuffs.Any(e => e.Equals(demonSkinSpell, StringComparison.OrdinalIgnoreCase))
                        && CastSpellIfPossible(demonSkinSpell, true)))
                {
                    return true;
                }
            }
            else if (Spells.ContainsKey(demonArmorSpell)
                    && Spells[demonArmorSpell] != null)
            {
                if (Spells.ContainsKey(demonArmorSpell)
                    && Spells[demonSkinSpell] != null
                    && (!myBuffs.Any(e => e.Equals(demonArmorSpell, StringComparison.OrdinalIgnoreCase))
                        && CastSpellIfPossible(demonArmorSpell, true)))
                {
                    return true;
                }
            }
            else if (Spells.ContainsKey(felArmorSpell)
                    && Spells[felArmorSpell] != null)
            {

                if (Spells.ContainsKey(felArmorSpell)
                    && Spells[demonSkinSpell] != null
                    && (!myBuffs.Any(e => e.Equals(felArmorSpell, StringComparison.OrdinalIgnoreCase))
                        && CastSpellIfPossible(felArmorSpell, true)))
                {
                    return true;
                }
            }

            LastBuffCheck = DateTime.Now;
            return false;
        }

        private bool HandleDebuffing()
        {
            List<string> targetDebuffs = HookManager.GetDebuffs(WowLuaUnit.Target);

            if ((!targetDebuffs.Any(e => e.Equals(hauntSpell, StringComparison.OrdinalIgnoreCase))
                    && CastSpellIfPossible(hauntSpell, true))
                || (!targetDebuffs.Any(e => e.Equals(unstableAfflictionSpell, StringComparison.OrdinalIgnoreCase))
                    && CastSpellIfPossible(unstableAfflictionSpell, true))
                || (!targetDebuffs.Any(e => e.Equals(curseOfAgonySpell, StringComparison.OrdinalIgnoreCase))
                    && CastSpellIfPossible(curseOfAgonySpell, true))
                || !targetDebuffs.Any(e => e.Equals(corruptionSpell, StringComparison.OrdinalIgnoreCase))
                    && CastSpellIfPossible(corruptionSpell, true))
            {
                return true;
            }

            LastDebuffCheck = DateTime.Now;
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
