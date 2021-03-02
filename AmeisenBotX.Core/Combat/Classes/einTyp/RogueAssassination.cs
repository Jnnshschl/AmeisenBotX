using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Combat.Classes.einTyp
{
    public class RogueAssassination : ICombatClass
    {
        private readonly bool hasTargetMoved = false;
        private readonly RogueAssassinSpells spells;
        private readonly string[] standingEmotes = { "/bored" };
        private readonly WowInterface WowInterface;
        private bool computeNewRoute = false;

        private double distanceToBehindTarget = 0;

        private double distanceToTarget = 0;

        private double distanceTraveled = 0;
        private bool isAttackingFromBehind = false;

        private bool isSneaky = false;

        private bool standing = false;

        private bool wasInStealth = false;

        public RogueAssassination(WowInterface wowInterface)
        {
            WowInterface = wowInterface;
            spells = new RogueAssassinSpells(wowInterface);
        }

        public string Author => "einTyp";

        public IEnumerable<int> BlacklistedTargetDisplayIds { get; set; }

        public Dictionary<string, dynamic> C { get; set; } = new Dictionary<string, dynamic>();

        public string Description => "...";

        public string Displayname => "Assasination Rogue";

        public bool HandlesFacing => false;

        public bool HandlesMovement => true;

        public bool HandlesTargetSelection => true;

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
            WowUnit target = WowInterface.Target;
            if (target == null)
            {
                return;
            }

            if (WowInterface.Player.Position.GetDistance(target.Position) <= 3.0)
            {
                WowInterface.HookManager.WowStopClickToMove();
                WowInterface.MovementEngine.Reset();
                WowInterface.HookManager.WowUnitRightClick(target);
            }
            else
            {
                WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, target.Position);
            }
        }

        public void Execute()
        {
            computeNewRoute = false;
            WowUnit target = WowInterface.Target;
            if ((WowInterface.TargetGuid != 0 && target != null && !(target.IsDead || target.Health < 1)) || SearchNewTarget(ref target, false))
            {
                Dancing = false;
                bool targetDistanceChanged = false;
                if (!LastPlayerPosition.Equals(WowInterface.Player.Position))
                {
                    distanceTraveled = WowInterface.Player.Position.GetDistance(LastPlayerPosition);
                    LastPlayerPosition = new Vector3(WowInterface.Player.Position.X, WowInterface.Player.Position.Y, WowInterface.Player.Position.Z);
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
                    WowInterface.HookManager.WowClearTarget();
                    WowInterface.HookManager.LuaSendChatMessage(standingEmotes[new Random().Next(standingEmotes.Length)]);
                    Dancing = true;
                    WowInterface.Globals.ForceCombat = false;
                }
                else
                {
                    WowInterface.HookManager.WowClearTarget();
                    Dancing = true;
                }
            }
        }

        public bool IsTargetAttackable(WowUnit target)
        {
            return true;
        }

        public void OutOfCombatExecute()
        {
            computeNewRoute = false;
            List<string> buffs = WowInterface.Player.Auras.Select(e => e.Name).ToList();
            if (!buffs.Any(e => e.Contains("tealth")))
            {
                WowInterface.HookManager.LuaCastSpell("Stealth");
                spells.ResetAfterTargetDeath();
            }

            if (!LastPlayerPosition.Equals(WowInterface.Player.Position))
            {
                distanceTraveled = WowInterface.Player.Position.GetDistance(LastPlayerPosition);
                LastPlayerPosition = new Vector3(WowInterface.Player.Position.X, WowInterface.Player.Position.Y, WowInterface.Player.Position.Z);
            }

            if (distanceTraveled < 0.001)
            {
                ulong leaderGuid = WowInterface.ObjectManager.PartyleaderGuid;
                WowUnit target = WowInterface.Target;
                if ((WowInterface.TargetGuid != 0 && target != null && !(target.IsDead || target.Health < 1)) || SearchNewTarget(ref target, true))
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
                    WowInterface.Globals.ForceCombat = true;
                    HandleAttacking(target);
                }
                else if (!Dancing || standing)
                {
                    standing = false;
                    WowInterface.HookManager.WowClearTarget();
                    WowInterface.HookManager.LuaSendChatMessage(standingEmotes[new Random().Next(standingEmotes.Length)]);
                    Dancing = true;
                }
            }
            else
            {
                if (!Dancing || !standing)
                {
                    standing = true;
                    WowInterface.HookManager.WowClearTarget();
                    Dancing = true;
                }
            }
        }

        private void HandleAttacking(WowUnit target)
        {
            WowInterface.HookManager.WowTargetGuid(target.Guid);
            spells.CastNextSpell(distanceToTarget, target);
            if (target.IsDead || target.Health < 1)
            {
                spells.ResetAfterTargetDeath();
            }
        }

        private void HandleMovement(WowUnit target)
        {
            if (target == null)
            {
                return;
            }

            if (WowInterface.Player.Auras.Any(e => e.Name.Contains("tealth")))
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
                if (WowInterface.MovementEngine.Status != Movement.Enums.MovementAction.None && distanceToTarget < 0.75f * (WowInterface.Player.CombatReach + target.CombatReach))
                {
                    WowInterface.MovementEngine.StopMovement();
                }

                if (WowInterface.Player.IsInCombat)
                {
                    isAttackingFromBehind = false;
                }
            }

            if (computeNewRoute)
            {
                if (!isAttackingFromBehind && isSneaky && distanceToBehindTarget > 0.75f * (WowInterface.Player.CombatReach + target.CombatReach))
                {
                    WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Move, LastBehindTargetPosition);
                }
                else
                {
                    isAttackingFromBehind = true;
                    if (!BotMath.IsFacing(LastPlayerPosition, WowInterface.Player.Rotation, LastTargetPosition, 0.5f))
                    {
                        WowInterface.HookManager.WowFacePosition(WowInterface.Player, target.Position);
                    }

                    WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Move, LastTargetPosition, LastTargetRotation);
                }
            }
        }

        private bool SearchNewTarget(ref WowUnit target, bool grinding)
        {
            List<string> buffs = WowInterface.Player.Auras.Select(e => e.Name).ToList();
            if ((WowInterface.TargetGuid != 0 && target != null && !(target.IsDead || target.Health < 1 || target.Auras.Any(e => e.Name.Contains("Spirit of Redem")))) || (buffs.Any(e => e.Contains("tealth")) && WowInterface.Player.HealthPercentage <= 20))
            {
                return false;
            }

            List<WowUnit> wowUnits = WowInterface.ObjectManager.WowObjects.OfType<WowUnit>().Where(e => WowInterface.HookManager.WowGetUnitReaction(WowInterface.Player, e) != WowUnitReaction.Friendly && WowInterface.HookManager.WowGetUnitReaction(WowInterface.Player, e) != WowUnitReaction.Neutral).ToList();
            bool newTargetFound = false;
            int targetHealth = (target == null || target.IsDead || target.Health < 1) ? 0 : target.Health;
            bool inCombat = target == null ? false : target.IsInCombat;
            int targetCount = 0;
            foreach (WowUnit unit in wowUnits)
            {
                if (BotUtils.IsValidUnit(unit) && unit != target && !(unit.IsDead || unit.Health < 1 || unit.Auras.Any(e => e.Name.Contains("Spirit of Redem"))))
                {
                    double tmpDistance = WowInterface.Player.Position.GetDistance(unit.Position);
                    if ((isSneaky && tmpDistance < 100.0) || isSneaky && tmpDistance < 50.0)
                    {
                        if (tmpDistance < 6.0)
                        {
                            targetCount++;
                        }

                        if (((unit.IsInCombat && unit.Health > targetHealth) || (!inCombat && grinding && unit.Health > targetHealth)) && WowInterface.HookManager.WowIsInLineOfSight(WowInterface.Player.Position, unit.Position))
                        {
                            target = unit;
                            targetHealth = unit.Health;
                            newTargetFound = true;
                            inCombat = unit.IsInCombat;
                        }
                    }
                }
            }

            if (target == null || target.IsDead || target.Health < 1 || target.Auras.Any(e => e.Name.Contains("Spirit of Redem")))
            {
                WowInterface.HookManager.WowClearTarget();
                newTargetFound = false;
                target = null;
            }

            if (newTargetFound)
            {
                WowInterface.HookManager.WowTargetGuid(target.Guid);
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

            private readonly Dictionary<string, DateTime> nextActionTime = new Dictionary<string, DateTime>()
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

            private readonly WowInterface WowInterface;
            private bool askedForHeal = false;

            private bool askedForHelp = false;

            private int comboCnt = 0;

            public RogueAssassinSpells(WowInterface wowInterface)
            {
                WowInterface = wowInterface;
                Player = WowInterface.Player;
                NextGCDSpell = DateTime.Now;
                NextCast = DateTime.Now;
            }

            private DateTime NextCast { get; set; }

            private DateTime NextGCDSpell { get; set; }

            private WowPlayer Player { get; set; }

            public void CastNextSpell(double distanceToTarget, WowUnit target)
            {
                if (!IsReady(NextCast) || !IsReady(NextGCDSpell))
                {
                    return;
                }

                if (!WowInterface.Player.IsAutoAttacking && !IsInStealth())
                {
                    WowInterface.HookManager.LuaStartAutoAttack();
                }

                Player = WowInterface.Player;
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
                    WowInterface.HookManager.LuaSendChatMessage("/helpme");
                    askedForHelp = true;
                }
                else if (mediumHealth && !askedForHeal)
                {
                    WowInterface.HookManager.LuaSendChatMessage("/healme");
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
                            WowInterface.HookManager.WowClearTarget();
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
                                if (WowInterface.HookManager.LuaGetUnitCastingInfo(WowLuaUnit.Target).Item2 > 0 && energy > 25 && IsReady(Kick))
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

            private void CastSpell(string spell, ref int rage, int rageCosts, double cooldown, bool gcd)
            {
                WowInterface.HookManager.LuaCastSpell(spell);
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
                List<string> buffs = WowInterface.Player.Auras.Select(e => e.Name).ToList();
                return buffs.Any(e => e.Contains("tealth"));
            }

            private bool IsReady(DateTime nextAction)
            {
                return DateTime.Now > nextAction;
            }

            private bool IsReady(string spell)
            {
                bool result = true; // begin with neutral element of AND
                if (spell.Equals(HungerForBlood) || spell.Equals(SliceAndDice) || spell.Equals(Garrote))
                {
                    // only use these spells in a certain interval
                    result &= !nextActionTime.TryGetValue(spell, out DateTime NextSpellAvailable) || IsReady(NextSpellAvailable);
                }

                result &= WowInterface.HookManager.LuaGetSpellCooldown(spell) <= 0 && WowInterface.HookManager.LuaGetUnitCastingInfo(WowLuaUnit.Player).Item2 <= 0;
                return result;
            }

            private bool IsTargetBleeding()
            {
                List<string> buffs = WowInterface.Target.Auras.Select(e => e.Name).ToList();
                return buffs.Any(e => e.Contains("acerate") || e.Contains("Bleed") || e.Contains("bleed") || e.Contains("Rip") || e.Contains("rip")
                 || e.Contains("Rake") || e.Contains("rake") || e.Contains("iercing") || e.Contains("arrote") || e.Contains("emorrhage") || e.Contains("upture") || e.Contains("Wounds") || e.Contains("wounds"));
            }

            private bool IsTargetPoisoned()
            {
                List<string> buffs = WowInterface.Target.Auras.Select(e => e.Name).ToList();
                return buffs.Any(e => e.Contains("Poison") || e.Contains("poison"));
            }
        }
    }
}