using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Statemachine.Enums;
using AmeisenBotX.Core.Statemachine.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using static AmeisenBotX.Core.Statemachine.Utils.AuraManager;

namespace AmeisenBotX.Core.Statemachine.CombatClasses.Jannis
{
    public class WarlockDemonology : BasicCombatClass
    {
        // author: Jannis Höschele

        private readonly string corruptionSpell = "Corruption";
        private readonly string curseOftheElementsSpell = "Curse of the Elements";
        private readonly int damageBuffCheckTime = 1;
        private readonly string deathCoilSpell = "Death Coil";
        private readonly string decimationSpell = "Decimation";
        private readonly string demonArmorSpell = "Demon Armor";
        private readonly string demonicEmpowermentSpell = "Demonic Empowerment";
        private readonly string demonSkinSpell = "Demon Skin";
        private readonly string drainLifeSpell = "Drain Life";
        private readonly string drainSoulSpell = "Drain Soul";
        private readonly int fearAttemptDelay = 5;
        private readonly string fearSpell = "Fear";
        private readonly string felArmorSpell = "Fel Armor";
        private readonly string howlOfTerrorSpell = "Howl of Terror";
        private readonly string immolateSpell = "Immolate";
        private readonly string immolationAuraSpell = "Immolation Aura";
        private readonly string incinerateSpell = "Incinerate";
        private readonly string lifeTapSpell = "Life Tap";
        private readonly string metamorphosisSpell = "Metamorphosis";
        private readonly string moltenCoreSpell = "Molten Core";
        private readonly string shadowBoltSpell = "Shadow Bolt";
        private readonly string shadowMasterySpell = "Shadow Mastery";
        private readonly string soulfireSpell = "Soul Fire";
        private readonly string summonFelguardSpell = "Summon Felguard";
        private readonly string summonImpSpell = "Summon Imp";

        public WarlockDemonology(WowInterface wowInterface) : base(wowInterface)
        {
            PetManager = new PetManager(
                WowInterface.ObjectManager.Pet,
                TimeSpan.FromSeconds(1),
                null,
                () => (WowInterface.CharacterManager.SpellBook.IsSpellKnown(summonFelguardSpell) && WowInterface.CharacterManager.Inventory.Items.Any(e => e.Name.Equals("Soul Shard", StringComparison.OrdinalIgnoreCase)) && CastSpellIfPossible(summonFelguardSpell))
                   || (WowInterface.CharacterManager.SpellBook.IsSpellKnown(summonImpSpell) && CastSpellIfPossible(summonImpSpell)),
                null);

            MyAuraManager.BuffsToKeepActive = new Dictionary<string, CastFunction>();

            TargetAuraManager.DebuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { corruptionSpell, () => CastSpellIfPossible(corruptionSpell, true) },
                { curseOftheElementsSpell, () => CastSpellIfPossible(curseOftheElementsSpell, true) },
                { immolateSpell, () => CastSpellIfPossible(immolateSpell, true) }
            };

            WowInterface.CharacterManager.SpellBook.OnSpellBookUpdate += SpellBook_OnSpellBookUpdate;
        }

        public override string Author => "Jannis";

        public override WowClass Class => WowClass.Warlock;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "FCFS based CombatClass for the Demonology Warlock spec.";

        public override string Displayname => "Warlock Demonology";

        public override bool HandlesMovement => false;

        public override bool HandlesTargetSelection => false;

        public override bool IsMelee => false;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicIntellectComparator();

        public PetManager PetManager { get; private set; }

        public override CombatClassRole Role => CombatClassRole.Dps;

        public override string Version => "1.0";

        private DateTime LastDamageBuffCheck { get; set; }

        private DateTime LastFearAttempt { get; set; }

        public override void Execute()
        {
            // we dont want to do anything if we are casting something...
            if (WowInterface.ObjectManager.Player.IsCasting)
            {
                return;
            }

            if (MyAuraManager.Tick()
                || TargetAuraManager.Tick()
                || WowInterface.ObjectManager.Player.ManaPercentage < 20
                    && WowInterface.ObjectManager.Player.HealthPercentage > 60
                    && CastSpellIfPossible(lifeTapSpell)
                || (WowInterface.ObjectManager.Player.HealthPercentage < 80
                    && CastSpellIfPossible(deathCoilSpell, true))
                || (WowInterface.ObjectManager.Player.HealthPercentage < 50
                    && CastSpellIfPossible(drainLifeSpell, true))
                || (DateTime.Now - LastDamageBuffCheck > TimeSpan.FromSeconds(damageBuffCheckTime)
                    && HandleDamageBuffing())
                || CastSpellIfPossible(metamorphosisSpell)
                || (WowInterface.ObjectManager.Pet != null && CastSpellIfPossible(demonicEmpowermentSpell)))
            {
                return;
            }

            if (WowInterface.ObjectManager.Target != null)
            {
                if (WowInterface.ObjectManager.Target.GetType() == typeof(WowPlayer))
                {
                    if (DateTime.Now - LastFearAttempt > TimeSpan.FromSeconds(fearAttemptDelay)
                        && ((WowInterface.ObjectManager.Player.Position.GetDistance(WowInterface.ObjectManager.Target.Position) < 6
                            && CastSpellIfPossible(howlOfTerrorSpell, true))
                        || (WowInterface.ObjectManager.Player.Position.GetDistance(WowInterface.ObjectManager.Target.Position) < 12
                            && CastSpellIfPossible(fearSpell, true))))
                    {
                        LastFearAttempt = DateTime.Now;
                        return;
                    }
                }

                if (!WowInterface.ObjectManager.Player.IsCasting
                    && WowInterface.CharacterManager.Inventory.Items.Count(e => e.Name.Equals("Soul Shard", StringComparison.OrdinalIgnoreCase)) < 5
                    && WowInterface.ObjectManager.Target.HealthPercentage < 8
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

        private void SpellBook_OnSpellBookUpdate()
        {
            if (WowInterface.CharacterManager.SpellBook.IsSpellKnown(felArmorSpell))
            {
                MyAuraManager.BuffsToKeepActive.Add(felArmorSpell, () => WowInterface.CharacterManager.SpellBook.IsSpellKnown(felArmorSpell) && CastSpellIfPossible(felArmorSpell, true));
            }
            else if (WowInterface.CharacterManager.SpellBook.IsSpellKnown(demonArmorSpell))
            {
                MyAuraManager.BuffsToKeepActive.Add(demonArmorSpell, () => WowInterface.CharacterManager.SpellBook.IsSpellKnown(demonArmorSpell) && CastSpellIfPossible(demonArmorSpell, true));
            }
            else if (WowInterface.CharacterManager.SpellBook.IsSpellKnown(demonSkinSpell))
            {
                MyAuraManager.BuffsToKeepActive.Add(demonSkinSpell, () => WowInterface.CharacterManager.SpellBook.IsSpellKnown(demonSkinSpell) && CastSpellIfPossible(demonSkinSpell, true));
            }
        }
    }
}