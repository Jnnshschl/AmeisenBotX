using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.Movement;
using AmeisenBotX.Logging;
using AmeisenBotX.Pathfinding;
using AmeisenBotX.Pathfinding.Objects;
using Newtonsoft.Json;
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
            Berserk = false;
        }

        public bool HandlesMovement => true;

        public bool HandlesTargetSelection => true;

        public bool IsMelee => true;

        public bool Jumped { get; set; }

        private CharacterManager CharacterManager { get; }

        private HookManager HookManager { get; }

        private Vector3 LastPosition { get; set; }

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
        private bool Berserk { get; set; }
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
                if (!ObjectManager.Player.IsAutoAttacking)
                {
                    HookManager.StartAutoAttack();
                }

                HandleMovement(target);
                HandleAttacking(target);
            }
        }

        public void OutOfCombatExecute()
        {
            double distanceTraveled = ObjectManager.Player.Position.GetDistance(LastPosition);
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
            bool gcdWaiting = IsGCD();
            double playerRage = ObjectManager.Player.Rage;
            double distanceToTarget = ObjectManager.Player.Position.GetDistance(target.Position);
            double targetHealthPercent = (target.Health / (double)target.MaxHealth) * 100.0;
            double playerHealthPercent = (ObjectManager.Player.Health / (double)ObjectManager.Player.MaxHealth) * 100.0;
            (string, int) targetCastingInfo = HookManager.GetUnitCastingInfo(WowLuaUnit.Target);
            List<string> Debuffs = HookManager.GetDebuffs(WowLuaUnit.Target.ToString());
            List<string> Buffs = HookManager.GetBuffs(WowLuaUnit.Player.ToString());
            bool stanceChanged = false;

            // special
            if (!gcdWaiting && Buffs.Any(e => e.Contains("slam")) && playerRage >= 15 && distanceToTarget < 4)
            {
                HookManager.CastSpell("Slam");
                playerRage -= 15;
                SetGCD(1.5);
                return;
            }

            // buffs
            if (!gcdWaiting && DateTime.Now.Subtract(LastRage).TotalSeconds > 20.1)
            {
                // alle 20.1 sec
                HookManager.CastSpell("Berserker Rage");
                LastRage = DateTime.Now;
                SetGCD(1.5);
                return;
            }
            if (!gcdWaiting && DateTime.Now.Subtract(LastReckless).TotalSeconds > 201)
            {
                // alle 3.35 min
                if(Berserk)
                {
                    HookManager.CastSpell("Recklessness");
                    LastReckless = DateTime.Now;
                    SetGCD(1.5);
                    return;
                }
                else
                {
                    if(!stanceChanged)
                    {
                        HookManager.CastSpell("Berserker Stance");
                        ObjectManager.UpdateObject(ObjectManager.Player.Type, ObjectManager.Player.BaseAddress);
                        playerRage = ObjectManager.Player.Rage;/*
                        HookManager.CastSpell("Recklessness");
                        LastReckless = DateTime.Now;*/
                        Berserk = true;
                        stanceChanged = true;
                    }
                }
            }
            if (!gcdWaiting && DateTime.Now.Subtract(LastShout).TotalSeconds > 120 && playerRage >= 10)
            {
                // alle 2 min
                HookManager.CastSpell("Battle Shout");
                LastShout = DateTime.Now;
                playerRage -= 10;
                SetGCD(1.5);
                return;
            }
            if (!gcdWaiting && DateTime.Now.Subtract(LastWish).TotalSeconds > 120.6 && playerRage >= 10)
            {
                // alle 2.01 min
                HookManager.CastSpell("Death Wish");
                LastWish = DateTime.Now;
                playerRage -= 10;
                SetGCD(1.5);
                return;
            }

            // distance attacks
            if (distanceToTarget > 10 && distanceToTarget < 30)
            {
                if (!gcdWaiting && ObjectManager.Player.IsInCombat)
                {
                    if (distanceToTarget < 25)
                    {
                        if (DateTime.Now.Subtract(LastIntercept).TotalSeconds > 30 && playerRage >= 10)
                        {
                            // alle 30 sec
                            if (Berserk)
                            {
                                HookManager.CastSpell("Intercept");
                                LastIntercept = DateTime.Now;
                                HookManager.SendChatMessage("/s gotcha!");
                                playerRage -= 10;
                                SetGCD(1.5);
                                return;
                            }
                            else
                            {
                                if (!stanceChanged)
                                {
                                    HookManager.CastSpell("Berserker Stance");
                                    ObjectManager.UpdateObject(ObjectManager.Player.Type, ObjectManager.Player.BaseAddress);
                                    playerRage = ObjectManager.Player.Rage;/*
                                    if(playerRage >= 10)
                                    {
                                        HookManager.CastSpell("Intercept");
                                        LastIntercept = DateTime.Now;
                                        HookManager.SendChatMessage("/s gotcha!");
                                        playerRage -= 10;
                                    }*/
                                    Berserk = true;
                                    stanceChanged = true;
                                }
                            }
                        }
                    }
                    else if (DateTime.Now.Subtract(LastShatter).TotalSeconds > 300 && playerRage >= 25)
                    {
                        // alle 5 min
                        if (Berserk)
                        {
                            if (!stanceChanged)
                            {
                                HookManager.CastSpell("Battle Stance");
                                ObjectManager.UpdateObject(ObjectManager.Player.Type, ObjectManager.Player.BaseAddress);
                                playerRage = ObjectManager.Player.Rage;/*
                                if (playerRage >= 25)
                                {
                                    HookManager.CastSpell("Shattering Throw");
                                    LastShatter = DateTime.Now;
                                    HookManager.SendChatMessage("/s and i'm like.. bam!");
                                    playerRage -= 25;
                                }*/
                                Berserk = false;
                                stanceChanged = true;
                            }
                        }
                        else
                        {
                            HookManager.CastSpell("Shattering Throw");
                            LastShatter = DateTime.Now;
                            HookManager.SendChatMessage("/s and i'm like.. bam!");
                            playerRage -= 25;
                            SetGCD(1.5);
                            return;
                        }
                    }
                    else if (DateTime.Now.Subtract(LastHero).TotalSeconds > 60)
                    {
                        // alle 1 min
                        HookManager.CastSpell("Heroic Throw");
                        LastHero = DateTime.Now;
                        HookManager.SendChatMessage("/s drive by shootin, baby!");
                        SetGCD(1.5);
                        return;
                    }
                }
                else
                {
                    // not in combat
                    if(distanceToTarget < 25 && !ObjectManager.Player.IsInCombat)
                    {
                        if(DateTime.Now.Subtract(LastCharge).TotalSeconds > 15)
                        {
                            // alle 15 sec
                            if (Berserk)
                            {
                                if (!stanceChanged)
                                {
                                    HookManager.CastSpell("Battle Stance");
                                    ObjectManager.UpdateObject(ObjectManager.Player.Type, ObjectManager.Player.BaseAddress);
                                    playerRage = ObjectManager.Player.Rage;/*
                                    HookManager.CastSpell("Charge");
                                    LastCharge = DateTime.Now;
                                    HookManager.SendChatMessage("/s boo!");*/
                                    Berserk = false;
                                    stanceChanged = true;
                                }
                            }
                            else
                            {
                                HookManager.CastSpell("Charge");
                                LastCharge = DateTime.Now;
                                HookManager.SendChatMessage("/s boo!");
                            }
                        }
                    }
                }
            }
            else
            {
                // close combat
                if (!stanceChanged)
                {
                    HookManager.CastSpell("Berserker Stance");
                    ObjectManager.UpdateObject(ObjectManager.Player.Type, ObjectManager.Player.BaseAddress);
                    playerRage = ObjectManager.Player.Rage;/*
                                    if(playerRage >= 10)
                                    {
                                        HookManager.CastSpell("Intercept");
                                        LastIntercept = DateTime.Now;
                                        HookManager.SendChatMessage("/s gotcha!");
                                        playerRage -= 10;
                                    }*/
                    Berserk = true;
                    stanceChanged = true;
                }
                // debuffs
                if (!gcdWaiting && playerHealthPercent < 21 && DateTime.Now.Subtract(LastThirst).TotalSeconds > 8 && distanceToTarget < 4)
                {
                    HookManager.SendChatMessage("/healme");
                    if (playerRage >= 20)
                    {
                        // alle 8 sec
                        HookManager.CastSpell("Bloodthirst");
                        LastThirst = DateTime.Now;
                        HookManager.SendChatMessage("/s oooh shit");
                        playerRage -= 20;
                        SetGCD(1.5);
                        return;
                    }
                }
                else if (!gcdWaiting && DateTime.Now.Subtract(LastHamstring).TotalSeconds > 15 && playerRage >= 10 && distanceToTarget < 4)
                {
                    // alle 15 sec
                    HookManager.CastSpell("Hamstring");
                    LastHamstring = DateTime.Now;
                    playerRage -= 10;
                    SetGCD(1.5);
                    return;
                } // attacks
                else if(!gcdWaiting && targetHealthPercent < 21)
                {
                    if (playerRage >= 10)
                    {
                        HookManager.CastSpell("Execute");
                        playerRage -= 10;
                        SetGCD(1.5);
                        return;
                    }
                }
                else
                {
                    if (playerRage >= 12)
                    {
                        HookManager.CastSpell("Heroic Strike");
                        playerRage -= 12;
                    }
                }
            }

            // back to attack
            if (!ObjectManager.Player.IsAutoAttacking)
            {
                HookManager.StartAutoAttack();
            }

        }

        private void HandleMovement(WowUnit target)
        {
            double distanceTraveled = ObjectManager.Player.Position.GetDistance(LastPosition);
            double distanceToTarget = ObjectManager.Player.Position.GetDistance(target.Position);

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

            LastPosition = ObjectManager.Player.Position;
        }
    }
}