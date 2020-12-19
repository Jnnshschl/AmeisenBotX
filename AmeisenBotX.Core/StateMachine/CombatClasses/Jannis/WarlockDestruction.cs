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
using AmeisenBotX.Core.Character.Spells.Objects;
using static AmeisenBotX.Core.Statemachine.Utils.AuraManager;

namespace AmeisenBotX.Core.Statemachine.CombatClasses.Jannis
{
    public class WarlockDestruction : BasicCombatClass
    {
        public WarlockDestruction(AmeisenBotStateMachine stateMachine) : base(stateMachine)
        {
            PetManager = new PetManager
            (
                WowInterface,
                TimeSpan.FromSeconds(1),
                null,
                () => WowInterface.CharacterManager.SpellBook.IsSpellKnown(summonImpSpell) && TryCastSpell(summonImpSpell, 0, true),
                () => WowInterface.CharacterManager.SpellBook.IsSpellKnown(summonImpSpell) && TryCastSpell(summonImpSpell, 0, true)
            );

            MyAuraManager.BuffsToKeepActive = new Dictionary<string, CastFunction>();

            WowInterface.CharacterManager.SpellBook.OnSpellBookUpdate += () =>
            {
                if (SpellChain.Get(WowInterface.CharacterManager.SpellBook.IsSpellKnown, out string armorToUse, felArmorSpell, demonArmorSpell, demonSkinSpell))
                {
                    MyAuraManager.BuffsToKeepActive.Add(armorToUse, () => TryCastSpell(armorToUse, 0, true));
                }
            };

            TargetAuraManager.DebuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { corruptionSpell, () => !WowInterface.ObjectManager.Target.HasBuffByName(seedOfCorruptionSpell) && TryCastSpell(corruptionSpell, WowInterface.ObjectManager.TargetGuid, true) },
                { curseOfTheElementsSpell, () => TryCastSpell(curseOfTheElementsSpell, WowInterface.ObjectManager.TargetGuid, true) },
                { immolateSpell, () => TryCastSpell(immolateSpell, WowInterface.ObjectManager.TargetGuid, true) }
            };
        }

        public override string Description => "FCFS based CombatClass for the Destruction Warlock spec.";

        public override string Displayname => "Warlock Destruction";

        public override bool HandlesMovement => false;

        public override bool IsMelee => false;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicIntellectComparator(new List<ArmorType>() { ArmorType.SHIELDS });

        public PetManager PetManager { get; private set; }

        public override CombatClassRole Role => CombatClassRole.Dps;

        public override TalentTree Talents { get; } = new TalentTree()
        {
            Tree1 = new Dictionary<int, Talent>(),
            Tree2 = new Dictionary<int, Talent>()
            {
                { 2, new Talent(2, 2, 3) },
                { 3, new Talent(2, 3, 3) },
                { 4, new Talent(2, 4, 1) },
                { 7, new Talent(2, 7, 3) },
                { 9, new Talent(2, 9, 1) },
                { 10, new Talent(2, 10, 1) },
                { 11, new Talent(2, 11, 3) },
                { 12, new Talent(2, 12, 3) },
            },
            Tree3 = new Dictionary<int, Talent>()
            {
                { 2, new Talent(3, 2, 5) },
                { 3, new Talent(3, 3, 2) },
                { 5, new Talent(3, 5, 3) },
                { 6, new Talent(3, 6, 2) },
                { 8, new Talent(3, 8, 5) },
                { 9, new Talent(3, 9, 2) },
                { 10, new Talent(3, 10, 1) },
                { 12, new Talent(3, 12, 3) },
                { 13, new Talent(3, 13, 3) },
                { 14, new Talent(3, 14, 1) },
                { 16, new Talent(3, 16, 5) },
                { 17, new Talent(3, 17, 1) },
                { 19, new Talent(3, 19, 3) },
                { 20, new Talent(3, 20, 5) },
                { 22, new Talent(3, 22, 3) },
                { 24, new Talent(3, 24, 3) },
                { 25, new Talent(3, 25, 5) },
                { 26, new Talent(3, 26, 1) },
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

                if (WowInterface.ObjectManager.Player.ManaPercentage < 20
                        && WowInterface.ObjectManager.Player.HealthPercentage > 60
                        && TryCastSpell(lifeTapSpell, 0)
                    || (WowInterface.ObjectManager.Player.HealthPercentage < 80
                        && TryCastSpell(deathCoilSpell, WowInterface.ObjectManager.TargetGuid, true))
                    || (WowInterface.ObjectManager.Player.HealthPercentage < 50
                        && TryCastSpell(drainLifeSpell, WowInterface.ObjectManager.TargetGuid, true)))
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

                if (TryCastSpell(chaosBoltSpell, WowInterface.ObjectManager.TargetGuid, true)
                    // || CastSpellIfPossible(conflagrateSpell, WowInterface.ObjectManager.TargetGuid, true)
                    || TryCastSpell(incinerateSpell, WowInterface.ObjectManager.TargetGuid, true))
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