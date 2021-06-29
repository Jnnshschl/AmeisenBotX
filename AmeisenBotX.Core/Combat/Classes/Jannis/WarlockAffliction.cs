using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Fsm;
using AmeisenBotX.Core.Fsm.Utils.Auras.Objects;
using AmeisenBotX.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Combat.Classes.Jannis
{
    public class WarlockAffliction : BasicCombatClass
    {
        public WarlockAffliction(WowInterface wowInterface, AmeisenBotFsm stateMachine) : base(wowInterface, stateMachine)
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

            MyAuraManager.Jobs.Add(new KeepBestActiveAuraJob(new List<(string, Func<bool>)>()
            {
                (felArmorSpell, () => TryCastSpell(felArmorSpell, 0, true)),
                (demonArmorSpell, () => TryCastSpell(demonArmorSpell, 0, true)),
                (demonSkinSpell, () => TryCastSpell(demonSkinSpell, 0, true)),
            }));

            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(corruptionSpell, () => WowInterface.Target != null && !WowInterface.Target.HasBuffByName(seedOfCorruptionSpell) && TryCastSpell(corruptionSpell, WowInterface.Target.Guid, true)));
            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(curseOfAgonySpell, () => TryCastSpell(curseOfAgonySpell, WowInterface.Target.Guid, true)));
            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(unstableAfflictionSpell, () => TryCastSpell(unstableAfflictionSpell, WowInterface.Target.Guid, true)));
            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(hauntSpell, () => TryCastSpell(hauntSpell, WowInterface.Target.Guid, true)));
        }

        public override string Description => "FCFS based CombatClass for the Affliction Warlock spec.";

        public override string Displayname => "Warlock Affliction";

        public override bool HandlesMovement => false;

        public override bool IsMelee => false;

        public override IItemComparator ItemComparator { get; set; } = new BasicIntellectComparator(new() { WowArmorType.SHIELDS });

        public PetManager PetManager { get; private set; }

        public override WowRole Role => WowRole.Dps;

        public override TalentTree Talents { get; } = new()
        {
            Tree1 = new()
            {
                { 1, new(1, 1, 2) },
                { 2, new(1, 2, 3) },
                { 3, new(1, 3, 5) },
                { 7, new(1, 7, 2) },
                { 9, new(1, 9, 3) },
                { 12, new(1, 12, 2) },
                { 13, new(1, 13, 3) },
                { 14, new(1, 14, 5) },
                { 15, new(1, 15, 1) },
                { 17, new(1, 17, 2) },
                { 18, new(1, 18, 5) },
                { 19, new(1, 19, 3) },
                { 20, new(1, 20, 5) },
                { 23, new(1, 23, 3) },
                { 24, new(1, 24, 3) },
                { 25, new(1, 25, 1) },
                { 26, new(1, 26, 1) },
                { 27, new(1, 27, 5) },
                { 28, new(1, 28, 1) },
            },
            Tree2 = new(),
            Tree3 = new()
            {
                { 1, new(3, 1, 5) },
                { 2, new(3, 2, 5) },
                { 8, new(3, 8, 5) },
                { 9, new(3, 9, 1) },
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

            if (SelectTarget(TargetProviderDps))
            {
                if (PetManager.Tick()) { return; }

                if (WowInterface.Player.ManaPercentage < 90
                        && WowInterface.Player.HealthPercentage > 60
                        && TryCastSpell(lifeTapSpell, 0)
                    || (WowInterface.Player.HealthPercentage < 80
                        && TryCastSpell(deathCoilSpell, WowInterface.Target.Guid, true)))
                {
                    return;
                }

                if (WowInterface.Target != null)
                {
                    if (WowInterface.Target.GetType() == typeof(WowPlayer))
                    {
                        if (DateTime.UtcNow - LastFearAttempt > TimeSpan.FromSeconds(5)
                            && ((WowInterface.Player.Position.GetDistance(WowInterface.Target.Position) < 6.0f
                                && TryCastSpell(howlOfTerrorSpell, 0, true))
                            || (WowInterface.Player.Position.GetDistance(WowInterface.Target.Position) < 12.0f
                                && TryCastSpell(fearSpell, WowInterface.Target.Guid, true))))
                        {
                            LastFearAttempt = DateTime.UtcNow;
                            return;
                        }
                    }

                    if (WowInterface.CharacterManager.Inventory.Items.Count(e => e.Name.Equals("Soul Shard", StringComparison.OrdinalIgnoreCase)) < 5
                        && WowInterface.Target.HealthPercentage < 25.0
                        && TryCastSpell(drainSoulSpell, WowInterface.Target.Guid, true))
                    {
                        return;
                    }
                }

                if (WowInterface.Objects.GetNearEnemies<WowUnit>(WowInterface.NewWowInterface, WowInterface.Target.Position, 16.0f).Count() > 2
                    && !WowInterface.Target.HasBuffByName(seedOfCorruptionSpell)
                    && TryCastSpell(seedOfCorruptionSpell, WowInterface.Target.Guid, true))
                {
                    return;
                }

                if (TryCastSpell(shadowBoltSpell, WowInterface.Target.Guid, true))
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