using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObjects;
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
        public WarlockAffliction(AmeisenBotStateMachine stateMachine) : base(stateMachine)
        {
            PetManager = new PetManager
            (
                WowInterface,
                TimeSpan.FromSeconds(1),
                null,
                () => (WowInterface.CharacterManager.SpellBook.IsSpellKnown(summonFelhunterSpell)
                       && WowInterface.CharacterManager.Inventory.HasItemByName("Soul Shard")
                       && TryCastSpell(summonFelhunterSpell, 0))
                   || (WowInterface.CharacterManager.SpellBook.IsSpellKnown(summonImpSpell)
                       && TryCastSpell(summonImpSpell, 0)),
                () => (WowInterface.CharacterManager.SpellBook.IsSpellKnown(summonFelhunterSpell)
                       && WowInterface.CharacterManager.Inventory.HasItemByName("Soul Shard")
                       && TryCastSpell(summonFelhunterSpell, 0))
                   || (WowInterface.CharacterManager.SpellBook.IsSpellKnown(summonImpSpell)
                       && TryCastSpell(summonImpSpell, 0))
            );

            MyAuraManager.BuffsToKeepActive = new Dictionary<string, CastFunction>();

            if (SpellChain.Get(WowInterface.CharacterManager.SpellBook.IsSpellKnown, out string armorToUse, felArmorSpell, demonArmorSpell, demonSkinSpell))
            {
                MyAuraManager.BuffsToKeepActive.Add(armorToUse, () => TryCastSpell(armorToUse, 0, true));
            }

            TargetAuraManager.DebuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { corruptionSpell, () => !WowInterface.ObjectManager.Target.HasBuffByName(seedOfCorruptionSpell) && TryCastSpell(corruptionSpell, WowInterface.ObjectManager.TargetGuid, true) },
                { curseOfAgonySpell, () => TryCastSpell(curseOfAgonySpell, WowInterface.ObjectManager.TargetGuid, true) },
                { unstableAfflictionSpell, () => TryCastSpell(unstableAfflictionSpell, WowInterface.ObjectManager.TargetGuid, true) },
                { hauntSpell, () => TryCastSpell(hauntSpell, WowInterface.ObjectManager.TargetGuid, true) }
            };
        }

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

        public override WowClass WowClass => WowClass.Warlock;

        private DateTime LastFearAttempt { get; set; }

        public override void Execute()
        {
            base.Execute();

            if (SelectTarget(TargetManagerDps))
            {
                if (PetManager.Tick()) { return; }

                if (WowInterface.ObjectManager.Player.ManaPercentage < 90
                        && WowInterface.ObjectManager.Player.HealthPercentage > 60
                        && TryCastSpell(lifeTapSpell, 0)
                    || (WowInterface.ObjectManager.Player.HealthPercentage < 80
                        && TryCastSpell(deathCoilSpell, WowInterface.ObjectManager.TargetGuid, true)))
                {
                    return;
                }

                if (WowInterface.ObjectManager.Target != null)
                {
                    if (WowInterface.ObjectManager.Target.GetType() == typeof(WowPlayer))
                    {
                        if (DateTime.Now - LastFearAttempt > TimeSpan.FromSeconds(5)
                            && ((WowInterface.ObjectManager.Player.Position.GetDistance(WowInterface.ObjectManager.Target.Position) < 6
                                && TryCastSpell(howlOfTerrorSpell, 0, true))
                            || (WowInterface.ObjectManager.Player.Position.GetDistance(WowInterface.ObjectManager.Target.Position) < 12
                                && TryCastSpell(fearSpell, WowInterface.ObjectManager.TargetGuid, true))))
                        {
                            LastFearAttempt = DateTime.Now;
                            return;
                        }
                    }

                    if (WowInterface.CharacterManager.Inventory.Items.Count(e => e.Name.Equals("Soul Shard", StringComparison.OrdinalIgnoreCase)) < 5
                        && WowInterface.ObjectManager.Target.HealthPercentage < 25.0
                        && TryCastSpell(drainSoulSpell, WowInterface.ObjectManager.TargetGuid, true))
                    {
                        return;
                    }
                }

                if (WowInterface.ObjectManager.GetNearEnemies<WowUnit>(WowInterface.ObjectManager.Target.Position, 16.0).Count() > 2
                    && !WowInterface.ObjectManager.Target.HasBuffByName(seedOfCorruptionSpell)
                    && TryCastSpell(seedOfCorruptionSpell, WowInterface.ObjectManager.TargetGuid, true))
                {
                    return;
                }

                if (TryCastSpell(shadowBoltSpell, WowInterface.ObjectManager.TargetGuid, true))
                {
                    return;
                }
            }
        }

        public override void OutOfCombatExecute()
        {
            base.OutOfCombatExecute();

            if (PetManager.Tick())
            {
                return;
            }
        }
    }
}