using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
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
    public class WarlockDestruction : BasicCombatClass
    {
        // author: Jannis Höschele

#pragma warning disable IDE0051
        private const string chaosBoltSpell = "Chaos Bolt";
        private const string conflagrateSpell = "Conflagrate";
        private const string corruptionSpell = "Corruption";
        private const string curseOftheElementsSpell = "Curse of the Elements";
        private const string deathCoilSpell = "Death Coil";
        private const string demonArmorSpell = "Demon Armor";
        private const string demonSkinSpell = "Demon Skin";
        private const string drainLifeSpell = "Drain Life";
        private const string drainSoulSpell = "Drain Soul";
        private const int fearAttemptDelay = 5;
        private const string fearSpell = "Fear";
        private const string felArmorSpell = "Fel Armor";
        private const string howlOfTerrorSpell = "Howl of Terror";
        private const string immolateSpell = "Immolate";
        private const string incinerateSpell = "Incinerate";
        private const string lifeTapSpell = "Life Tap";
        private const string summonImpSpell = "Summon Imp";
#pragma warning restore IDE0051

        public WarlockDestruction(WowInterface wowInterface, AmeisenBotStateMachine stateMachine) : base(wowInterface, stateMachine)
        {
            PetManager = new PetManager(
                WowInterface.ObjectManager.Pet,
                TimeSpan.FromSeconds(1),
                null,
                () => WowInterface.CharacterManager.SpellBook.IsSpellKnown(summonImpSpell) && CastSpellIfPossible(summonImpSpell, 0),
                null);

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

        public override string Description => "FCFS based CombatClass for the Destruction Warlock spec.";

        public override string Displayname => "Warlock Destruction";

        public override bool HandlesMovement => false;

        public override bool HandlesTargetSelection => false;

        public override bool IsMelee => false;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicIntellectComparator(new List<ArmorType>() { ArmorType.SHIEDLS });

        public PetManager PetManager { get; private set; }

        public override CombatClassRole Role => CombatClassRole.Dps;

        public override string Version => "1.0";

        private DateTime LastFearAttempt { get; set; }

        public override void ExecuteCC()
        {
            if (PetManager.Tick()
                || WowInterface.ObjectManager.Player.ManaPercentage < 20
                    && WowInterface.ObjectManager.Player.HealthPercentage > 60
                    && CastSpellIfPossible(lifeTapSpell, 0)
                || (WowInterface.ObjectManager.Player.HealthPercentage < 80
                    && CastSpellIfPossible(deathCoilSpell, WowInterface.ObjectManager.TargetGuid, true))
                || (WowInterface.ObjectManager.Player.HealthPercentage < 50
                    && CastSpellIfPossible(drainLifeSpell, WowInterface.ObjectManager.TargetGuid, true)))
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

            if (CastSpellIfPossible(chaosBoltSpell, WowInterface.ObjectManager.TargetGuid, true)
                || CastSpellIfPossible(conflagrateSpell, WowInterface.ObjectManager.TargetGuid, true)
                || CastSpellIfPossible(incinerateSpell, WowInterface.ObjectManager.TargetGuid, true))
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