using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Talents.Objects;
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

#pragma warning disable IDE0051
        private const string corruptionSpell = "Corruption";
        private const string curseOftheElementsSpell = "Curse of the Elements";
        private const string deathCoilSpell = "Death Coil";
        private const string decimationSpell = "Decimation";
        private const string demonArmorSpell = "Demon Armor";
        private const string demonicEmpowermentSpell = "Demonic Empowerment";
        private const string demonSkinSpell = "Demon Skin";
        private const string drainLifeSpell = "Drain Life";
        private const string drainSoulSpell = "Drain Soul";
        private const int fearAttemptDelay = 5;
        private const string fearSpell = "Fear";
        private const string felArmorSpell = "Fel Armor";
        private const string howlOfTerrorSpell = "Howl of Terror";
        private const string immolateSpell = "Immolate";
        private const string immolationAuraSpell = "Immolation Aura";
        private const string incinerateSpell = "Incinerate";
        private const string lifeTapSpell = "Life Tap";
        private const string metamorphosisSpell = "Metamorphosis";
        private const string moltenCoreSpell = "Molten Core";
        private const string shadowBoltSpell = "Shadow Bolt";
        private const string shadowMasterySpell = "Shadow Mastery";
        private const string soulfireSpell = "Soul Fire";
        private const string summonFelguardSpell = "Summon Felguard";
        private const string summonImpSpell = "Summon Imp";
