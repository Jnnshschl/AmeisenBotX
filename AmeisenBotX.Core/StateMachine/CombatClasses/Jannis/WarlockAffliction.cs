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
    public class WarlockAffliction : BasicCombatClass
    {
        public WarlockAffliction(WowInterface wowInterface, AmeisenBotStateMachine stateMachine) : base(wowInterface, stateMachine)
        {
            PetManager = new PetManager(WowInterface,
                TimeSpan.FromSeconds(1),
                null,
                () => (WowInterface.CharacterManager.SpellBook.IsSpellKnown(summonFelhunterSpell) && WowInterface.CharacterManager.Inventory.Items.Any(e => e.Name.Equals("Soul Shard", StringComparison.OrdinalIgnoreCase)) && CastSpellIfPossible(summonFelhunterSpell, 0))
                   || (WowInterface.CharacterManager.SpellBook.IsSpellKnown(summonImpSpell) && CastSpellIfPossible(summonImpSpell, 0)),
                () => (WowInterface.CharacterManager.SpellBook.IsSpellKnown(summonFelhunterSpell) && WowInterface.CharacterManager.Inventory.Items.Any(e => e.Name.Equals("Soul Shard", StringComparison.OrdinalIgnoreCase)) && CastSpellIfPossible(summonFelhunterSpell, 0))
                   || (WowInterface.CharacterManager.SpellBook.IsSpellKnown(summonImpSpell) && CastSpellIfPossible(summonImpSpell, 0)));

            MyAuraManager.BuffsToKeepActive = new Dictionary<string, CastFunction>();

            TargetAuraManager.DebuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { corruptionSpell, () => CastSpellIfPossible(corruptionSpell, WowInterface.ObjectManager.TargetGuid, true) },
                { curseOfAgonySpell, () => CastSpellIfPossible(curseOfAgonySpell, WowInterface.ObjectManager.TargetGuid, true) },
                { unstableAfflictionSpell, () => CastSpellIfPossible(unstableAfflictionSpell, WowInterface.ObjectManager.TargetGuid, true) },
                { hauntSpell, () => CastSpellIfPossible(hauntSpell, WowInterface.ObjectManager.TargetGuid, true) }
            };

            WowInterface.CharacterManager.SpellBook.OnSpellBookUpdate += SpellBook_OnSpellBookUpdate;
        }

        public override string Author => "Jannis";

        public override WowClass Class => WowClass.Warlock;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "FCFS based CombatClass for the Affliction Warlock spec.";

        public override string Displayname => "Warlock Affliction";

        public override bool HandlesMovement => false;

        public override bool IsMelee => false;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicIntellectComparator(new List<ArmorType>() { ArmorType.SHIELDS });

        public PetManager PetManager { get; private set; }

        public override CombatClassRole Role => CombatClassRole.Dps;

        public override TalentTree Talents { get; } = new TalentTree()
        {
            Tree1 = new Dictionary<int, Talent>()
            {
                { 1, new Talent(1, 1, 2) },
                { 2, new Talent(1, 2, 3) },
                { 3, new Talent(1, 3, 5) },
                { 7, new Talent(1, 7, 2) },
                { 9, new Talent(1, 9, 3) },
                { 12, new Talent(1, 12, 2) },
                { 13, new Talent(1, 13, 3) },
                { 14, new Talent(1, 14, 5) },
                { 15, new Talent(1, 15, 1) },
                { 17, new Talent(1, 17, 2) },
                { 18, new Talent(1, 18, 5) },
                { 19, new Talent(1, 19, 3) },
                { 20, new Talent(1, 20, 5) },
                { 23, new Talent(1, 23, 3) },
                { 24, new Talent(1, 24, 3) },
                { 25, new Talent(1, 25, 1) },
                { 26, new Talent(1, 26, 1) },
                { 27, new Talent(1, 27, 5) },
                { 28, new Talent(1, 28, 1) },
            },
            Tree2 = new Dictionary<int, Talent>(),
            Tree3 = new Dictionary<int, Talent>()
            {
                { 1, new Talent(3, 1, 5) },
                { 2, new Talent(3, 2, 5) },
                { 8, new Talent(3, 8, 5) },
                { 9, new Talent(3, 9, 1) },
            },
        };

        public override bool UseAutoAttacks => false;

        public override string Version => "1.0";

        public override bool WalkBehindEnemy => false;

        private DateTime LastFearAttempt { get; set; }

        public override void ExecuteCC()
        {
            if (PetManager.Tick()) { return; }

            if (WowInterface.ObjectManager.Player.ManaPercentage < 90
                    && WowInterface.ObjectManager.Player.HealthPercentage > 60
                    && CastSpellIfPossible(lifeTapSpell, 0)
                || (WowInterface.ObjectManager.Player.HealthPercentage < 80
                    && CastSpellIfPossible(deathCoilSpell, WowInterface.ObjectManager.TargetGuid, true)))
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

            if (CastSpellIfPossible(shadowBoltSpell, WowInterface.ObjectManager.TargetGuid, true))
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