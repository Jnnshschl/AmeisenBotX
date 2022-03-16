using AmeisenBotX.Core.Engines.Combat.Helpers;
using AmeisenBotX.Core.Engines.Combat.Helpers.Aura.Objects;
using AmeisenBotX.Core.Engines.Movement.Enums;
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
    public class HunterMarksmanship : BasicCombatClass
    {
        public HunterMarksmanship(AmeisenBotInterfaces bot) : base(bot)
        {
            PetManager = new PetManager
            (
                Bot,
                TimeSpan.FromSeconds(15),
                () => TryCastSpell(Hunter335a.MendPet, 0, true),
                () => TryCastSpell(Hunter335a.CallPet, 0),
                () => TryCastSpell(Hunter335a.RevivePet, 0)
            );

            MyAuraManager.Jobs.Add(new KeepBestActiveAuraJob(bot.Db, new List<(string, Func<bool>)>()
            {
                (Hunter335a.AspectOfTheViper, () => Bot.Player.ManaPercentage < 25.0 && TryCastSpell(Hunter335a.AspectOfTheViper, 0, true)),
                (Hunter335a.AspectOfTheDragonhawk, () => (!bot.Character.SpellBook.IsSpellKnown(Hunter335a.AspectOfTheViper) || Bot.Player.ManaPercentage > 80.0) && TryCastSpell(Hunter335a.AspectOfTheDragonhawk, 0, true)),
                (Hunter335a.AspectOfTheHawk, () => TryCastSpell(Hunter335a.AspectOfTheHawk, 0, true))
            }));

            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Hunter335a.HuntersMark, () => TryCastSpell(Hunter335a.HuntersMark, Bot.Wow.TargetGuid, true)));
            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Hunter335a.SerpentSting, () => TryCastSpell(Hunter335a.SerpentSting, Bot.Wow.TargetGuid, true)));

            InterruptManager.InterruptSpells = new()
            {
                { 0, (x) => TryCastSpell(Hunter335a.SilencingShot, x.Guid, true) }
            };
        }

        public override string Description => "FCFS based CombatClass for the Marksmanship Hunter spec.";

        public override string DisplayName2 => "Hunter Marksmanship";

        public override bool HandlesMovement => false;

        public override bool IsMelee => false;

        public override IItemComparator ItemComparator { get; set; } = new BasicIntellectComparator(new() { WowArmorType.Shield });

        public override WowRole Role => WowRole.Dps;

        public override TalentTree Talents { get; } = new()
        {
            Tree1 = new()
            {
                { 1, new(1, 1, 5) },
                { 3, new(1, 3, 2) },
            },
            Tree2 = new()
            {
                { 2, new(2, 2, 3) },
                { 3, new(2, 3, 5) },
                { 4, new(2, 4, 3) },
                { 6, new(2, 6, 5) },
                { 7, new(2, 7, 1) },
                { 8, new(2, 8, 3) },
                { 9, new(2, 9, 1) },
                { 11, new(2, 11, 3) },
                { 14, new(2, 14, 1) },
                { 15, new(2, 15, 3) },
                { 16, new(2, 16, 2) },
                { 17, new(2, 17, 3) },
                { 18, new(2, 18, 3) },
                { 19, new(2, 19, 1) },
                { 20, new(2, 20, 3) },
                { 21, new(2, 21, 5) },
                { 23, new(2, 23, 3) },
                { 25, new(2, 25, 3) },
                { 26, new(2, 26, 5) },
                { 27, new(2, 27, 1) },
            },
            Tree3 = new()
            {
                { 1, new(3, 1, 5) },
                { 7, new(3, 7, 2) },
            },
        };

        public override bool UseAutoAttacks => true;

        public override string Version => "1.0";

        public override bool WalkBehindEnemy => false;

        public override WowClass WowClass => WowClass.Hunter;

        public override WowVersion WowVersion => WowVersion.WotLK335a;

        private PetManager PetManager { get; set; }

        private bool ReadyToDisengage { get; set; } = false;

        private bool SlowTargetWhenPossible { get; set; } = false;

        public override void Execute()
        {
            base.Execute();

            if (TryFindTarget(TargetProviderDps, out _))
            {
                if (PetManager.Tick()) { return; }

                IWowUnit target = (IWowUnit)Bot.Objects.WowObjects.FirstOrDefault(e => e != null && e.Guid == Bot.Wow.TargetGuid);

                if (target != null)
                {
                    double distanceToTarget = target.Position.GetDistance(Bot.Player.Position);

                    // make some distance
                    if ((Bot.Target.Type == WowObjectType.Player && Bot.Wow.TargetGuid != 0 && distanceToTarget < 10.0)
                        || (Bot.Target.Type == WowObjectType.Unit && Bot.Wow.TargetGuid != 0 && distanceToTarget < 3.0))
                    {
                        Bot.Movement.SetMovementAction(MovementAction.Flee, Bot.Target.Position, Bot.Target.Rotation);
                    }

                    if (Bot.Player.HealthPercentage < 15
                        && TryCastSpell(Hunter335a.FeignDeath, 0))
                    {
                        return;
                    }

                    if (distanceToTarget < 5.0)
                    {
                        if (ReadyToDisengage
                            && TryCastSpell(Hunter335a.Disengage, 0, true))
                        {
                            ReadyToDisengage = false;
                            return;
                        }

                        if (TryCastSpell(Hunter335a.FrostTrap, 0, true))
                        {
                            ReadyToDisengage = true;
                            SlowTargetWhenPossible = true;
                            return;
                        }

                        if (Bot.Player.HealthPercentage < 30
                            && TryCastSpell(Hunter335a.Deterrence, 0, true))
                        {
                            return;
                        }

                        if (TryCastSpell(Hunter335a.RaptorStrike, Bot.Wow.TargetGuid, true)
                            || TryCastSpell(Hunter335a.MongooseBite, Bot.Wow.TargetGuid, true))
                        {
                            return;
                        }
                    }
                    else
                    {
                        if (SlowTargetWhenPossible
                            && TryCastSpell(Hunter335a.Disengage, 0, true))
                        {
                            SlowTargetWhenPossible = false;
                            return;
                        }

                        if (target.HealthPercentage < 20
                            && TryCastSpell(Hunter335a.KillShot, Bot.Wow.TargetGuid, true))
                        {
                            return;
                        }

                        TryCastSpell(Hunter335a.KillCommand, Bot.Wow.TargetGuid, true);
                        TryCastSpell(Hunter335a.RapidFire, Bot.Wow.TargetGuid);

                        if (Bot.GetNearEnemies<IWowUnit>(Bot.Target.Position, 16.0f).Count() > 2
                            && TryCastSpell(Hunter335a.MultiShot, Bot.Wow.TargetGuid, true))
                        {
                            return;
                        }

                        if ((Bot.Objects.WowObjects.OfType<IWowUnit>().Where(e => target.Position.GetDistance(e.Position) < 16).Count() > 2 && TryCastSpell(Hunter335a.MultiShot, Bot.Wow.TargetGuid, true))
                            || TryCastSpell(Hunter335a.ChimeraShot, Bot.Wow.TargetGuid, true)
                            || TryCastSpell(Hunter335a.AimedShot, Bot.Wow.TargetGuid, true)
                            || TryCastSpell(Hunter335a.ArcaneShot, Bot.Wow.TargetGuid, true)
                            || TryCastSpell(Hunter335a.SteadyShot, Bot.Wow.TargetGuid, true))
                        {
                            return;
                        }
                    }
                }
            }
        }

        public override void OutOfCombatExecute()
        {
            ReadyToDisengage = false;
            SlowTargetWhenPossible = false;

            base.OutOfCombatExecute();

            if (PetManager.Tick())
            {
                return;
            }
        }
    }
}