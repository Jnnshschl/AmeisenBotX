using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.Movement;
using AmeisenBotX.Pathfinding;
using AmeisenBotX.Pathfinding.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.StateMachine.CombatClasses
{
    public class WarriorFury : ICombatClass
    {
        public WarriorFury(ObjectManager objectManager, CharacterManager characterManager, HookManager hookManager, IPathfindingHandler pathhandler, DefaultMovementEngine movement)
        {
            ObjectManager = objectManager;
            CharacterManager = characterManager;
            HookManager = hookManager;
            PathfindingHandler = pathhandler;
            MovementEngine = movement;
            Spells = new WarriorFurySpells(hookManager, objectManager);
        }

        public bool HandlesMovement => true;

        public bool HandlesTargetSelection => true;

        public bool IsMelee => true;

        private bool hasTargetMoved = false;
        private bool computeNewRoute = false;
        private readonly WarriorFurySpells Spells;

        private class WarriorFurySpells
        {
            static readonly string battleShout = "Battle Shout";
            static readonly string battleStance = "Battle Stance"; // noGCD
            static readonly string berserkerStance = "Berserker Stance"; // noGCD
            static readonly string berserkerRage = "Berserker Rage";
            static readonly string slam = "Slam";
            static readonly string recklessness = "Recklessness";
            static readonly string deathWish = "Death Wish";
            static readonly string intercept = "Intercept";
            static readonly string shatteringThrow = "Shattering Throw";
            static readonly string heroicThrow = "Heroic Throw";
            static readonly string charge = "Charge"; // noGCD
            static readonly string bloodthirst = "Bloodthirst";
            static readonly string hamstring = "Hamstring";
            static readonly string execute = "Execute";
            static readonly string heroicStrike = "Heroic Strike"; // noGCD, wird aber so behandelt, da low priority
            private HookManager HookManager { get; set; }
            private ObjectManager ObjectManager { get; set; }
            private WowPlayer Player { get; set; }
            private readonly Dictionary<string, DateTime> NextActionTime = new Dictionary<string, DateTime>()
            {
                { battleShout, DateTime.Now },
                { battleStance, DateTime.Now },
                { berserkerStance, DateTime.Now },
                { berserkerRage, DateTime.Now },
                { slam, DateTime.Now },
                { recklessness, DateTime.Now },
                { deathWish, DateTime.Now },
                { intercept, DateTime.Now },
                { shatteringThrow, DateTime.Now },
                { heroicThrow, DateTime.Now },
                { charge, DateTime.Now },
                { bloodthirst, DateTime.Now },
                { hamstring, DateTime.Now },
                { execute, DateTime.Now },
                { heroicStrike, DateTime.Now }
            };
            private DateTime NextGCDSpell { get; set; }
            private DateTime NextStance { get; set; }
            private DateTime NextCast { get; set; }
            private bool IsInBerserkerStance { get; set; }
            private bool askedForHelp = false;
            private bool askedForHeal = false;
            public WarriorFurySpells(HookManager hookManager, ObjectManager objectManager)
            {
                HookManager = hookManager;
                ObjectManager = objectManager;
                Player = ObjectManager.Player;
                IsInBerserkerStance = false;
                NextGCDSpell = DateTime.Now;
                NextStance = DateTime.Now;
                NextCast = DateTime.Now;
            }
            public void CastNextSpell(double distanceToTarget, WowUnit target)
            {
                if(!IsReady(NextCast))
                {
                    return;
                }
                if (!ObjectManager.Player.IsAutoAttacking)
                {
                    HookManager.StartAutoAttack();
                }
                Player = ObjectManager.Player;
                int rage = Player.Rage;
                bool isGCDReady = IsReady(NextGCDSpell);
                bool lowHealth = Player.HealthPercentage <= 20;
                bool mediumHealth = !lowHealth && Player.HealthPercentage <= 50;
                if(!(lowHealth || mediumHealth))
                {
                    askedForHelp = false;
                    askedForHeal = false;
                }
                else if(lowHealth && !askedForHelp)
                {
                    HookManager.SendChatMessage("/helpme");
                    askedForHelp = true;
                } else if(mediumHealth && !askedForHeal)
                {
                    HookManager.SendChatMessage("/healme");
                    askedForHeal = true;
                }
                // -- buffs --
                if (isGCDReady)
                {
                    // Death Wish
                    if (!lowHealth && rage > 10 && IsReady(deathWish))
                    {
                        CastSpell(deathWish, ref rage, 10, 120.6, true); // lasts 30 sec
                    }
                    // Battleshout
                    else if (rage > 10 && IsReady(battleShout))
                    {
                        CastSpell(battleShout, ref rage, 10, 120, true); // lasts 2 min
                    }
                    // Recklessness (in Berserker Stance)
                    else if (IsInBerserkerStance && rage > 10 && Player.HealthPercentage > 50 && IsReady(recklessness))
                    {
                        CastSpell(recklessness, ref rage, 0, 201, true); // lasts 12 sec
                    }
                    // Berserker Rage
                    else if (Player.Health < Player.MaxHealth && IsReady(berserkerRage))
                    {
                        CastSpell(berserkerRage, ref rage, 0, 20.1, true); // lasts 10 sec
                    }
                }
                if (distanceToTarget < 29)
                {
                    if(distanceToTarget < 24)
                    {
                        if(distanceToTarget > 9)
                        {
                            // -- run to the target! --
                            if (Player.IsInCombat)
                            {
                                if (isGCDReady)
                                {
                                    if(IsInBerserkerStance)
                                    {
                                        // intercept
                                        if(rage > 10 && IsReady(intercept))
                                        {
                                            CastSpell(intercept, ref rage, 10, 30, true);
                                        }
                                    }
                                    else
                                    {
                                        // Berserker Stance
                                        if (IsReady(NextStance))
                                        {
                                            ChangeToStance(berserkerStance, out rage);
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
                                        ChangeToStance(battleStance, out rage);
                                    }
                                }
                                else
                                {
                                    // charge
                                    if (IsReady(charge))
                                    {
                                        CastSpell(charge, ref rage, 0, 15, false);
                                    }
                                }
                            }
                        }
                        else
                        {
                            // -- close combat --
                            // Berserker Stance
                            if (!IsInBerserkerStance && IsReady(NextStance))
                            {
                                ChangeToStance(berserkerStance, out rage);
                            }
                            else if (isGCDReady)
                            {
                                // bloodthirst
                                if (lowHealth && rage > 20 && IsReady(bloodthirst))
                                {
                                    CastSpell(bloodthirst, ref rage, 20, 4, true);
                                }
                                // slam
                                else
                                {
                                    List<string> Buffs = HookManager.GetBuffs(WowLuaUnit.Player);
                                    if(Buffs.Any(e => e.Contains("slam")) && rage > 15)
                                    {
                                        CastSpell(slam, ref rage, 15, 0, false);
                                        NextCast = DateTime.Now.AddSeconds(1.5); // casting time
                                        NextGCDSpell = DateTime.Now.AddSeconds(3.0); // 1.5 sec gcd after the 1.5 sec casting time
                                    }
                                    // hamstring
                                    else if(rage > 10 && IsReady(hamstring))
                                    {
                                        CastSpell(hamstring, ref rage, 10, 15, true);
                                    }
                                    // execute
                                    else if (target.HealthPercentage <= 20 && rage > 10)
                                    {
                                        CastSpell(execute, ref rage, 10, 0, true);
                                    }
                                    // heroic strike
                                    else if (rage > 12 && IsReady(heroicStrike))
                                    {
                                        CastSpell(heroicStrike, ref rage, 12, 3.6, false);
                                    }
                                }
                            }
                            // heroic strike
                            else if (target.HealthPercentage > 20 && rage > 12 && IsReady(heroicStrike))
                            {
                                CastSpell(heroicStrike, ref rage, 12, 3.6, false);
                            }
                            else
                            {
                                if (!ObjectManager.Player.IsAutoAttacking)
                                {
                                    HookManager.StartAutoAttack();
                                }
                            }
                        }
                    }
                    else
                    {
                        if(isGCDReady)
                        {
                            // -- distant attacks --
                            if (isGCDReady)
                            {
                                // shattering throw (in Battle Stance)
                                if(rage > 25 && IsReady(shatteringThrow))
                                {
                                    if (IsInBerserkerStance)
                                    {
                                        if (IsReady(NextStance))
                                        {
                                            ChangeToStance(battleStance, out rage);
                                        }
                                    }
                                    else
                                    {
                                        CastSpell(shatteringThrow, ref rage, 25, 301.5, false);
                                        NextCast = DateTime.Now.AddSeconds(1.5); // casting time
                                        NextGCDSpell = DateTime.Now.AddSeconds(3.0); // 1.5 sec gcd after the 1.5 sec casting time
                                    }
                                }
                                // heroic throw
                                else
                                {
                                    CastSpell(heroicThrow, ref rage, 0, 60, true);
                                }
                            }
                        }
                    }
                }
            }
            public void ResetAfterTargetDeath()
            {
                NextActionTime[hamstring] = DateTime.Now;
            }
            private bool IsReady(DateTime nextAction)
            {
                return DateTime.Now > nextAction;
            }
            private bool IsReady(string spell)
            {
                return !NextActionTime.TryGetValue(spell, out DateTime NextSpellAvailable) || IsReady(NextSpellAvailable);
            }
            private int UpdateRage()
            {
                ObjectManager.UpdateObject(ObjectManager.Player.Type, ObjectManager.Player.BaseAddress);
                Player = ObjectManager.Player;
                return Player.Rage;
            }
            private void CastSpell(string spell, ref int rage, int rageCosts, double cooldown, bool gcd)
            {
                HookManager.CastSpell(spell);
                rage -= rageCosts;
                if(cooldown > 0)
                {
                    NextActionTime[spell] = DateTime.Now.AddSeconds(cooldown);
                }
                if (gcd)
                {
                    NextGCDSpell = DateTime.Now.AddSeconds(1.5);
                }
            }
            private void ChangeToStance(string stance, out int rage)
            {
                HookManager.CastSpell(stance);
                rage = UpdateRage();
                NextStance = DateTime.Now.AddSeconds(1);
                IsInBerserkerStance = stance == berserkerStance;
            }
        }

        private CharacterManager CharacterManager { get; }

        private HookManager HookManager { get; }

        private Vector3 LastPlayerPosition { get; set; }
        private Vector3 LastTargetPosition { get; set; }
        private double distanceToTarget = 0;
        private double distanceTraveled = 0;

        private ObjectManager ObjectManager { get; }

        private bool Dancing { get; set; }

        private IPathfindingHandler PathfindingHandler { get; set; }

        private DefaultMovementEngine MovementEngine { get; set; }

        public void Execute()
        {
            ulong targetGuid = ObjectManager.TargetGuid;
            WowUnit target = ObjectManager.WowObjects.OfType<WowUnit>().FirstOrDefault(t => t.Guid == targetGuid);
            SearchNewTarget(ref target, false);
            if (target != null)
            {
                bool targetDistanceChanged = false;
                if(!LastPlayerPosition.Equals(ObjectManager.Player.Position))
                {
                    distanceTraveled = ObjectManager.Player.Position.GetDistance(LastPlayerPosition);
                    LastPlayerPosition = new Vector3(ObjectManager.Player.Position.X, ObjectManager.Player.Position.Y, ObjectManager.Player.Position.Z);
                    targetDistanceChanged = true;
                }
                if (!LastTargetPosition.Equals(target.Position))
                {
                    hasTargetMoved = true;
                    LastTargetPosition = new Vector3(target.Position.X, target.Position.Y, target.Position.Z);
                    targetDistanceChanged = true;
                }
                else if(hasTargetMoved)
                {
                    hasTargetMoved = false;
                    computeNewRoute = true;
                }
                if(targetDistanceChanged)
                {
                    distanceToTarget = LastPlayerPosition.GetDistance(LastTargetPosition);
                }
                HandleMovement(target);
                HandleAttacking(target);
            }
        }

        public void OutOfCombatExecute()
        {
            if (!LastPlayerPosition.Equals(ObjectManager.Player.Position))
            {
                distanceTraveled = ObjectManager.Player.Position.GetDistance(LastPlayerPosition);
                LastPlayerPosition = new Vector3(ObjectManager.Player.Position.X, ObjectManager.Player.Position.Y, ObjectManager.Player.Position.Z);
            }
            if (distanceTraveled < 0.001)
            {
                ulong leaderGuid = ObjectManager.ReadPartyLeaderGuid();
                WowUnit target = null;
                if (leaderGuid == ObjectManager.PlayerGuid && SearchNewTarget(ref target, true))
                {
                    if (!LastTargetPosition.Equals(target.Position))
                    {
                        hasTargetMoved = true;
                        LastTargetPosition = new Vector3(target.Position.X, target.Position.Y, target.Position.Z);
                        distanceToTarget = LastPlayerPosition.GetDistance(LastTargetPosition);
                    }
                    else
                    {
                        computeNewRoute = true;
                        hasTargetMoved = false;
                    }
                    HandleMovement(target);
                    HandleAttacking(target);
                }
                else if(!Dancing)
                {
                    HookManager.ClearTarget();
                    HookManager.SendChatMessage("/dance");
                    Dancing = true;
                }
            } 
            else
            {
                Dancing = false;
            }
        }

        private bool SearchNewTarget (ref WowUnit? target, bool grinding)
        {
            List<WowUnit> wowUnits = ObjectManager.WowObjects.OfType<WowUnit>().Where(e => HookManager.GetUnitReaction(ObjectManager.Player, e) != WowUnitReaction.Friendly && HookManager.GetUnitReaction(ObjectManager.Player, e) != WowUnitReaction.Neutral).ToList();
            bool newTargetFound = false;
            int targetHealth = (target == null || target.IsDead) ? 2147483647 : target.Health;
            bool inCombat = target == null ? false : target.IsInCombat;
            foreach (WowUnit unit in wowUnits)
            {
                if (BotUtils.IsValidUnit(unit) && unit != target && !unit.IsDead && ObjectManager.Player.Position.GetDistance(unit.Position) < 100)
                {
                    if((unit.IsInCombat && unit.Health < targetHealth) || (!inCombat && grinding && unit.Health < targetHealth))
                    {
                        target = unit;
                        targetHealth = unit.Health;
                        newTargetFound = true;
                        inCombat = unit.IsInCombat;
                    }
                }
            }
            if(target == null || target.IsDead)
            {
                HookManager.ClearTarget();
                newTargetFound = false;
                target = null;
            }
            if(newTargetFound)
            {
                HookManager.TargetGuid(target.Guid);
            }
            return newTargetFound;
        }

        private void HandleAttacking(WowUnit target)
        {
            Spells.CastNextSpell(distanceToTarget, target);
            if(target.IsDead)
            {
                Spells.ResetAfterTargetDeath();
            }
        }

        private void HandleMovement(WowUnit target)
        {
            if(target == null)
            {
                return;
            }

            if (hasTargetMoved || (distanceToTarget < 6.0 && !BotMath.IsFacing(LastPlayerPosition, ObjectManager.Player.Rotation, LastTargetPosition, 0.75, 1.25)))
            {
                CharacterManager.MoveToPosition(LastTargetPosition);
            }
            else if(distanceToTarget >= 6.0)
            {
                if (computeNewRoute || MovementEngine.CurrentPath?.Count == 0)
                {
                    List<Vector3> path = PathfindingHandler.GetPath(ObjectManager.MapId, LastPlayerPosition, LastTargetPosition);
                    MovementEngine.LoadPath(path);
                    MovementEngine.PostProcessPath();
                }
                else
                {
                    if (MovementEngine.GetNextStep(LastPlayerPosition, ObjectManager.Player.Rotation, out Vector3 positionToGoTo, out bool needToJump))
                    {
                        CharacterManager.MoveToPosition(positionToGoTo);

                        if (needToJump)
                        {
                            CharacterManager.Jump();
                        }
                    }
                }
            }
        }
    }
}