#pragma warning restore IDE0051

        public WarlockDemonology(WowInterface wowInterface, AmeisenBotStateMachine stateMachine) : base(wowInterface, stateMachine)
        {
            PetManager = new PetManager(
                WowInterface,
                TimeSpan.FromSeconds(1),
                null,
                () => (WowInterface.CharacterManager.SpellBook.IsSpellKnown(summonFelguardSpell) && WowInterface.CharacterManager.Inventory.Items.Any(e => e.Name.Equals("Soul Shard", StringComparison.OrdinalIgnoreCase)) && CastSpellIfPossible(summonFelguardSpell, 0))
                   || (WowInterface.CharacterManager.SpellBook.IsSpellKnown(summonImpSpell) && CastSpellIfPossible(summonImpSpell, 0)),
                () => (WowInterface.CharacterManager.SpellBook.IsSpellKnown(summonFelguardSpell) && WowInterface.CharacterManager.Inventory.Items.Any(e => e.Name.Equals("Soul Shard", StringComparison.OrdinalIgnoreCase)) && CastSpellIfPossible(summonFelguardSpell, 0))
                   || (WowInterface.CharacterManager.SpellBook.IsSpellKnown(summonImpSpell) && CastSpellIfPossible(summonImpSpell, 0)));

            MyAuraManager.BuffsToKeepActive = new Dictionary<string, CastFunction>();

            TargetAuraManager.DebuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { corruptionSpell, () => CastSpellIfPossible(corruptionSpell, WowInterface.ObjectManager.TargetGuid, true) },
                { curseOftheElementsSpell, () => CastSpellIfPossible(curseOftheElementsSpell, WowInterface.ObjectManager.TargetGuid, true) },
                { immolateSpell, () => CastSpellIfPossible(immolateSpell, WowInterface.ObjectManager.TargetGuid, true) }
            };

            WowInterface.CharacterManager.SpellBook.OnSpellBookUpdate += SpellBook_OnSpellBookUpdate;
        }

        public override string Author => "Jannis";

        public override WowClass Class => WowClass.Warlock;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "FCFS based CombatClass for the Demonology Warlock spec.";

        public override string Displayname => "Warlock Demonology";

        public override bool HandlesMovement => false;

        public override bool IsMelee => false;

        public override bool WalkBehindEnemy => false;

        public override bool UseAutoAttacks => false;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicIntellectComparator(new List<ArmorType>() { ArmorType.SHIELDS });

        public PetManager PetManager { get; private set; }

        public override CombatClassRole Role => CombatClassRole.Dps;

        public override string Version => "1.0";

        private DateTime LastFearAttempt { get; set; }

        public override TalentTree Talents { get; } = new TalentTree()
        {
            Tree1 = new Dictionary<int, Talent>(),
            Tree2 = new Dictionary<int, Talent>()
            {
                { 3, new Talent(2, 3, 3) },
                { 4, new Talent(2, 4, 2) },
                { 6, new Talent(2, 6, 3) },
                { 7, new Talent(2, 7, 3) },
                { 9, new Talent(2, 9, 1) },
                { 10, new Talent(2, 10, 1) },
                { 11, new Talent(2, 11, 3) },
                { 12, new Talent(2, 12, 5) },
                { 13, new Talent(2, 13, 2) },
                { 15, new Talent(2, 15, 2) },
                { 16, new Talent(2, 16, 5) },
                { 17, new Talent(2, 17, 3) },
                { 19, new Talent(2, 19, 1) },
                { 20, new Talent(2, 20, 3) },
                { 21, new Talent(2, 21, 5) },
                { 22, new Talent(2, 22, 2) },
                { 23, new Talent(2, 23, 3) },
                { 24, new Talent(2, 24, 1) },
                { 25, new Talent(2, 25, 3) },
                { 26, new Talent(2, 26, 5) },
                { 27, new Talent(2, 27, 1) },
            },
            Tree3 = new Dictionary<int, Talent>()
            {
                { 1, new Talent(3, 1, 5) },
                { 2, new Talent(3, 2, 5) },
                { 8, new Talent(3, 8, 4) },
            },
        };

        public override void ExecuteCC()
        {
            if (PetManager.Tick()) { return; }

            if (WowInterface.ObjectManager.Player.ManaPercentage < 20
                    && WowInterface.ObjectManager.Player.HealthPercentage > 60
                    && CastSpellIfPossible(lifeTapSpell, 0)
                || (WowInterface.ObjectManager.Player.HealthPercentage < 80
                    && CastSpellIfPossible(deathCoilSpell, WowInterface.ObjectManager.TargetGuid, true))
                || (WowInterface.ObjectManager.Player.HealthPercentage < 50
                    && CastSpellIfPossible(drainLifeSpell, WowInterface.ObjectManager.TargetGuid, true))
                || CastSpellIfPossible(metamorphosisSpell, 0)
                || (WowInterface.ObjectManager.Pet?.Health > 0 && CastSpellIfPossible(demonicEmpowermentSpell, 0)))
            {
                return;
            }

            if (WowInterface.ObjectManager.Target != null)
            {
                if (WowInterface.ObjectManager.Target.GetType() == typeof(WowPlayer))
                {
                    if (DateTime.Now - LastFearAttempt > TimeSpan.FromSeconds(fearAttemptDelay)
                        && ((WowInterface.ObjectManager.Player.Position.GetDistance(WowInterface.ObjectManager.Target.Position) < 6
                            && CastSpellIfPossible(howlOfTerrorSpell, 0, true))
                        || (WowInterface.ObjectManager.Player.Position.GetDistance(WowInterface.ObjectManager.Target.Position) < 12
                            && CastSpellIfPossible(fearSpell, WowInterface.ObjectManager.TargetGuid, true))))
                    {
                        LastFearAttempt = DateTime.Now;
                        return;
                    }
                }

                if (!WowInterface.ObjectManager.Player.IsCasting
                    && WowInterface.CharacterManager.Inventory.Items.Count(e => e.Name.Equals("Soul Shard", StringComparison.OrdinalIgnoreCase)) < 5
                    && WowInterface.ObjectManager.Target.HealthPercentage < 8
                    && CastSpellIfPossible(drainSoulSpell, WowInterface.ObjectManager.TargetGuid, true))
                {
                    return;
                }
            }

            if (CastSpellIfPossible(incinerateSpell, WowInterface.ObjectManager.TargetGuid, true))
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
                MyAuraManager.BuffsToKeepActive.Add(felArmorSpell, () => WowInterface.CharacterManager.SpellBook.IsSpellKnown(felArmorSpell) && CastSpellIfPossible(felArmorSpell, 0, true));
            }
            else if (WowInterface.CharacterManager.SpellBook.IsSpellKnown(demonArmorSpell))
            {
                MyAuraManager.BuffsToKeepActive.Add(demonArmorSpell, () => WowInterface.CharacterManager.SpellBook.IsSpellKnown(demonArmorSpell) && CastSpellIfPossible(demonArmorSpell, 0, true));
            }
            else if (WowInterface.CharacterManager.SpellBook.IsSpellKnown(demonSkinSpell))
            {
                MyAuraManager.BuffsToKeepActive.Add(demonSkinSpell, () => WowInterface.CharacterManager.SpellBook.IsSpellKnown(demonSkinSpell) && CastSpellIfPossible(demonSkinSpell, 0, true));
            }
        }
    }
}