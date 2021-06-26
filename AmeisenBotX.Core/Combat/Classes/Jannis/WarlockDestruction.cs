using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Fsm;
using AmeisenBotX.Core.Fsm.Utils.Auras.Objects;
using AmeisenBotX.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Combat.Classes.Jannis
{
    public class WarlockDestruction : BasicCombatClass
    {
        public WarlockDestruction(WowInterface wowInterface, AmeisenBotFsm stateMachine) : base(wowInterface, stateMachine)
        {
            PetManager = new PetManager
            (
                WowInterface,
                TimeSpan.FromSeconds(1),
                null,
                () => WowInterface.CharacterManager.SpellBook.IsSpellKnown(summonImpSpell) && TryCastSpell(summonImpSpell, 0, true),
                () => WowInterface.CharacterManager.SpellBook.IsSpellKnown(summonImpSpell) && TryCastSpell(summonImpSpell, 0, true)
            );

            MyAuraManager.Jobs.Add(new KeepBestActiveAuraJob(new List<(string, Func<bool>)>()
            {
                (felArmorSpell, () => TryCastSpell(felArmorSpell, 0, true)),
                (demonArmorSpell, () => TryCastSpell(demonArmorSpell, 0, true)),
                (demonSkinSpell, () => TryCastSpell(demonSkinSpell, 0, true)),
            }));

            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(corruptionSpell, () => WowInterface.Target != null && !WowInterface.Target.HasBuffByName(seedOfCorruptionSpell) && TryCastSpell(corruptionSpell, WowInterface.TargetGuid, true)));
            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(curseOfTheElementsSpell, () => TryCastSpell(curseOfTheElementsSpell, WowInterface.TargetGuid, true)));
            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(immolateSpell, () => TryCastSpell(immolateSpell, WowInterface.TargetGuid, true)));
        }

        public override string Description => "FCFS based CombatClass for the Destruction Warlock spec.";

        public override string Displayname => "Warlock Destruction";

        public override bool HandlesMovement => false;

        public override bool IsMelee => false;

        public override IItemComparator ItemComparator { get; set; } = new BasicIntellectComparator(new() { WowArmorType.SHIELDS });

        public PetManager PetManager { get; private set; }

        public override WowRole Role => WowRole.Dps;

        public override TalentTree Talents { get; } = new()
        {
            Tree1 = new(),
            Tree2 = new()
            {
                { 2, new(2, 2, 3) },
                { 3, new(2, 3, 3) },
                { 4, new(2, 4, 1) },
                { 7, new(2, 7, 3) },
                { 9, new(2, 9, 1) },
                { 10, new(2, 10, 1) },
                { 11, new(2, 11, 3) },
                { 12, new(2, 12, 3) },
            },
            Tree3 = new()
            {
                { 2, new(3, 2, 5) },
                { 3, new(3, 3, 2) },
                { 5, new(3, 5, 3) },
                { 6, new(3, 6, 2) },
                { 8, new(3, 8, 5) },
                { 9, new(3, 9, 2) },
                { 10, new(3, 10, 1) },
                { 12, new(3, 12, 3) },
                { 13, new(3, 13, 3) },
                { 14, new(3, 14, 1) },
                { 16, new(3, 16, 5) },
                { 17, new(3, 17, 1) },
                { 19, new(3, 19, 3) },
                { 20, new(3, 20, 5) },
                { 22, new(3, 22, 3) },
                { 24, new(3, 24, 3) },
                { 25, new(3, 25, 5) },
                { 26, new(3, 26, 1) },
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

                if (WowInterface.Player.ManaPercentage < 20
                        && WowInterface.Player.HealthPercentage > 60
                        && TryCastSpell(lifeTapSpell, 0)
                    || (WowInterface.Player.HealthPercentage < 80
                        && TryCastSpell(deathCoilSpell, WowInterface.TargetGuid, true))
                    || (WowInterface.Player.HealthPercentage < 50
                        && TryCastSpell(drainLifeSpell, WowInterface.TargetGuid, true)))
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
                                && TryCastSpell(fearSpell, WowInterface.TargetGuid, true))))
                        {
                            LastFearAttempt = DateTime.UtcNow;
                            return;
                        }
                    }

                    if (WowInterface.CharacterManager.Inventory.Items.Count(e => e.Name.Equals("Soul Shard", StringComparison.OrdinalIgnoreCase)) < 5
                        && WowInterface.Target.HealthPercentage < 25.0
                        && TryCastSpell(drainSoulSpell, WowInterface.TargetGuid, true))
                    {
                        return;
                    }
                }

                if (WowInterface.ObjectManager.GetNearEnemies<WowUnit>(WowInterface.Target.Position, 16.0f).Count() > 2
                    && !WowInterface.Target.HasBuffByName(seedOfCorruptionSpell)
                    && TryCastSpell(seedOfCorruptionSpell, WowInterface.TargetGuid, true))
                {
                    return;
                }

                if (TryCastSpell(chaosBoltSpell, WowInterface.TargetGuid, true)
                    // || CastSpellIfPossible(conflagrateSpell, WowInterface.TargetGuid, true)
                    || TryCastSpell(incinerateSpell, WowInterface.TargetGuid, true))
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