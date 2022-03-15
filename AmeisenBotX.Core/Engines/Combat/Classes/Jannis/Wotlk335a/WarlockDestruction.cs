using AmeisenBotX.Core.Engines.Combat.Helpers;
using AmeisenBotX.Core.Engines.Combat.Helpers.Aura.Objects;
using AmeisenBotX.Core.Managers.Character.Comparators;
using AmeisenBotX.Core.Managers.Character.Talents.Objects;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Wow335a.Constants;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Combat.Classes.Jannis.Wotlk335a
{
    public class WarlockDestruction : BasicCombatClass
    {
        public WarlockDestruction(AmeisenBotInterfaces bot) : base(bot)
        {
            PetManager = new PetManager
            (
                Bot,
                TimeSpan.FromSeconds(1),
                null,
                () => Bot.Character.SpellBook.IsSpellKnown(Warlock335a.SummonImp) && TryCastSpell(Warlock335a.SummonImp, 0, true),
                () => Bot.Character.SpellBook.IsSpellKnown(Warlock335a.SummonImp) && TryCastSpell(Warlock335a.SummonImp, 0, true)
            );

            MyAuraManager.Jobs.Add(new KeepBestActiveAuraJob(bot.Db, new List<(string, Func<bool>)>()
            {
                (Warlock335a.FelArmor, () => TryCastSpell(Warlock335a.FelArmor, 0, true)),
                (Warlock335a.DemonArmor, () => TryCastSpell(Warlock335a.DemonArmor, 0, true)),
                (Warlock335a.DemonSkin, () => TryCastSpell(Warlock335a.DemonSkin, 0, true)),
            }));

            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Warlock335a.Corruption, () => Bot.Target != null && !Bot.Target.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == Warlock335a.SeedOfCorruption) && TryCastSpell(Warlock335a.Corruption, Bot.Wow.TargetGuid, true)));
            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Warlock335a.CurseOfTheElements, () => TryCastSpell(Warlock335a.CurseOfTheElements, Bot.Wow.TargetGuid, true)));
            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Warlock335a.Immolate, () => TryCastSpell(Warlock335a.Immolate, Bot.Wow.TargetGuid, true)));
        }

        public override string Description => "FCFS based CombatClass for the Destruction Warlock spec.";

        public override string DisplayName2 => "Warlock Destruction";

        public override bool HandlesMovement => false;

        public override bool IsMelee => false;

        public override IItemComparator ItemComparator { get; set; } = new BasicIntellectComparator(new() { WowArmorType.Shield });

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

        public override WowVersion WowVersion => WowVersion.WotLK335a;

        private DateTime LastFearAttempt { get; set; }

        public override void Execute()
        {
            base.Execute();

            if (FindTarget(TargetProviderDps))
            {
                if (PetManager.Tick()) { return; }

                if ((Bot.Player.ManaPercentage < 75.0 && Bot.Player.HealthPercentage > 60.0 && TryCastSpell(Warlock335a.LifeTap, 0))
                    || (Bot.Player.HealthPercentage < 80.0 && TryCastSpell(Warlock335a.DeathCoil, Bot.Wow.TargetGuid, true))
                    || (Bot.Player.HealthPercentage < 50.0 && TryCastSpell(Warlock335a.DrainLife, Bot.Wow.TargetGuid, true)))
                {
                    return;
                }

                if (Bot.Target != null)
                {
                    if (Bot.Target.GetType() == typeof(IWowPlayer))
                    {
                        if (DateTime.UtcNow - LastFearAttempt > TimeSpan.FromSeconds(5)
                            && ((Bot.Player.Position.GetDistance(Bot.Target.Position) < 6.0f && TryCastSpell(Warlock335a.HowlOfTerror, 0, true))
                            || (Bot.Player.Position.GetDistance(Bot.Target.Position) < 12.0f && TryCastSpell(Warlock335a.Fear, Bot.Wow.TargetGuid, true))))
                        {
                            LastFearAttempt = DateTime.UtcNow;
                            return;
                        }
                    }

                    if (Bot.Character.Inventory.Items.Count(e => e.Name.Equals("Soul Shard", StringComparison.OrdinalIgnoreCase)) < 5
                        && Bot.Target.HealthPercentage < 25.0
                        && TryCastSpell(Warlock335a.DrainSoul, Bot.Wow.TargetGuid, true))
                    {
                        return;
                    }
                }

                if (Bot.GetNearEnemies<IWowUnit>(Bot.Target.Position, 16.0f).Count() > 2
                    && ((!Bot.Target.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == Warlock335a.SeedOfCorruption)
                        && TryCastSpell(Warlock335a.SeedOfCorruption, Bot.Wow.TargetGuid, true))
                    || TryCastAoeSpell(Warlock335a.RainOfFire, Bot.Wow.TargetGuid, true)))
                {
                    return;
                }

                if (TryCastSpell(Warlock335a.ChaosBolt, Bot.Wow.TargetGuid, true)
                    || TryCastSpell(Warlock335a.Incinerate, Bot.Wow.TargetGuid, true))
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