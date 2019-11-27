using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Character.Comparators;
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
    public class WarriorArms : ICombatClass
    {
        public WarriorArms(ObjectManager objectManager, CharacterManager characterManager, HookManager hookManager, IPathfindingHandler pathhandler, DefaultMovementEngine movement)
        {
            ObjectManager = objectManager;
            CharacterManager = characterManager;
            HookManager = hookManager;
            PathfindingHandler = pathhandler;
            MovementEngine = movement;
            Spells = new WarriorArmSpells(hookManager, objectManager);
        }

        public bool HandlesMovement => true;

        public bool HandlesTargetSelection => true;

        public bool IsMelee => true;

        public IWowItemComparator ItemComparator => new ArmsAxeItemComparator();

        private bool hasTargetMoved = false;
        private bool computeNewRoute = false;
        public bool multipleTargets = false;
        private readonly WarriorArmSpells Spells;

        private class WarriorArmSpells
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
            static readonly string retaliation = "Retaliation";
            static readonly string enragedRegeneration = "Enraged Regeneration";
            static readonly string whirlwind = "Whirlwind";
            static readonly string intimidatingShout = "Intimidating Shout";
            static readonly string bloodrage = "Bloodrage";
            static readonly string bladestorm = "Bladestorm";
            static readonly string mortalStrike = "Mortal Strike";
            static readonly string rend = "Rend";
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
                { whirlwind, DateTime.Now },
                { retaliation, DateTime.Now },
                { enragedRegeneration, DateTime.Now },
                { intimidatingShout, DateTime.Now },
                { bloodrage, DateTime.Now },
                { bladestorm, DateTime.Now },
                { rend, DateTime.Now },
                { mortalStrike, DateTime.Now },
                { heroicStrike, DateTime.Now }
            };
            private DateTime NextGCDSpell { get; set; }
            private DateTime NextStance { get; set; }
            private DateTime NextCast { get; set; }
            private bool IsInBerserkerStance { get; set; }
            private bool askedForHelp = false;
            private bool askedForHeal = false;
            public WarriorArmSpells(HookManager hookManager, ObjectManager objectManager)
            {
                HookManager = hookManager;
                ObjectManager = objectManager;
                Player = ObjectManager.Player;
                IsInBerserkerStance = false;
                NextGCDSpell = DateTime.Now;
                NextStance = DateTime.Now;
                NextCast = DateTime.Now;
            }
            public void CastNextSpell(double distanceToTarget, WowUnit target, bool multipleTargets)
            {
                if (!IsReady(NextCast))
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
                if (!(lowHealth || mediumHealth))
                {
                    askedForHelp = false;
                    askedForHeal = false;
                }
                else if (lowHealth && !askedForHelp)
                {
                    HookManager.SendChatMessage("/helpme");
                    askedForHelp = true;
                }
                else if (mediumHealth && !askedForHeal)
                {
                    HookManager.SendChatMessage("/healme");
                    askedForHeal = true;
                }
                // -- buffs --
                if (lowHealth && rage > 15 && IsReady(enragedRegeneration))
                {
                    if (IsReady(enragedRegeneration))
                    {
                        CastSpell(enragedRegeneration, ref rage, 15, 180, false);
                    }
                }
                else if(rage < 20 && !lowHealth && !mediumHealth && IsReady(bloodrage))
                {
                    CastSpell(bloodrage, ref rage, 0, 40.2, false);
                }
                if (isGCDReady)
                {
                    // Berserker Rage
                    if (Player.Health < Player.MaxHealth && IsReady(berserkerRage))
                    {
                        CastSpell(berserkerRage, ref rage, 0, 20.1, true); // lasts 10 sec
                    }
                }
                if(multipleTargets)
                {
                    if (rage > 25 && IsReady(intimidatingShout))
                    {
                        CastSpell(intimidatingShout, ref rage, 25, 120, false);
                    }
                    else if (IsReady(retaliation))
                    {
                        CastSpell(retaliation, ref rage, 0, 300, false);
                    }
                }
                if (distanceToTarget < 29)
                {
                    if (distanceToTarget < 24)
                    {
                        if (distanceToTarget > 9)
                        {
                            // -- run to the target! --
                            if (Player.IsInCombat)
                            {
                                if (isGCDReady)
                                {
                                    if (IsInBerserkerStance)
                                    {
                                        // intercept
                                        if (rage > 10 && IsReady(intercept))
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
                        else if (distanceToTarget < 6)
                        {
                            // -- close combat --
                            // Battle Stance
                            if (IsInBerserkerStance && IsReady(NextStance))
                            {
                                ChangeToStance(battleStance, out rage);
                            }
                            else if (isGCDReady)
                            {
                                List<string> Buffs = HookManager.GetBuffs(WowLuaUnit.Player);
                                if (Buffs.Any(e => e.Contains("slam")) && rage > 15)
                                {
                                    CastSpell(slam, ref rage, 15, 0, false);
                                    NextCast = DateTime.Now.AddSeconds(1.5); // casting time
                                    NextGCDSpell = DateTime.Now.AddSeconds(3.0); // 1.5 sec gcd after the 1.5 sec casting time
                                }
                                else if (rage > 10 && IsReady(rend))
                                {
                                    CastSpell(rend, ref rage, 10, 15, true);
                                }
                                // hamstring
                                else if (rage > 10 && IsReady(hamstring))
                                {
                                    CastSpell(hamstring, ref rage, 10, 15, true);
                                }
                                // execute
                                else if (target.HealthPercentage <= 20 && rage > 10)
                                {
                                    CastSpell(execute, ref rage, 10, 0, true);
                                }
                                // bladestorm
                                else if (multipleTargets && rage > 25 && IsReady(bladestorm))
                                {
                                    CastSpell(bladestorm, ref rage, 25, 90, true);
                                }
                                // mortal strike
                                else if (rage > 30 && IsReady(mortalStrike))
                                {
                                    CastSpell(mortalStrike, ref rage, 30, 6, true);
                                }
                                // heroic strike
                                else if (rage > 12 && IsReady(heroicStrike))
                                {
                                    CastSpell(heroicStrike, ref rage, 12, 3.6, false);
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
                        if (isGCDReady)
                        {
                            // -- distant attacks --
                            if (isGCDReady)
                            {
                                // shattering throw (in Battle Stance)
                                if (rage > 25 && IsReady(shatteringThrow))
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
                NextActionTime[rend] = DateTime.Now;
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
                if (cooldown > 0)
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
            /*Character.Inventory.Objects.IWowItem weapon;
            if(CharacterManager.Equipment.Equipment.TryGetValue(Character.Inventory.Enums.EquipmentSlot.INVSLOT_MAINHAND, out weapon))
            {
                if(mainhandSpeed != 1 && weapon != null && weapon.Stats != null && weapon.Stats.Keys != null)
                {
                    foreach (string stat in weapon.Stats.Keys)
                    {
                        Console.WriteLine(stat);
                        mainhandSpeed = 1;
                    }
                }
                //mainhandSpeed = weapon.Stats["ITEM_MOD_SPEED_SHORT"];
            }*/
            WowUnit target = ObjectManager.WowObjects.OfType<WowUnit>().FirstOrDefault(t => t.Guid == targetGuid);
            SearchNewTarget(ref target, false);
            if (target != null)
            {
                bool targetDistanceChanged = false;
                if (!LastPlayerPosition.Equals(ObjectManager.Player.Position))
                {
                    distanceTraveled = ObjectManager.Player.Position.GetDistance2D(LastPlayerPosition);
                    LastPlayerPosition = new Vector3(ObjectManager.Player.Position.X, ObjectManager.Player.Position.Y, ObjectManager.Player.Position.Z);
                    targetDistanceChanged = true;
                }
                if (!LastTargetPosition.Equals(target.Position))
                {
                    hasTargetMoved = true;
                    LastTargetPosition = new Vector3(target.Position.X, target.Position.Y, target.Position.Z);
                    targetDistanceChanged = true;
                }
                else if (hasTargetMoved)
                {
                    hasTargetMoved = false;
                    computeNewRoute = true;
                }
                if (targetDistanceChanged)
                {
                    distanceToTarget = LastPlayerPosition.GetDistance2D(LastTargetPosition);
                }
                HandleMovement(target);
                HandleAttacking(target);
            }
        }

        public void OutOfCombatExecute()
        {
            if (!LastPlayerPosition.Equals(ObjectManager.Player.Position))
            {
                distanceTraveled = ObjectManager.Player.Position.GetDistance2D(LastPlayerPosition);
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
                        distanceToTarget = LastPlayerPosition.GetDistance2D(LastTargetPosition);
                    }
                    else
                    {
                        computeNewRoute = true;
                        hasTargetMoved = false;
                    }
                    HandleMovement(target);
                    HandleAttacking(target);
                }
                else if (!Dancing)
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

        private bool SearchNewTarget(ref WowUnit? target, bool grinding)
        {
            if (target != null && !(target.IsDead || target.Health == 0))
            {
                return false;
            }
            List<WowUnit> wowUnits = ObjectManager.WowObjects.OfType<WowUnit>().Where(e => HookManager.GetUnitReaction(ObjectManager.Player, e) != WowUnitReaction.Friendly && HookManager.GetUnitReaction(ObjectManager.Player, e) != WowUnitReaction.Neutral).ToList();
            bool newTargetFound = false;
            int targetHealth = (target == null || target.IsDead) ? 2147483647 : target.Health;
            bool inCombat = target == null ? false : target.IsInCombat;
            int targetCount = 0;
            multipleTargets = false;
            foreach (WowUnit unit in wowUnits)
            {
                if (BotUtils.IsValidUnit(unit) && unit != target && !unit.IsDead)
                {
                    double tmpDistance = ObjectManager.Player.Position.GetDistance2D(unit.Position);
                    if (tmpDistance < 100.0)
                    {
                        if (tmpDistance < 6.0)
                        {
                            targetCount++;
                        }
                        if ((unit.IsInCombat && unit.Health < targetHealth) || (!inCombat && grinding && unit.Health < targetHealth))
                        {
                            target = unit;
                            targetHealth = unit.Health;
                            newTargetFound = true;
                            inCombat = unit.IsInCombat;
                        }
                    }
                }
            }
            if (target == null || target.IsDead)
            {
                HookManager.ClearTarget();
                newTargetFound = false;
                target = null;
            }
            else if (targetCount > 1)
            {
                multipleTargets = true;
            }
            if (newTargetFound)
            {
                HookManager.TargetGuid(target.Guid);
            }
            return newTargetFound;
        }

        private void HandleAttacking(WowUnit target)
        {
            Spells.CastNextSpell(distanceToTarget, target, multipleTargets);
            if (target.IsDead)
            {
                Spells.ResetAfterTargetDeath();
            }
        }

        private void HandleMovement(WowUnit target)
        {
            if (target == null)
            {
                return;
            }

            if (hasTargetMoved || (distanceToTarget < 6.0 && !BotMath.IsFacing(LastPlayerPosition, ObjectManager.Player.Rotation, LastTargetPosition, 0.75, 1.25)))
            {
                CharacterManager.MoveToPosition(LastTargetPosition);
            }
            else if (distanceToTarget >= 6.0)
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