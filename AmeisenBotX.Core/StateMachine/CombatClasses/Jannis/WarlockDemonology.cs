using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Character.Comparators;
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
    public class WarlockDemonology : ICombatClass
    {
        // author: Jannis Höschele

        private readonly string corruptionSpell = "Corruption";
        private readonly string curseOftheElementsSpell = "Curse of the Elements";
        private readonly string soulfireSpell = "Soul Fire";
        private readonly string incinerateSpell = "Incinerate";
        private readonly string moltenCoreSpell = "Molten Core";
        private readonly string decimationSpell = "Decimation";
        private readonly string metamorphosisSpell = "Metamorphosis";
        private readonly string immolationAuraSpell = "Immolation Aura";
        private readonly string demonicEmpowermentSpell = "Demonic Empowerment";
        private readonly string immolateSpell = "Immolate";
        private readonly string lifeTapSpell = "Life Tap";
        private readonly string drainSoulSpell = "Drain Soul";
        private readonly string drainLifeSpell = "Drain Life";
        private readonly string shadowBoltSpell = "Shadow Bolt";
        private readonly string shadowMasterySpell = "Shadow Mastery";
        private readonly string fearSpell = "Fear";
        private readonly string howlOfTerrorSpell = "Howl of Terror";
        private readonly string demonSkinSpell = "Demon Skin";
        private readonly string demonArmorSpell = "Demon Armor";
        private readonly string felArmorSpell = "Fel Armor";
        private readonly string deathCoilSpell = "Death Coil";
        private readonly string summonImpSpell = "Summon Imp";
        private readonly string summonFelguardSpell = "Summon Felguard";

        private readonly int buffCheckTime = 8;
        private readonly int damageBuffCheckTime = 1;
        private readonly int debuffCheckTime = 1;
        private readonly int fearAttemptDelay = 5;

        public WarlockDemonology(ObjectManager objectManager, CharacterManager characterManager, HookManager hookManager)
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

        private DateTime LastDamageBuffCheck { get; set; }

        private DateTime LastDebuffCheck { get; set; }

        private ObjectManager ObjectManager { get; }

        private CooldownManager CooldownManager { get; }

        private Dictionary<string, Spell> Spells { get; }

        private DateTime LastFearAttempt { get; set; }

        public string Displayname => "Warlock Demonology";

        public string Version => "1.0";

        public string Author => "Jannis";

        public string Description => "FCFS based CombatClass for the Demonology Warlock spec.";

        public WowClass Class => WowClass.Warlock;

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
                || (DateTime.Now - LastDebuffCheck > TimeSpan.FromSeconds(debuffCheckTime)
                    && HandleDebuffing())
                || ObjectManager.Player.ManaPercentage < 20
                    && ObjectManager.Player.HealthPercentage > 60
                    && CastSpellIfPossible(lifeTapSpell)
                || (ObjectManager.Player.HealthPercentage < 80
                    && CastSpellIfPossible(deathCoilSpell, true))
                || (ObjectManager.Player.HealthPercentage < 50
                    && CastSpellIfPossible(drainLifeSpell, true))
                || (DateTime.Now - LastDamageBuffCheck > TimeSpan.FromSeconds(damageBuffCheckTime)
                    && HandleDamageBuffing())
                || CastSpellIfPossible(metamorphosisSpell)
                || CastSpellIfPossible(demonicEmpowermentSpell))
            {
                return;
            }

            if (ObjectManager.Target != null)
            {
                if (ObjectManager.Target.GetType() == typeof(WowPlayer))
                {
                    if (DateTime.Now - LastFearAttempt > TimeSpan.FromSeconds(fearAttemptDelay)
                        && ((ObjectManager.Player.Position.GetDistance(ObjectManager.Target.Position) < 6
                            && CastSpellIfPossible(howlOfTerrorSpell, true))
                        || (ObjectManager.Player.Position.GetDistance(ObjectManager.Target.Position) < 12
                            && CastSpellIfPossible(fearSpell, true))))
                    {
                        LastFearAttempt = DateTime.Now;
                        return;
                    }
                }

                if ((ObjectManager.Player.CurrentlyCastingSpellId == 0
                    && ObjectManager.Player.CurrentlyCastingSpellId == 0
                    && CharacterManager.Inventory.Items.Count(e => e.Name.Equals("Soul Shard", StringComparison.OrdinalIgnoreCase)) < 5
                    && ObjectManager.Target.HealthPercentage < 8
                    && CastSpellIfPossible(drainSoulSpell, true)))
                {
                    return;
                }
            }

            if (CastSpellIfPossible(incinerateSpell, true))
            {
                return;
            }
        }

        public void OutOfCombatExecute()
        {
            if (DateTime.Now - LastBuffCheck > TimeSpan.FromSeconds(buffCheckTime)
                && HandleBuffing())
            {
                return;
            }

            if (ObjectManager.PetGuid == 0
                && SummonPet())
            {
                return;
            }
        }

        private bool SummonPet()
        {
            if (CharacterManager.Inventory.Items.Any(e => e.Name.Equals("Soul Shard", StringComparison.OrdinalIgnoreCase)))
            {
                if (CastSpellIfPossible(summonFelguardSpell, true))
                {
                    return true;
                }
            }
            else
            {
                if (CastSpellIfPossible(summonImpSpell, true))
                {
                    return true;
                }
            }

            return false;
        }

        private bool HandleDamageBuffing()
        {
            List<string> myBuffs = HookManager.GetBuffs(WowLuaUnit.Player);

            if (myBuffs.Any(e => e.Equals(decimationSpell, StringComparison.OrdinalIgnoreCase)))
            {
                if (myBuffs.Any(e => e.Equals(moltenCoreSpell, StringComparison.OrdinalIgnoreCase))
                    && CastSpellIfPossible(soulfireSpell, true))
                {
                    return true;
                }
                else if (CastSpellIfPossible(soulfireSpell, true))
                {
                    return true;
                }
            }
            else if (myBuffs.Any(e => e.Equals(moltenCoreSpell, StringComparison.OrdinalIgnoreCase))
                    && CastSpellIfPossible(incinerateSpell, true))
            {
                return true;
            }

            LastDamageBuffCheck = DateTime.Now;
            return false;
        }

        private bool HandleBuffing()
        {
            List<string> myBuffs = HookManager.GetBuffs(WowLuaUnit.Player);

            if (!ObjectManager.Player.IsInCombat)
            {
                HookManager.TargetGuid(ObjectManager.PlayerGuid);
            }

            if (CharacterManager.SpellBook.IsSpellKnown(felArmorSpell))
            {
                if ((!myBuffs.Any(e => e.Equals(felArmorSpell, StringComparison.OrdinalIgnoreCase))
                    && CastSpellIfPossible(felArmorSpell, true)))
                {
                    return true;
                }
            }
            else if (CharacterManager.SpellBook.IsSpellKnown(demonArmorSpell))
            {
                if ((!myBuffs.Any(e => e.Equals(demonArmorSpell, StringComparison.OrdinalIgnoreCase))
                    && CastSpellIfPossible(demonArmorSpell, true)))
                {
                    return true;
                }
            }
            else if (CharacterManager.SpellBook.IsSpellKnown(demonSkinSpell))
            {
                if ((!myBuffs.Any(e => e.Equals(demonSkinSpell, StringComparison.OrdinalIgnoreCase))
                    && CastSpellIfPossible(demonSkinSpell, true)))
                {
                    return true;
                }
            }

            if (ObjectManager.PetGuid == 0
                && SummonPet())
            {
                return true;
            }

            LastBuffCheck = DateTime.Now;
            return false;
        }

        private bool HandleDebuffing()
        {
            List<string> targetDebuffs = HookManager.GetDebuffs(WowLuaUnit.Target);

            if ((!targetDebuffs.Any(e => e.Equals(curseOftheElementsSpell, StringComparison.OrdinalIgnoreCase))
                    && CastSpellIfPossible(curseOftheElementsSpell, true))
                || (!targetDebuffs.Any(e => e.Equals(immolateSpell, StringComparison.OrdinalIgnoreCase))
                    && CastSpellIfPossible(immolateSpell, true))
                || (!targetDebuffs.Any(e => e.Equals(corruptionSpell, StringComparison.OrdinalIgnoreCase))
                    && CastSpellIfPossible(corruptionSpell, true))
                || (!targetDebuffs.Any(e => e.Equals(shadowMasterySpell, StringComparison.OrdinalIgnoreCase))
                    && CastSpellIfPossible(shadowBoltSpell, true)))
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
