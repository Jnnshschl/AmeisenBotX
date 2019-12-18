using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Character.Comparators;
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

namespace AmeisenBotX.Core.StateMachine.CombatClasses.Jannis
{
    public class WarlockDemonology : BasicCombatClass
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

        private readonly int damageBuffCheckTime = 1;
        private readonly int fearAttemptDelay = 5;

        public WarlockDemonology(ObjectManager objectManager, CharacterManager characterManager, HookManager hookManager) : base(objectManager, characterManager, hookManager)
        {
            PetManager = new PetManager(
                ObjectManager.Pet,
                TimeSpan.FromSeconds(1),
                null,
                () => (CharacterManager.SpellBook.IsSpellKnown(summonFelguardSpell) && CharacterManager.Inventory.Items.Any(e => e.Name.Equals("Soul Shard", StringComparison.OrdinalIgnoreCase)) && CastSpellIfPossible(summonFelguardSpell))
                   || (CharacterManager.SpellBook.IsSpellKnown(summonImpSpell) && CastSpellIfPossible(summonImpSpell)),
                null);

            BuffsToKeepOnMe = new Dictionary<string, CastFunction>()
            {
                { felArmorSpell, () => CharacterManager.SpellBook.IsSpellKnown(felArmorSpell) && CastSpellIfPossible(felArmorSpell, true) },
                { demonArmorSpell, () => CharacterManager.SpellBook.IsSpellKnown(demonArmorSpell) && CastSpellIfPossible(demonArmorSpell, true) },
                { demonSkinSpell, () => CharacterManager.SpellBook.IsSpellKnown(demonSkinSpell) && CastSpellIfPossible(demonSkinSpell, true) }
            };

            DebuffsToKeepOnTarget = new Dictionary<string, CastFunction>()
            {
                { corruptionSpell, () => CastSpellIfPossible(corruptionSpell, true) },
                { curseOftheElementsSpell, () => CastSpellIfPossible(curseOftheElementsSpell, true) },
                { immolateSpell, () => CastSpellIfPossible(immolateSpell, true) }
            };
        }

        public override bool HandlesMovement => false;

        public override bool HandlesTargetSelection => false;

        public override bool IsMelee => false;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicIntellectComparator();

        private DateTime LastDamageBuffCheck { get; set; }

        private DateTime LastFearAttempt { get; set; }

        public override string Displayname => "Warlock Demonology";

        public override string Version => "1.0";

        public override string Author => "Jannis";

        public override string Description => "FCFS based CombatClass for the Demonology Warlock spec.";

        public override WowClass Class => WowClass.Warlock;

        public override CombatClassRole Role => CombatClassRole.Dps;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public PetManager PetManager { get; private set; }

        public override void Execute()
        {
            // we dont want to do anything if we are casting something...
            if (ObjectManager.Player.IsCasting)
            {
                return;
            }

            if (MyAuraManager.Tick()
                || TargetAuraManager.Tick()
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

                if (!ObjectManager.Player.IsCasting
                    && CharacterManager.Inventory.Items.Count(e => e.Name.Equals("Soul Shard", StringComparison.OrdinalIgnoreCase)) < 5
                    && ObjectManager.Target.HealthPercentage < 8
                    && CastSpellIfPossible(drainSoulSpell, true))
                {
                    return;
                }
            }

            if (CastSpellIfPossible(incinerateSpell, true))
            {
                return;
            }
        }

        public override void OutOfCombatExecute()
        {
            if (MyAuraManager.Tick()
                || PetManager.Tick())
            {
                return;
            }
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
