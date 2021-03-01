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
    public class WarlockDemonology : BasicCombatClass
    {
        public WarlockDemonology(WowInterface wowInterface, AmeisenBotFsm stateMachine) : base(wowInterface, stateMachine)
        {
            PetManager = new PetManager
            (
                WowInterface,
                TimeSpan.FromSeconds(1),
                null,
                () => (WowInterface.CharacterManager.SpellBook.IsSpellKnown(summonFelguardSpell)
                       && WowInterface.CharacterManager.Inventory.HasItemByName("Soul Shard")
                       && TryCastSpell(summonFelguardSpell, 0, true))
                   || (WowInterface.CharacterManager.SpellBook.IsSpellKnown(summonImpSpell)
                       && TryCastSpell(summonImpSpell, 0, true)),
                () => (WowInterface.CharacterManager.SpellBook.IsSpellKnown(summonFelguardSpell)
                       && WowInterface.CharacterManager.Inventory.HasItemByName("Soul Shard")
                       && TryCastSpell(summonFelguardSpell, 0, true))
                   || (WowInterface.CharacterManager.SpellBook.IsSpellKnown(summonImpSpell)
                       && TryCastSpell(summonImpSpell, 0, true))
            );

            MyAuraManager.Jobs.Add(new KeepBestActiveAuraJob(new List<(string, Func<bool>)>()
            {
                (felArmorSpell, () => TryCastSpell(felArmorSpell, 0, true)),
                (demonArmorSpell, () => TryCastSpell(demonArmorSpell, 0, true)),
                (demonSkinSpell, () => TryCastSpell(demonSkinSpell, 0, true)),
            }));

            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(corruptionSpell, () => WowInterface.ObjectManager.Target != null && !WowInterface.ObjectManager.Target.HasBuffByName(seedOfCorruptionSpell) && TryCastSpell(corruptionSpell, WowInterface.ObjectManager.TargetGuid, true)));
            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(curseOfTonguesSpell, () => TryCastSpell(curseOfTonguesSpell, WowInterface.ObjectManager.TargetGuid, true)));
            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(immolateSpell, () => TryCastSpell(immolateSpell, WowInterface.ObjectManager.TargetGuid, true)));
        }

        public override string Description => "FCFS based CombatClass for the Demonology Warlock spec.";

        public override string Displayname => "Warlock Demonology";

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
                { 3, new(2, 3, 3) },
                { 4, new(2, 4, 2) },
                { 6, new(2, 6, 3) },
                { 7, new(2, 7, 3) },
                { 9, new(2, 9, 1) },
                { 10, new(2, 10, 1) },
                { 11, new(2, 11, 3) },
                { 12, new(2, 12, 5) },
                { 13, new(2, 13, 2) },
                { 15, new(2, 15, 2) },
                { 16, new(2, 16, 5) },
                { 17, new(2, 17, 3) },
                { 19, new(2, 19, 1) },
                { 20, new(2, 20, 3) },
                { 21, new(2, 21, 5) },
                { 22, new(2, 22, 2) },
                { 23, new(2, 23, 3) },
                { 24, new(2, 24, 1) },
                { 25, new(2, 25, 3) },
                { 26, new(2, 26, 5) },
                { 27, new(2, 27, 1) },
            },
            Tree3 = new()
            {
                { 1, new(3, 1, 5) },
                { 2, new(3, 2, 5) },
                { 8, new(3, 8, 4) },
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

                if (WowInterface.ObjectManager.Player.ManaPercentage < 50.0
                        && WowInterface.ObjectManager.Player.HealthPercentage > 60.0
                        && TryCastSpell(lifeTapSpell, 0)
                    || (WowInterface.ObjectManager.Player.HealthPercentage < 80.0
                        && TryCastSpell(deathCoilSpell, WowInterface.ObjectManager.TargetGuid, true))
                    || (WowInterface.ObjectManager.Player.HealthPercentage < 50.0
                        && TryCastSpell(drainLifeSpell, WowInterface.ObjectManager.TargetGuid, true))
                    || TryCastSpell(metamorphosisSpell, 0)
                    || (WowInterface.ObjectManager.Pet?.Health > 0 && TryCastSpell(demonicEmpowermentSpell, 0)))
                {
                    return;
                }

                if (WowInterface.ObjectManager.Target != null)
                {
                    if (WowInterface.ObjectManager.Target.GetType() == typeof(WowPlayer))
                    {
                        if (DateTime.Now - LastFearAttempt > TimeSpan.FromSeconds(5)
                            && ((WowInterface.ObjectManager.Player.Position.GetDistance(WowInterface.ObjectManager.Target.Position) < 6.0
                                && TryCastSpell(howlOfTerrorSpell, 0, true))
                            || (WowInterface.ObjectManager.Player.Position.GetDistance(WowInterface.ObjectManager.Target.Position) < 12.0
                                && TryCastSpell(fearSpell, WowInterface.ObjectManager.TargetGuid, true))))
                        {
                            LastFearAttempt = DateTime.Now;
                            return;
                        }
                    }

                    if (WowInterface.CharacterManager.Inventory.Items.Count(e => e.Name.Equals("Soul Shard", StringComparison.OrdinalIgnoreCase)) < 5.0
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

                bool hasDecimation = WowInterface.ObjectManager.Player.HasBuffByName(decimationSpell);
                bool hasMoltenCore = WowInterface.ObjectManager.Player.HasBuffByName(moltenCoreSpell);

                if (hasDecimation && hasMoltenCore && TryCastSpell(soulfireSpell, WowInterface.ObjectManager.TargetGuid, true))
                {
                    return;
                }
                else if (hasDecimation && TryCastSpell(soulfireSpell, WowInterface.ObjectManager.TargetGuid, true))
                {
                    return;
                }
                else if (hasMoltenCore && TryCastSpell(incinerateSpell, WowInterface.ObjectManager.TargetGuid, true))
                {
                    return;
                }
                else if (TryCastSpell(shadowBoltSpell, WowInterface.ObjectManager.TargetGuid, true))
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