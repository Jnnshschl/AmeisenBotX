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
            Jumped = false;
            Spells = new WarriorFurySpells(hookManager, objectManager);
        }

        public bool HandlesMovement => true;

        public bool HandlesTargetSelection => true;

        public bool IsMelee => true;

        private bool Jumped { get; set; }
        private WarriorFurySpells Spells;

        private class WarriorFurySpells
        {
            static string battleShout = "Battle Shout";
            static string battleStance = "Battle Stance"; // noGCD
            static string berserkerStance = "Berserker Stance"; // noGCD
            static string berserkerRage = "Berserker Rage";
            static string slam = "Slam";
            static string recklessness = "Recklessness";
            static string deathWish = "Death Wish";
            static string intercept = "Intercept";
            static string shatteringThrow = "Shattering Throw";
            static string heroicThrow = "Heroic Throw";
            static string charge = "Charge"; // noGCD
            static string bloodthirst = "Bloodthirst";
            static string hamstring = "Hamstring";
            static string execute = "Execute";
            static string heroicStrike = "Heroic Strike"; // noGCD, wird aber so behandelt, da low priority
            private HookManager HookManager { get; set; }
            private ObjectManager ObjectManager { get; set; }
            private WowPlayer Player { get; set; }
            private Dictionary<string, DateTime> NextActionTime = new Dictionary<string, DateTime>()
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
            public void castNextSpell(double distanceToTarget, WowUnit target)
            {
                if(!IsReady(NextCast))
                {
                    return;
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
                        castSpell(deathWish, ref rage, 10, 120.6, true); // lasts 30 sec
                    }
                    // Battleshout
                    else if (rage > 10 && IsReady(battleShout))
                    {
                        castSpell(battleShout, ref rage, 10, 120, true); // lasts 2 min
                    }
                    // Recklessness (in Berserker Stance)
                    else if (IsInBerserkerStance && rage > 10 && Player.HealthPercentage > 50 && IsReady(recklessness))
                    {
                        castSpell(recklessness, ref rage, 0, 201, true); // lasts 12 sec
                    }
                    // Berserker Rage
                    else if (Player.Health < Player.MaxHealth && IsReady(berserkerRage))
                    {
                        castSpell(berserkerRage, ref rage, 0, 20.1, true); // lasts 10 sec
                    }
                }
                if (distanceToTarget < 30)
                {
                    if(distanceToTarget < 25)
                    {
                        if(distanceToTarget > 8)
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
                                            castSpell(intercept, ref rage, 10, 30, true);
                                            HookManager.SendChatMessage("/s gotcha!");
                                        }
                                    }
                                    else
                                    {
                                        // Berserker Stance
                                        if (IsReady(NextStance))
                                        {
                                            changeToStance(berserkerStance, out rage);
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
                                        changeToStance(battleStance, out rage);
                                        // charge
                                        if (IsReady(charge))
                                        {
                                            castSpell(charge, ref rage, 0, 15, false);
                                            HookManager.SendChatMessage("/s boo!");
                                        }
                                    }
                                }
                                else
                                {
                                    // charge
                                    if (IsReady(charge))
                                    {
                                        castSpell(charge, ref rage, 0, 15, false);
                                        HookManager.SendChatMessage("/s boo!");
                                    }
                                }
                            }
                        }
                        else if(distanceToTarget < 3)
                        {
                            // -- close combat --
                            // Berserker Stance
                            if (IsReady(NextStance))
                            {
                                changeToStance(berserkerStance, out rage);
                            }
                            if (isGCDReady)
                            {
                                // bloodthirst
                                if (lowHealth && rage > 20 && IsReady(bloodthirst))
                                {
                                    castSpell(bloodthirst, ref rage, 20, 4, true);
                                    HookManager.SendChatMessage("/s oooh shit");
                                }
                                // slam
                                else
                                {
                                    List<string> Buffs = HookManager.GetBuffs(WowLuaUnit.Player.ToString());
                                    if(Buffs.Any(e => e.Contains("slam")) && rage > 15)
                                    {
                                        castSpell(slam, ref rage, 15, 0, false);
                                        NextCast = DateTime.Now.AddSeconds(1.5); // casting time
                                        NextGCDSpell = DateTime.Now.AddSeconds(3.0); // 1.5 sec gcd after the 1.5 sec casting time
                                    }
                                    // hamstring
                                    else if(rage > 10 && IsReady(hamstring))
                                    {
                                        castSpell(hamstring, ref rage, 10, 15, true);
                                    }
                                    // execute
                                    else if (target.HealthPercentage <= 20 && rage > 10)
                                    {
                                        castSpell(execute, ref rage, 10, 0, true);
                                    }
                                    // heroic strike
                                    else if (rage > 12)
                                    {
                                        castSpell(heroicStrike, ref rage, 12, 0, false);
                                    }
                                }
                            }
                        }
                    } else
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
                                            changeToStance(battleStance, out rage);
                                            if(rage > 25)
                                            {
                                                castSpell(shatteringThrow, ref rage, 25, 301.5, false);
                                                NextCast = DateTime.Now.AddSeconds(1.5); // casting time
                                                NextGCDSpell = DateTime.Now.AddSeconds(3.0); // 1.5 sec gcd after the 1.5 sec casting time
                                                HookManager.SendChatMessage("/s and i'm like.. bam!");
                                            }
                                        }
                                    }
                                    else
                                    {
                                        castSpell(shatteringThrow, ref rage, 25, 301.5, false);
                                        NextCast = DateTime.Now.AddSeconds(1.5); // casting time
                                        NextGCDSpell = DateTime.Now.AddSeconds(3.0); // 1.5 sec gcd after the 1.5 sec casting time
                                        HookManager.SendChatMessage("/s and i'm like.. bam!");
                                    }
                                }
                                // heroic throw
                                else
                                {
                                    castSpell(heroicThrow, ref rage, 0, 60, true);
                                    HookManager.SendChatMessage("/s drive by shootin, baby!");
                                }
                            }
                        }
                    }
                }
            }
            public void resetAfterTargetDeath()
            {
                DateTime NextSpellAvailable;
                if(NextActionTime.TryGetValue(hamstring, out NextSpellAvailable))
                {
                    NextSpellAvailable = DateTime.Now;
                }
            }
            private bool IsReady(DateTime nextAction)
            {
                return DateTime.Now > nextAction;
            }
            private bool IsReady(string spell)
            {
                DateTime NextSpellAvailable;
                return !NextActionTime.TryGetValue(spell, out NextSpellAvailable) || IsReady(NextSpellAvailable);
            }
            private int updateRage()
            {
                ObjectManager.UpdateObject(ObjectManager.Player.Type, ObjectManager.Player.BaseAddress);
                Player = ObjectManager.Player;
                return Player.Rage;
            }
            private void castSpell(string spell, ref int rage, int rageCosts, double cooldown, bool gcd)
            {
                HookManager.CastSpell(spell);
                rage -= rageCosts;
                if(cooldown > 0)
                {
                    DateTime NextSpellAvailable;
                    if(NextActionTime.TryGetValue(spell, out NextSpellAvailable))
                    {
                        NextSpellAvailable = DateTime.Now.AddSeconds(cooldown);
                    }
                }
                if (gcd)
                {
                    NextGCDSpell = DateTime.Now.AddSeconds(1.5);
                }
            }
            private void changeToStance(string stance, out int rage)
            {
                HookManager.CastSpell(stance);
                rage = updateRage();
                NextStance = DateTime.Now.AddSeconds(1);
                IsInBerserkerStance = stance == berserkerStance;
            }
        }

        private CharacterManager CharacterManager { get; }

        private HookManager HookManager { get; }

        private Vector3 LastPlayerPosition { get; set; }
        private Vector3 LastTargetPosition { get; set; }
        private double distanceToTarget = 0;

        private ObjectManager ObjectManager { get; }

        private DateTime LastRage { get; set; }
        private DateTime LastReckless { get; set; }
        private DateTime LastShout { get; set; }
        private DateTime LastWish { get; set; }
        private DateTime LastHamstring { get; set; }
        private DateTime LastIntercept { get; set; }
        private DateTime LastShatter { get; set; }
        private DateTime LastHero { get; set; }
        private DateTime LastCharge { get; set; }
        private DateTime LastThirst { get; set; }
        private bool Dancing { get; set; }
        private DateTime LastGCD { get; set; }
        private double GCDTime { get; set; }


        private bool IsGCD()
        {
            return DateTime.Now.Subtract(LastGCD).TotalSeconds < GCDTime;
        }
        private void SetGCD(double GCDinSec)
        {
            GCDTime = GCDinSec;
            LastGCD = DateTime.Now;
        }

        private IPathfindingHandler PathfindingHandler { get; set; }

        private DefaultMovementEngine MovementEngine { get; set; }

        public void Execute()
        {
            ulong targetGuid = ObjectManager.TargetGuid;
            WowUnit target = ObjectManager.WowObjects.OfType<WowUnit>().FirstOrDefault(t => t.Guid == targetGuid);
            SearchNewTarget(ref target, false);
            if (target != null)
            {
                // make sure we're auto attacking
                if (LastPlayerPosition != ObjectManager.Player.Position || LastTargetPosition != target.Position)
                {
                    LastPlayerPosition = ObjectManager.Player.Position;
                    LastTargetPosition = target.Position;
                    distanceToTarget = ObjectManager.Player.Position.GetDistance2D(target.Position);
                }
                if (!ObjectManager.Player.IsAutoAttacking)
                {
                    HookManager.StartAutoAttack();
                }
                if (ObjectManager.Player.IsInCombat || distanceToTarget <= 8 || distanceToTarget >= 25)
                {
                    HandleMovement(target);
                }
                HandleAttacking(target);
            }
        }

        public void OutOfCombatExecute()
        {
            double distanceTraveled = ObjectManager.Player.Position.GetDistance(LastPlayerPosition);
            if(distanceTraveled < 0.001)
            {
                ulong leaderGuid = ObjectManager.ReadPartyLeaderGuid();
                WowUnit target = null;
                if (leaderGuid == ObjectManager.PlayerGuid && SearchNewTarget(ref target, true))
                {
                    HandleMovement(target);
                    HandleAttacking(target);
                }
                else if(!Dancing)
                {
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
                    if((unit.IsInCombat && unit.Health < targetHealth) || !inCombat && grinding && (target == null || target.IsDead) && unit.Health < targetHealth)
                    {
                        target = unit;
                        targetHealth = unit.Health;
                        HookManager.TargetGuid(target.Guid);
                        newTargetFound = true;
                        inCombat = unit.IsInCombat;
                    }
                }
            }
            if(target == null || target.IsDead)
            {
                HookManager.ClearTarget();
                ulong leaderGuid = ObjectManager.ReadPartyLeaderGuid();
                if(leaderGuid != ObjectManager.PlayerGuid)
                {
                    WowUnit leader = ObjectManager.WowObjects.OfType<WowUnit>().FirstOrDefault(t => t.Guid == leaderGuid);
                    HandleMovement(leader);
                }

            }
            return newTargetFound;
        }

        private void HandleAttacking(WowUnit target)
        {
            Spells.castNextSpell(distanceToTarget, target);
            if(target.IsDead)
            {
                Spells.resetAfterTargetDeath();
            }
            // back to attack
            else if (!ObjectManager.Player.IsAutoAttacking)
            {
                HookManager.StartAutoAttack();
            }
        }

        private void HandleMovement(WowUnit target)
        {
            double distanceTraveled = ObjectManager.Player.Position.GetDistance(LastPlayerPosition);
            double distanceToTarget = ObjectManager.Player.Position.GetDistance(LastTargetPosition);

            if (distanceToTarget > 3 && distanceTraveled < 0.001)
            {
                if(Jumped)
                {
                    List<Vector3> path = PathfindingHandler.GetPath(ObjectManager.MapId, ObjectManager.Player.Position, target.Position);
                    MovementEngine.LoadPath(path);
                    CharacterManager.Jump();
                }
                else
                {
                    CharacterManager.MoveToPosition(target.Position);
                    CharacterManager.Jump();
                    Jumped = true;
                }
            }
            else
            {
                Jumped = false;
                CharacterManager.MoveToPosition(target.Position);
            }
        }
    }
}