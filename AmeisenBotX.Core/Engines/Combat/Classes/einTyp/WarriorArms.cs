using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Engines.Character.Comparators;
using AmeisenBotX.Core.Engines.Character.Talents.Objects;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Core.Fsm;
using AmeisenBotX.Core.Fsm.States;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Combat.Classes.einTyp
{
    public class WarriorArms : ICombatClass
    {
        private readonly AmeisenBotInterfaces Bot;
        private readonly string[] runningEmotes = { "/fart", "/burp", "/moo" };
        private readonly WarriorArmSpells spells;
        private readonly string[] standingEmotes = { "/chug", "/pick", "/whistle", "/violin" };
        private bool computeNewRoute = false;
        private double distanceToTarget = 0;
        private double distanceTraveled = 0;
        private bool multipleTargets = false;
        private bool standing = false;

        public WarriorArms(AmeisenBotInterfaces bot, AmeisenBotFsm stateMachine)
        {
            Bot = bot;
            StateMachine = stateMachine;
            spells = new WarriorArmSpells(bot);
        }

        public string Author => "einTyp";

        public IEnumerable<int> BlacklistedTargetDisplayIds { get; set; }

        public Dictionary<string, dynamic> C { get; set; } = new Dictionary<string, dynamic>();

        public string Description => "...";

        public string Displayname => "Arms Warrior";

        public bool HandlesFacing => false;

        public bool HandlesMovement => true;

        public bool IsMelee => true;

        public IItemComparator ItemComparator => new ArmsItemComparator(Bot);

        public IEnumerable<int> PriorityTargetDisplayIds { get; set; }

        public WowRole Role => WowRole.Dps;

        public TalentTree Talents { get; } = new()
        {
            Tree1 = new()
            {
                { 1, new(1, 1, 3) },
                { 3, new(1, 3, 2) },
                { 4, new(1, 4, 2) },
                { 6, new(1, 6, 3) },
                { 7, new(1, 7, 2) },
                { 8, new(1, 8, 1) },
                { 9, new(1, 9, 2) },
                { 10, new(1, 10, 3) },
                { 11, new(1, 11, 3) },
                { 12, new(1, 12, 3) },
                { 13, new(1, 13, 5) },
                { 14, new(1, 14, 1) },
                { 17, new(1, 17, 2) },
                { 21, new(1, 21, 1) },
                { 22, new(1, 22, 2) },
                { 24, new(1, 24, 1) },
                { 25, new(1, 25, 3) },
                { 26, new(1, 26, 2) },
                { 27, new(1, 27, 3) },
                { 28, new(1, 28, 1) },
                { 29, new(1, 29, 2) },
                { 30, new(1, 30, 5) },
                { 31, new(1, 31, 1) }
            },
            Tree2 = new()
            {
                { 1, new(2, 1, 3) },
                { 2, new(2, 2, 2) },
                { 3, new(2, 3, 5) },
                { 5, new(2, 5, 5) },
                { 11, new(2, 11, 1) }
            },
            Tree3 = new()
        };

        public string Version => "1.0";

        public bool WalkBehindEnemy => false;

        public WowClass WowClass => WowClass.Warrior;

        private bool Dancing { get; set; }

        private Vector3 LastPlayerPosition { get; set; }

        private Vector3 LastTargetPosition { get; set; }

        private AmeisenBotFsm StateMachine { get; }

        public void AttackTarget()
        {
            IWowUnit target = Bot.Target;
            if (target == null)
            {
                return;
            }

            if (Bot.Player.Position.GetDistance(target.Position) <= 3.0)
            {
                Bot.Wow.WowStopClickToMove();
                Bot.Movement.Reset();
                Bot.Wow.WowUnitRightClick(target.BaseAddress);
            }
            else
            {
                Bot.Movement.SetMovementAction(MovementAction.Move, target.Position);
            }
        }

        public void Execute()
        {
            computeNewRoute = false;
            IWowUnit target = Bot.Target;
            if ((Bot.Wow.TargetGuid != 0 && target != null && !(target.IsDead || target.Health < 1)) || SearchNewTarget(ref target, false))
            {
                bool targetDistanceChanged = false;
                if (!LastPlayerPosition.Equals(Bot.Player.Position))
                {
                    distanceTraveled = Bot.Player.Position.GetDistance(LastPlayerPosition);
                    LastPlayerPosition = new Vector3(Bot.Player.Position.X, Bot.Player.Position.Y, Bot.Player.Position.Z);
                    targetDistanceChanged = true;
                }

                if (!LastTargetPosition.Equals(target.Position))
                {
                    computeNewRoute = true;
                    LastTargetPosition = new Vector3(target.Position.X, target.Position.Y, target.Position.Z);
                    targetDistanceChanged = true;
                }

                if (targetDistanceChanged)
                {
                    distanceToTarget = LastPlayerPosition.GetDistance(LastTargetPosition);
                }

                HandleMovement(target);
                HandleAttacking(target);
            }
            StateMachine.GetState<StateCombat>().Mode = CombatMode.Allowed;
        }

        public void OutOfCombatExecute()
        {
            computeNewRoute = false;
            if (!LastPlayerPosition.Equals(Bot.Player.Position))
            {
                distanceTraveled = Bot.Player.Position.GetDistance(LastPlayerPosition);
                LastPlayerPosition = new Vector3(Bot.Player.Position.X, Bot.Player.Position.Y, Bot.Player.Position.Z);
            }

            if (distanceTraveled < 0.001)
            {
                ulong leaderGuid = Bot.Objects.Partyleader.Guid;
                IWowUnit target = Bot.Target;
                IWowUnit leader = null;
                if (leaderGuid != 0)
                {
                    leader = Bot.GetWowObjectByGuid<IWowUnit>(leaderGuid);
                }

                if (leaderGuid != 0 && leaderGuid != Bot.Wow.PlayerGuid && leader != null && !(leader.IsDead || leader.Health < 1))
                {
                    Bot.Movement.SetMovementAction(Movement.Enums.MovementAction.Move, Bot.GetWowObjectByGuid<IWowUnit>(leaderGuid).Position);
                }
                else if ((Bot.Wow.TargetGuid != 0 && target != null && !(target.IsDead || target.Health < 1)) || SearchNewTarget(ref target, true))
                {
                    if (!LastTargetPosition.Equals(target.Position))
                    {
                        computeNewRoute = true;
                        LastTargetPosition = new Vector3(target.Position.X, target.Position.Y, target.Position.Z);
                        distanceToTarget = LastPlayerPosition.GetDistance(LastTargetPosition);
                    }

                    Dancing = false;
                    HandleMovement(target);
                    StateMachine.GetState<StateCombat>().Mode = CombatMode.Force;
                    HandleAttacking(target);
                }
                else if (!Dancing || standing)
                {
                    standing = false;
                    Bot.Wow.WowClearTarget();
                    Bot.Wow.LuaSendChatMessage(standingEmotes[new Random().Next(standingEmotes.Length)]);
                    Dancing = true;
                }
            }
            else
            {
                if (!Dancing || !standing)
                {
                    standing = true;
                    Bot.Wow.WowClearTarget();
                    Bot.Wow.LuaSendChatMessage(runningEmotes[new Random().Next(runningEmotes.Length)]);
                    Dancing = true;
                }
            }
        }

        private void HandleAttacking(IWowUnit target)
        {
            Bot.Wow.WowTargetGuid(target.Guid);
            spells.CastNextSpell(distanceToTarget, target, multipleTargets);
            if (target.IsDead || target.Health < 1)
            {
                spells.ResetAfterTargetDeath();
            }
        }

        private void HandleMovement(IWowUnit target)
        {
            if (target == null)
            {
                return;
            }

            if (Bot.Movement.Status != Movement.Enums.MovementAction.None && distanceToTarget < 0.75f * (Bot.Player.CombatReach + target.CombatReach))
            {
                Bot.Movement.StopMovement();
            }

            if (computeNewRoute)
            {
                if (!BotMath.IsFacing(LastPlayerPosition, Bot.Player.Rotation, LastTargetPosition, 0.5f))
                {
                    Bot.Wow.WowFacePosition(Bot.Player.BaseAddress, Bot.Player.Position, target.Position);
                }

                Bot.Movement.SetMovementAction(Movement.Enums.MovementAction.Move, target.Position, target.Rotation);
            }
        }

        private bool SearchNewTarget(ref IWowUnit target, bool grinding)
        {
            if (Bot.Wow.TargetGuid != 0 && target != null && !(target.IsDead || target.Health < 1 || target.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId).Contains("Spirit of Redem"))))
            {
                return false;
            }

            List<IWowUnit> wowUnits = Bot.Objects.WowObjects.OfType<IWowUnit>().Where(e => Bot.Db.GetReaction(Bot.Player, e) != WowUnitReaction.Friendly && Bot.Db.GetReaction(Bot.Player, e) != WowUnitReaction.Neutral).ToList();
            bool newTargetFound = false;
            int targetHealth = (target == null || target.IsDead || target.Health < 1) ? 2147483647 : target.Health;
            bool inCombat = target != null && target.IsInCombat;
            int targetCount = 0;
            multipleTargets = false;
            foreach (IWowUnit unit in wowUnits)
            {
                if (IWowUnit.IsValidUnit(unit) && unit != target && !(unit.IsDead || unit.Health < 1 || unit.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId).Contains("Spirit of Redem"))))
                {
                    double tmpDistance = Bot.Player.Position.GetDistance(unit.Position);
                    if (tmpDistance < 100.0 || grinding)
                    {
                        if (tmpDistance < 6.0)
                        {
                            targetCount++;
                        }

                        if (((unit.IsInCombat && unit.Health < targetHealth) || (!inCombat && grinding && unit.Health < targetHealth)) && Bot.Wow.WowIsInLineOfSight(Bot.Player.Position, unit.Position))
                        {
                            target = unit;
                            targetHealth = unit.Health;
                            newTargetFound = true;
                            inCombat = unit.IsInCombat;
                        }
                    }
                }
            }

            if (target == null || target.IsDead || target.Health < 1 || target.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId).Contains("Spirit of Redem")))
            {
                Bot.Wow.WowClearTarget();
                newTargetFound = false;
                target = null;
            }
            else if (targetCount > 1)
            {
                multipleTargets = true;
            }

            if (newTargetFound)
            {
                Bot.Wow.WowTargetGuid(target.Guid);
                spells.ResetAfterTargetDeath();
            }

            return newTargetFound;
        }

        private class WarriorArmSpells
        {
            private static readonly string BattleShout = "Battle Shout";
            private static readonly string BattleStance = "Battle Stance";
            private static readonly string BerserkerRage = "Berserker Rage";
            private static readonly string BerserkerStance = "Berserker Stance";
            private static readonly string Bladestorm = "Bladestorm";
            private static readonly string Bloodrage = "Bloodrage";
            private static readonly string Bloodthirst = "Bloodthirst";
            private static readonly string Charge = "Charge";
            private static readonly string DeathWish = "Death Wish";
            private static readonly string EnragedRegeneration = "Enraged Regeneration";
            private static readonly string Execute = "Execute";
            private static readonly string Hamstring = "Hamstring";
            private static readonly string HeroicStrike = "Heroic Strike";
            private static readonly string HeroicThrow = "Heroic Throw";
            private static readonly string Intercept = "Intercept";
            private static readonly string IntimidatingShout = "Intimidating Shout";
            private static readonly string MortalStrike = "Mortal Strike";
            private static readonly string Recklessness = "Recklessness";
            private static readonly string Rend = "Rend";
            private static readonly string Retaliation = "Retaliation";
            private static readonly string ShatteringThrow = "Shattering Throw";
            private static readonly string Slam = "Slam";
            private static readonly string Whirlwind = "Whirlwind";

            private readonly AmeisenBotInterfaces Bot;

            private readonly Dictionary<string, DateTime> nextActionTime = new()
            {
                { BattleShout, DateTime.Now },
                { BattleStance, DateTime.Now },
                { BerserkerStance, DateTime.Now },
                { BerserkerRage, DateTime.Now },
                { Slam, DateTime.Now },
                { Recklessness, DateTime.Now },
                { DeathWish, DateTime.Now },
                { Intercept, DateTime.Now },
                { ShatteringThrow, DateTime.Now },
                { HeroicThrow, DateTime.Now },
                { Charge, DateTime.Now },
                { Bloodthirst, DateTime.Now },
                { Hamstring, DateTime.Now },
                { Execute, DateTime.Now },
                { Whirlwind, DateTime.Now },
                { Retaliation, DateTime.Now },
                { EnragedRegeneration, DateTime.Now },
                { IntimidatingShout, DateTime.Now },
                { Bloodrage, DateTime.Now },
                { Bladestorm, DateTime.Now },
                { Rend, DateTime.Now },
                { MortalStrike, DateTime.Now },
                { HeroicStrike, DateTime.Now }
            };

            private bool askedForHeal = false;
            private bool askedForHelp = false;

            public WarriorArmSpells(AmeisenBotInterfaces bot)
            {
                Bot = bot;
                Player = Bot.Player;
                IsInBerserkerStance = false;
                NextGCDSpell = DateTime.Now;
                NextStance = DateTime.Now;
                NextCast = DateTime.Now;
            }

            private bool IsInBerserkerStance { get; set; }

            private DateTime NextCast { get; set; }

            private DateTime NextGCDSpell { get; set; }

            private DateTime NextStance { get; set; }

            private IWowPlayer Player { get; set; }

            public void CastNextSpell(double distanceToTarget, IWowUnit target, bool multipleTargets)
            {
                if (!IsReady(NextCast))
                {
                    return;
                }

                if (!Bot.Player.IsAutoAttacking)
                {
                    Bot.Wow.LuaStartAutoAttack();
                }

                Player = Bot.Player;
                int rage = Player.Rage;
                bool isGCDReady = IsReady(NextGCDSpell);
                bool lowHealth = Player.HealthPercentage <= 25;
                bool mediumHealth = !lowHealth && Player.HealthPercentage <= 75;
                if (!(lowHealth || mediumHealth))
                {
                    askedForHelp = false;
                    askedForHeal = false;
                }
                else if (lowHealth && !askedForHelp)
                {
                    Bot.Wow.LuaSendChatMessage("/helpme");
                    askedForHelp = true;
                }
                else if (mediumHealth && !askedForHeal)
                {
                    Bot.Wow.LuaSendChatMessage("/healme");
                    askedForHeal = true;
                }

                // -- buffs --
                if (lowHealth && rage > 15 && IsReady(EnragedRegeneration))
                {
                    if (IsReady(EnragedRegeneration))
                    {
                        CastSpell(EnragedRegeneration, ref rage, 15, 180, false);
                    }
                }
                else if (rage < 20 && !lowHealth && !mediumHealth && IsReady(Bloodrage))
                {
                    CastSpell(Bloodrage, ref rage, 0, 40.2, false);
                }

                if (isGCDReady)
                {
                    // Berserker Rage
                    if (Player.Health < Player.MaxHealth && IsReady(BerserkerRage))
                    {
                        CastSpell(BerserkerRage, ref rage, 0, 20.1, true); // lasts 10 sec
                    }
                }

                if (multipleTargets)
                {
                    if (rage > 25 && IsReady(IntimidatingShout))
                    {
                        CastSpell(IntimidatingShout, ref rage, 25, 120, false);
                    }
                    else if (IsReady(Retaliation))
                    {
                        CastSpell(Retaliation, ref rage, 0, 300, false);
                    }
                }

                if (distanceToTarget < (29 + target.CombatReach))
                {
                    if (distanceToTarget < (24 + target.CombatReach))
                    {
                        if (distanceToTarget > (9 + target.CombatReach))
                        {
                            // -- run to the target! --
                            if (Player.IsInCombat)
                            {
                                if (isGCDReady)
                                {
                                    if (IsInBerserkerStance)
                                    {
                                        // intercept
                                        if (rage > 10 && IsReady(Intercept))
                                        {
                                            CastSpell(Intercept, ref rage, 10, 30, true);
                                        }
                                    }
                                    else
                                    {
                                        // Berserker Stance
                                        if (IsReady(NextStance))
                                        {
                                            ChangeToStance(BerserkerStance, out rage);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (IsInBerserkerStance)
                                {
                                    // Battle Stance
                                    if (IsReady(NextStance))
                                    {
                                        ChangeToStance(BattleStance, out rage);
                                    }
                                }
                                else
                                {
                                    // charge
                                    if (IsReady(Charge))
                                    {
                                        CastSpell(Charge, ref rage, 0, 15, false);
                                    }
                                }
                            }
                        }
                        else if (distanceToTarget <= 0.75f * (Player.CombatReach + target.CombatReach))
                        {
                            // -- close combat --
                            // Battle Stance
                            if (IsInBerserkerStance && IsReady(NextStance))
                            {
                                ChangeToStance(BattleStance, out rage);
                            }
                            else if (isGCDReady)
                            {
                                List<string> buffs = Bot.Player.Auras.Select(e => Bot.Db.GetSpellName(e.SpellId)).ToList();
                                if (buffs.Any(e => e.Contains("slam") || e.Contains("Slam")) && rage > 15)
                                {
                                    CastSpell(Slam, ref rage, 15, 0, false);
                                    NextCast = DateTime.Now.AddSeconds(1.5);
                                    NextGCDSpell = DateTime.Now.AddSeconds(3.0);
                                }
                                else if (rage > 10 && IsReady(Rend))
                                {
                                    CastSpell(Rend, ref rage, 10, 15, true);
                                }
                                else if (rage > 10 && IsReady(Hamstring))
                                {
                                    CastSpell(Hamstring, ref rage, 10, 15, true);
                                }
                                else if (target.HealthPercentage <= 20 && rage > 10)
                                {
                                    CastSpell(Execute, ref rage, 10, 0, true);
                                }
                                else if (multipleTargets && rage > 25 && IsReady(Bladestorm))
                                {
                                    CastSpell(Bladestorm, ref rage, 25, 90, true);
                                }
                                else if (rage > 30 && IsReady(MortalStrike))
                                {
                                    CastSpell(MortalStrike, ref rage, 30, 6, true);
                                }
                                else if (rage > 12 && IsReady(HeroicStrike))
                                {
                                    CastSpell(HeroicStrike, ref rage, 12, 3.6, false);
                                }
                            }
                            else if (target.HealthPercentage > 20 && rage > 12 && IsReady(HeroicStrike))
                            {
                                CastSpell(HeroicStrike, ref rage, 12, 3.6, false);
                            }
                            else
                            {
                                if (!Bot.Player.IsAutoAttacking)
                                {
                                    Bot.Wow.LuaStartAutoAttack();
                                }
                            }
                        }
                    }
                    else
                    {
                        if (isGCDReady)
                        {
                            // -- distant attacks --
                            if (isGCDReady)
                            {
                                // shattering throw (in Battle Stance)
                                if (rage > 25 && IsReady(ShatteringThrow))
                                {
                                    if (IsInBerserkerStance)
                                    {
                                        if (IsReady(NextStance))
                                        {
                                            ChangeToStance(BattleStance, out rage);
                                        }
                                    }
                                    else
                                    {
                                        CastSpell(ShatteringThrow, ref rage, 25, 301.5, false);
                                        NextCast = DateTime.Now.AddSeconds(1.5); // casting time
                                        NextGCDSpell = DateTime.Now.AddSeconds(3.0); // 1.5 sec gcd after the 1.5 sec casting time
                                    }
                                }
                                else
                                {
                                    CastSpell(HeroicThrow, ref rage, 0, 60, true);
                                }
                            }
                        }
                    }
                }
            }

            public void ResetAfterTargetDeath()
            {
                nextActionTime[Hamstring].AddSeconds(-15.0);
                nextActionTime[Rend].AddSeconds(-15.0);
                nextActionTime[HeroicStrike].AddSeconds(-3.6);
            }

            private static bool IsReady(DateTime nextAction)
            {
                return DateTime.Now > nextAction;
            }

            private void CastSpell(string spell, ref int rage, int rageCosts, double cooldown, bool gcd)
            {
                Bot.Wow.LuaCastSpell(spell);
                rage -= rageCosts;
                if (cooldown > 0)
                {
                    nextActionTime[spell] = DateTime.Now.AddSeconds(cooldown);
                }

                if (gcd)
                {
                    NextGCDSpell = DateTime.Now.AddSeconds(1.5);
                }
            }

            private void ChangeToStance(string stance, out int rage)
            {
                Bot.Wow.LuaCastSpell(stance);
                rage = UpdateRage();
                NextStance = DateTime.Now.AddSeconds(1);
                IsInBerserkerStance = stance == BerserkerStance;
            }

            private bool IsReady(string spell)
            {
                return !nextActionTime.TryGetValue(spell, out DateTime NextSpellAvailable) || IsReady(NextSpellAvailable);
            }

            private int UpdateRage()
            {
                Player = Bot.Player;
                return Player.Rage;
            }
        }
    }
}