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
    public class WarlockAffliction : BasicCombatClass
    {
        // author: Jannis Höschele

        private readonly string corruptionSpell = "Corruption";
        private readonly string curseOfAgonySpell = "Curse of Agony";
        private readonly string deathCoilSpell = "Death Coil";
        private readonly string demonArmorSpell = "Demon Armor";
        private readonly string demonSkinSpell = "Demon Skin";
        private readonly string drainSoulSpell = "Drain Soul";
        private readonly int fearAttemptDelay = 5;
        private readonly string fearSpell = "Fear";
        private readonly string felArmorSpell = "Fel Armor";
        private readonly string hauntSpell = "Haunt";
        private readonly string howlOfTerrorSpell = "Howl of Terror";
        private readonly string lifeTapSpell = "Life Tap";
        private readonly string shadowBoltSpell = "Shadow Bolt";
        private readonly string summonFelhunterSpell = "Summon Felhunter";
        private readonly string summonImpSpell = "Summon Imp";
        private readonly string unstableAfflictionSpell = "Unstable Affliction";

        public WarlockAffliction(WowInterface wowInterface) : base(wowInterface)
        {
            PetManager = new PetManager(
                WowInterface.ObjectManager.Pet,
                TimeSpan.FromSeconds(1),
                null,
                () => (WowInterface.CharacterManager.SpellBook.IsSpellKnown(summonFelhunterSpell) && WowInterface.CharacterManager.Inventory.Items.Any(e => e.Name.Equals("Soul Shard", StringComparison.OrdinalIgnoreCase)) && CastSpellIfPossible(summonFelhunterSpell))
                   || (WowInterface.CharacterManager.SpellBook.IsSpellKnown(summonImpSpell) && CastSpellIfPossible(summonImpSpell)),
                null);

            MyAuraManager.BuffsToKeepActive = new Dictionary<string, CastFunction>();

            TargetAuraManager.DebuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { corruptionSpell, () => CastSpellIfPossible(corruptionSpell, true) },
                { curseOfAgonySpell, () => CastSpellIfPossible(curseOfAgonySpell, true) },
                { unstableAfflictionSpell, () => CastSpellIfPossible(unstableAfflictionSpell, true) },
                { hauntSpell, () => CastSpellIfPossible(hauntSpell, true) }
            };

            WowInterface.CharacterManager.SpellBook.OnSpellBookUpdate += SpellBook_OnSpellBookUpdate;
        }

        public override string Author => "Jannis";

        public override WowClass Class => WowClass.Warlock;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "FCFS based CombatClass for the Affliction Warlock spec.";

        public override string Displayname => "Warlock Affliction";

        public override bool HandlesMovement => false;

        public override bool HandlesTargetSelection => false;

        public override bool IsMelee => false;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicIntellectComparator();

        public PetManager PetManager { get; private set; }

        public override CombatClassRole Role => CombatClassRole.Dps;

        public override string Version => "1.0";

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
                || PetManager.Tick()
                || WowInterface.ObjectManager.Player.ManaPercentage < 90
                    && WowInterface.ObjectManager.Player.HealthPercentage > 60
                    && CastSpellIfPossible(lifeTapSpell)
                || (WowInterface.ObjectManager.Player.HealthPercentage < 80
                    && CastSpellIfPossible(deathCoilSpell, true)))
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

            if (CastSpellIfPossible(shadowBoltSpell, true))
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