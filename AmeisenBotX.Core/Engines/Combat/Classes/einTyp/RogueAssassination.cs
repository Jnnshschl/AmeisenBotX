using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Engines.Character.Comparators;
using AmeisenBotX.Core.Engines.Character.Talents.Objects;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Combat.Classes.einTyp
{
    public class RogueAssassination : ICombatClass
    {
        private readonly AmeisenBotInterfaces Bot;
        private readonly bool hasTargetMoved = false;
        private readonly RogueAssassinSpells spells;
        private readonly string[] standingEmotes = { "/bored" };
        private bool computeNewRoute = false;

        private double distanceToBehindTarget = 0;

        private double distanceToTarget = 0;

        private double distanceTraveled = 0;
        private bool isAttackingFromBehind = false;

        private bool isSneaky = false;

        private bool standing = false;

        private bool wasInStealth = false;

        public RogueAssassination(AmeisenBotInterfaces bot)
        {
            Bot = bot;
            spells = new RogueAssassinSpells(bot);
        }

        public string Author => "einTyp";

        public IEnumerable<int> BlacklistedTargetDisplayIds { get; set; }

        public Dictionary<string, dynamic> C { get; set; } = new Dictionary<string, dynamic>();

        public string Description => "...";

        public string Displayname => "Assasination Rogue";

        public bool HandlesFacing => false;

        public bool HandlesMovement => true;

        public bool IsMelee => true;

        public IItemComparator ItemComparator => new AssassinationItemComparator();

        public IEnumerable<int> PriorityTargetDisplayIds { get; set; }

        public WowRole Role => WowRole.Dps;

        public TalentTree Talents { get; } = new()
        {
            Tree1 = new()
            {
                { 3, new(1, 3, 5) },
                { 4, new(1, 4, 3) },
                { 5, new(1, 5, 2) },
                { 6, new(1, 6, 3) },
                { 9, new(1, 9, 5) },
                { 10, new(1, 10, 3) },
                { 11, new(1, 11, 5) },
                { 13, new(1, 13, 1) },
                { 16, new(1, 16, 5) },
                { 17, new(1, 17, 2) },
                { 19, new(1, 19, 1) },
                { 21, new(1, 21, 3) },
                { 22, new(1, 22, 3) },
                { 23, new(1, 23, 3) },
                { 24, new(1, 24, 1) },
                { 26, new(1, 26, 5) },
                { 27, new(1, 27, 1) }
            },
            Tree2 = new()
            {
                { 3, new(2, 3, 5) },
                { 6, new(2, 6, 5) },
                { 9, new(2, 9, 5) },
                { 12, new(2, 12, 3) }
            },
            Tree3 = new()
            {
                { 3, new(3, 3, 2) }
            }
        };

        public string Version => "1.0";

        public bool WalkBehindEnemy => false;

        public WowClass WowClass => WowClass.Rogue;

        private bool Dancing { get; set; }

        private Vector3 LastBehindTargetPosition { get; set; }

        private Vector3 LastPlayerPosition { get; set; }

        private Vector3 LastTargetPosition { get; set; }

        private float LastTargetRotation { get; set; }

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
                Dancing = false;
                bool targetDistanceChanged = false;
                if (!LastPlayerPosition.Equals(Bot.Player.Position))
                {
                    distanceTraveled = Bot.Player.Position.GetDistance(LastPlayerPosition);
                    LastPlayerPosition = new Vector3(Bot.Player.Position.X, Bot.Player.Position.Y, Bot.Player.Position.Z);
                    targetDistanceChanged = true;
                }

                if (LastTargetRotation != target.Rotation)
                {
                    computeNewRoute = true;
                    LastTargetRotation = target.Rotation;
                }

                if (!LastTargetPosition.Equals(target.Position))
                {
                    computeNewRoute = true;
                    LastTargetPosition = new Vector3(target.Position.X, target.Position.Y, target.Position.Z);
                    LastBehindTargetPosition = BotMath.CalculatePositionBehind(target.Position, target.Rotation, 3f);
                    targetDistanceChanged = true;
                }

                if (targetDistanceChanged)
                {
                    distanceToTarget = LastPlayerPosition.GetDistance(LastTargetPosition);
                    distanceToBehindTarget = LastPlayerPosition.GetDistance(LastBehindTargetPosition);
                }

                HandleMovement(target);
                HandleAttacking(target);
            }
            else if (!Dancing)
            {
                if (distanceTraveled < 0.001)
                {
                    Bot.Wow.WowClearTarget();
                    Bot.Wow.LuaSendChatMessage(standingEmotes[new Random().Next(standingEmotes.Length)]);
                    Dancing = true;
                    Bot.Globals.ForceCombat = false;
                }
                else
                {
                    Bot.Wow.WowClearTarget();
                    Dancing = true;
                }
            }
        }

        public void OutOfCombatExecute()
        {
            computeNewRoute = false;
            List<string> buffs = Bot.Player.Auras.Select(e => Bot.Db.GetSpellName(e.SpellId)).ToList();
            if (!buffs.Any(e => e.Contains("tealth")))
            {
                Bot.Wow.LuaCastSpell("Stealth");
                spells.ResetAfterTargetDeath();
            }

            if (!LastPlayerPosition.Equals(Bot.Player.Position))
            {
                distanceTraveled = Bot.Player.Position.GetDistance(LastPlayerPosition);
                LastPlayerPosition = new Vector3(Bot.Player.Position.X, Bot.Player.Position.Y, Bot.Player.Position.Z);
            }

            if (distanceTraveled < 0.001)
            {
                ulong leaderGuid = Bot.Objects.Partyleader.Guid;
                IWowUnit target = Bot.Target;
                if ((Bot.Wow.TargetGuid != 0 && target != null && !(target.IsDead || target.Health < 1)) || SearchNewTarget(ref target, true))
                {
                    if (!LastTargetPosition.Equals(target.Position))
                    {
                        computeNewRoute = true;
                        LastTargetPosition = new Vector3(target.Position.X, target.Position.Y, target.Position.Z);
                        LastBehindTargetPosition = BotMath.CalculatePositionBehind(target.Position, target.Rotation, 5f);
                        distanceToTarget = LastPlayerPosition.GetDistance(LastTargetPosition);
                        distanceToBehindTarget = LastPlayerPosition.GetDistance(LastBehindTargetPosition);
                    }

                    Dancing = false;
                    HandleMovement(target);
                    Bot.Globals.ForceCombat = true;
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
                    Dancing = true;
                }
            }
        }

        private void HandleAttacking(IWowUnit target)
        {
            Bot.Wow.WowTargetGuid(target.Guid);
            spells.CastNextSpell(distanceToTarget, target);
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

            if (Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId).Contains("tealth")))
            {
                if (!wasInStealth || hasTargetMoved)
                {
                    isSneaky = true;
                }

                wasInStealth = true;
            }
            else
            {
                isSneaky = false;
                wasInStealth = false;
            }

            if (isAttackingFromBehind)
            {
                if (Bot.Movement.Status != Movement.Enums.MovementAction.None && distanceToTarget < 0.75f * (Bot.Player.CombatReach + target.CombatReach))
                {
                    Bot.Movement.StopMovement();
                }

                if (Bot.Player.IsInCombat)
                {
                    isAttackingFromBehind = false;
                }
            }

            if (computeNewRoute)
            {
                if (!isAttackingFromBehind && isSneaky && distanceToBehindTarget > 0.75f * (Bot.Player.CombatReach + target.CombatReach))
                {
                    Bot.Movement.SetMovementAction(Movement.Enums.MovementAction.Move, LastBehindTargetPosition);
                }
                else
                {
                    isAttackingFromBehind = true;
                    if (!BotMath.IsFacing(LastPlayerPosition, Bot.Player.Rotation, LastTargetPosition, 0.5f))
                    {
                        Bot.Wow.WowFacePosition(Bot.Player.BaseAddress, Bot.Player.Position, target.Position);
                    }

                    Bot.Movement.SetMovementAction(Movement.Enums.MovementAction.Move, LastTargetPosition, LastTargetRotation);
                }
            }
        }

        private bool SearchNewTarget(ref IWowUnit target, bool grinding)
        {
            List<string> buffs = Bot.Player.Auras.Select(e => Bot.Db.GetSpellName(e.SpellId)).ToList();
            if ((Bot.Wow.TargetGuid != 0 && target != null && !(target.IsDead || target.Health < 1 || target.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId).Contains("Spirit of Redem")))) || (buffs.Any(e => e.Contains("tealth")) && Bot.Player.HealthPercentage <= 20))
            {
                return false;
            }

            List<IWowUnit> wowUnits = Bot.Objects.WowObjects.OfType<IWowUnit>().Where(e => Bot.Db.GetReaction(Bot.Player, e) != WowUnitReaction.Friendly && Bot.Db.GetReaction(Bot.Player, e) != WowUnitReaction.Neutral).ToList();
            bool newTargetFound = false;
            int targetHealth = (target == null || target.IsDead || target.Health < 1) ? 0 : target.Health;
            bool inCombat = target != null && target.IsInCombat;
            int targetCount = 0;
            foreach (IWowUnit unit in wowUnits)
            {
                if (IWowUnit.IsValidUnit(unit) && unit != target && !(unit.IsDead || unit.Health < 1 || unit.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId).Contains("Spirit of Redem"))))
                {
                    double tmpDistance = Bot.Player.Position.GetDistance(unit.Position);
                    if ((isSneaky && tmpDistance < 100.0) || isSneaky && tmpDistance < 50.0)
                    {
                        if (tmpDistance < 6.0)
                        {
                            targetCount++;
                        }

                        if (((unit.IsInCombat && unit.Health > targetHealth) || (!inCombat && grinding && unit.Health > targetHealth)) && Bot.Wow.WowIsInLineOfSight(Bot.Player.Position, unit.Position))
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

            if (newTargetFound)
            {
                Bot.Wow.WowTargetGuid(target.Guid);
                spells.ResetAfterTargetDeath();
            }

            return newTargetFound;
        }

        private class RogueAssassinSpells
        {
            private static readonly string Ambush = "Ambush";
            private static readonly string ColdBlood = "Cold Blood";
            private static readonly string DeadlyThrow = "Deadly Throw";
            private static readonly string Envenom = "Envenom";
            private static readonly string Eviscerate = "Eviscerate";
            private static readonly string Garrote = "Garrote";
            private static readonly string HungerForBlood = "Hunger For Blood";
            private static readonly string Kick = "Kick";
            private static readonly string Mutilate = "Mutilate";
            private static readonly string Overkill = "Overkill";
            private static readonly string Rupture = "Rupture";
            private static readonly string SinisterStrike = "Sinister Strike";
            private static readonly string SliceAndDice = "Slice and Dice";
            private static readonly string Sprint = "Sprint";
            private static readonly string Stealth = "Stealth";
            private static readonly string ThrowAttack = "Throw";
            private static readonly string Vanish = "Vanish";

            private readonly AmeisenBotInterfaces Bot;

            private readonly Dictionary<string, DateTime> nextActionTime = new()
            {
                { Garrote, DateTime.Now },
                { Ambush, DateTime.Now },
                { HungerForBlood, DateTime.Now },
                { SliceAndDice, DateTime.Now },
                { Mutilate, DateTime.Now },
                { Envenom, DateTime.Now },
                { Vanish, DateTime.Now },
                { Overkill, DateTime.Now },
                { ColdBlood, DateTime.Now },
                { Stealth, DateTime.Now },
                { Sprint, DateTime.Now },
                { SinisterStrike, DateTime.Now },
                { Rupture, DateTime.Now },
                { Eviscerate, DateTime.Now },
                { DeadlyThrow, DateTime.Now },
                { Kick, DateTime.Now }
            };

            private bool askedForHeal = false;

            private bool askedForHelp = false;

            private int comboCnt = 0;

            public RogueAssassinSpells(AmeisenBotInterfaces bot)
            {
                Bot = bot;
                Player = Bot.Player;
                NextGCDSpell = DateTime.Now;
                NextCast = DateTime.Now;
            }

            private DateTime NextCast { get; set; }

            private DateTime NextGCDSpell { get; set; }

            private IWowPlayer Player { get; set; }

            public void CastNextSpell(double distanceToTarget, IWowUnit target)
            {
                if (!IsReady(NextCast) || !IsReady(NextGCDSpell))
                {
                    return;
                }

                if (!Bot.Player.IsAutoAttacking && !IsInStealth())
                {
                    Bot.Wow.LuaStartAutoAttack();
                }

                Player = Bot.Player;
                int energy = Player.Energy;
                bool lowHealth = Player.HealthPercentage <= 20;
                bool mediumHealth = !lowHealth && Player.HealthPercentage <= 50;
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

                // -- stealth --
                if (!IsInStealth())
                {
                    if (!Player.IsInCombat)
                    {
                        CastSpell(Stealth, ref energy, 0, 1, false);
                    }
                    else if (lowHealth)
                    {
                        if (IsReady(Vanish))
                        {
                            CastSpell(Vanish, ref energy, 0, 180, false);
                            Bot.Wow.WowClearTarget();
                            return;
                        }
                    }
                }

                // combat
                if (distanceToTarget < (29 + target.CombatReach))
                {
                    // in range
                    if (energy > 15 && IsReady(HungerForBlood) && IsTargetBleeding() && !IsInStealth())
                    {
                        CastSpell(HungerForBlood, ref energy, 15, 60, true);
                    }

                    if (distanceToTarget < (24 + target.CombatReach))
                    {
                        if (distanceToTarget > (9 + target.CombatReach))
                        {
                            // 9 < distance < 24
                            // run?
                            if (energy > 15 && IsReady(Sprint) && IsTargetBleeding())
                            {
                                CastSpell(Sprint, ref energy, 15, 180, true);
                            }
                        }
                        else if (distanceToTarget <= 0.75f * (Player.CombatReach + target.CombatReach))
                        {
                            // distance <= 9
                            // close combat
                            if (IsInStealth())
                            {
                                if (energy > 50 && IsReady(Garrote) && !IsTargetBleeding())
                                {
                                    CastSpell(Garrote, ref energy, 50, 3, true);
                                    comboCnt++;
                                }
                                else if (energy > 60)
                                {
                                    CastSpell(Ambush, ref energy, 60, 0, true);
                                    comboCnt += 2;
                                }
                            }
                            else
                            {
                                if (Bot.Wow.LuaGetUnitCastingInfo(WowLuaUnit.Target).Item2 > 0 && energy > 25 && IsReady(Kick))
                                {
                                    CastSpell(Kick, ref energy, 25, 10, true);
                                }
                                else if (comboCnt > 4 && energy > 35 && IsTargetPoisoned())
                                {
                                    if (IsReady(ColdBlood))
                                    {
                                        CastSpell(ColdBlood, ref energy, 0, 180, false);
                                    }

                                    CastSpell(Envenom, ref energy, 35, 0, true);
                                    comboCnt -= 5;
                                }
                                else if (comboCnt > 4 && energy > 35)
                                {
                                    if (IsReady(ColdBlood))
                                    {
                                        CastSpell(ColdBlood, ref energy, 0, 180, false);
                                    }

                                    CastSpell(Eviscerate, ref energy, 35, 0, true);
                                    comboCnt -= 5;
                                }
                                else if (comboCnt > 0 && energy > 25 && IsReady(SliceAndDice))
                                {
                                    int comboCntUsed = Math.Min(5, comboCnt);
                                    CastSpell(SliceAndDice, ref energy, 25, 6 + (3 * comboCntUsed), true);
                                    comboCnt -= comboCntUsed;
                                }
                                else if (energy > 60 && IsTargetPoisoned())
                                {
                                    CastSpell(Mutilate, ref energy, 60, 0, true);
                                    comboCnt += 2;
                                }
                                else if (energy > 45)
                                {
                                    CastSpell(SinisterStrike, ref energy, 45, 0, true);
                                    comboCnt++;
                                }
                            }
                        }
                    }
                    else
                    {
                        // 24 <= distance < 29
                        // distance attacks
                        if (Player.IsInCombat)
                        {
                            if (comboCnt > 4 && energy > 35 && IsReady(DeadlyThrow))
                            {
                                CastSpell(DeadlyThrow, ref energy, 35, 1.5, true);
                                comboCnt -= 5;
                            }
                            else
                            {
                                CastSpell(ThrowAttack, ref energy, 0, 2.1, false);
                                NextCast = DateTime.Now.AddSeconds(0.5); // casting time
                                NextGCDSpell = DateTime.Now.AddSeconds(2.0); // 1.5 sec gcd after the casting time
                            }
                        }
                    }
                }
            }

            public void ResetAfterTargetDeath()
            {
                comboCnt = 0;
                nextActionTime[HungerForBlood] = DateTime.Now;
                nextActionTime[Garrote] = DateTime.Now;
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

            private bool IsInStealth()
            {
                List<string> buffs = Bot.Player.Auras.Select(e => Bot.Db.GetSpellName(e.SpellId)).ToList();
                return buffs.Any(e => e.Contains("tealth"));
            }

            private bool IsReady(string spell)
            {
                bool result = true; // begin with neutral element of AND
                if (spell.Equals(HungerForBlood) || spell.Equals(SliceAndDice) || spell.Equals(Garrote))
                {
                    // only use these spells in a certain interval
                    result &= !nextActionTime.TryGetValue(spell, out DateTime NextSpellAvailable) || IsReady(NextSpellAvailable);
                }

                result &= Bot.Wow.LuaGetSpellCooldown(spell) <= 0 && Bot.Wow.LuaGetUnitCastingInfo(WowLuaUnit.Player).Item2 <= 0;
                return result;
            }

            private bool IsTargetBleeding()
            {
                List<string> buffs = Bot.Target.Auras.Select(e => Bot.Db.GetSpellName(e.SpellId)).ToList();
                return buffs.Any(e => e.Contains("acerate") || e.Contains("Bleed") || e.Contains("bleed") || e.Contains("Rip") || e.Contains("rip")
                 || e.Contains("Rake") || e.Contains("rake") || e.Contains("iercing") || e.Contains("arrote") || e.Contains("emorrhage") || e.Contains("upture") || e.Contains("Wounds") || e.Contains("wounds"));
            }

            private bool IsTargetPoisoned()
            {
                List<string> buffs = Bot.Target.Auras.Select(e => Bot.Db.GetSpellName(e.SpellId)).ToList();
                return buffs.Any(e => e.Contains("Poison") || e.Contains("poison"));
            }
        }
    }
}