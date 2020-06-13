using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.Movement;
using AmeisenBotX.Core.Movement.Pathfinding;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Core.Statemachine.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Statemachine.CombatClasses
{
    public class WarriorArms : ICombatClass
    {
        private readonly WarriorArmSpells spells;

        private bool computeNewRoute = false;
        private double distanceToTarget = 0;
        private double distanceTraveled = 0;
        private bool hasTargetMoved = false;
        private bool multipleTargets = false;

        public WarriorArms(IObjectManager objectManager, ICharacterManager characterManager, IHookManager hookManager, IPathfindingHandler pathhandler, DefaultMovementEngine movement)
        {
            ObjectManager = objectManager;
            CharacterManager = characterManager;
            HookManager = hookManager;
            PathfindingHandler = pathhandler;
            MovementEngine = movement;
            spells = new WarriorArmSpells(hookManager, objectManager);
        }

        public string Author => "einTyp";

        public WowClass Class => WowClass.Warrior;

        public Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public string Description => "...";

        public string Displayname => "Arms Warrior";

        public bool HandlesMovement => true;

        public bool HandlesTargetSelection => true;

        public bool IsMelee => true;

        public IWowItemComparator ItemComparator => new ArmsAxeItemComparator();

        public List<string> PriorityTargets { get; set; }

        public CombatClassRole Role => CombatClassRole.Dps;

        public string Version => "1.0";

        public bool WalkBehindEnemy => false;

        private ICharacterManager CharacterManager { get; }

        private bool Dancing { get; set; }

        private IHookManager HookManager { get; }

        private Vector3 LastPlayerPosition { get; set; }

        private Vector3 LastTargetPosition { get; set; }

        private DefaultMovementEngine MovementEngine { get; set; }

        private IObjectManager ObjectManager { get; }

        private IPathfindingHandler PathfindingHandler { get; set; }

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
                else if (hasTargetMoved)
                {
                    hasTargetMoved = false;
                    computeNewRoute = true;
                }

                if (targetDistanceChanged)
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
                ulong leaderGuid = ObjectManager.PartyleaderGuid;
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

        private void HandleAttacking(WowUnit target)
        {
            spells.CastNextSpell(distanceToTarget, target, multipleTargets);
            if (target.IsDead)
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

            if (hasTargetMoved || (distanceToTarget < target.CombatReach && !BotMath.IsFacing(LastPlayerPosition, ObjectManager.Player.Rotation, LastTargetPosition, 0.75, 1.25)))
            {
                CharacterManager.MoveToPosition(LastTargetPosition);
            }
            else if (distanceToTarget >= target.CombatReach)
            {
                if (computeNewRoute || MovementEngine.CurrentPath?.Count == 0)
                {
                    List<Vector3> path = PathfindingHandler.GetPath((int)ObjectManager.MapId, LastPlayerPosition, LastTargetPosition);
                    MovementEngine.LoadPath(path);
                    MovementEngine.PostProcessPath();
                }
                else
                {
                    if (MovementEngine.GetNextStep(LastPlayerPosition, out Vector3 positionToGoTo, out bool needToJump))
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

        private bool SearchNewTarget(ref WowUnit target, bool grinding)
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
                    double tmpDistance = ObjectManager.Player.Position.GetDistance(unit.Position);
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

            private readonly Dictionary<string, DateTime> nextActionTime = new Dictionary<string, DateTime>()
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

            public WarriorArmSpells(IHookManager hookManager, IObjectManager objectManager)
            {
                HookManager = hookManager;
                ObjectManager = objectManager;
                Player = ObjectManager.Player;
                IsInBerserkerStance = false;
                NextGCDSpell = DateTime.Now;
                NextStance = DateTime.Now;
                NextCast = DateTime.Now;
            }

            private IHookManager HookManager { get; set; }

            private bool IsInBerserkerStance { get; set; }

            private DateTime NextCast { get; set; }

            private DateTime NextGCDSpell { get; set; }

            private DateTime NextStance { get; set; }

            private IObjectManager ObjectManager { get; set; }

            private WowPlayer Player { get; set; }

            public void CastNextSpell(double distanceToTarget, WowUnit target, bool multipleTargets)
            {
                if (!IsReady(NextCast))
                {
                    return;
                }

                if (!ObjectManager.Player.IsAutoAttacking)
                {
                    HookManager.StartAutoAttack(ObjectManager.Target);
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
                        else if (distanceToTarget < target.CombatReach)
                        {
                            // -- close combat --
                            // Battle Stance
                            if (IsInBerserkerStance && IsReady(NextStance))
                            {
                                ChangeToStance(BattleStance, out rage);
                            }
                            else if (isGCDReady)
                            {
                                List<string> buffs = HookManager.GetBuffs(WowLuaUnit.Player);
                                if (buffs.Any(e => e.Contains("slam")) && rage > 15)
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
                                if (!ObjectManager.Player.IsAutoAttacking)
                                {
                                    HookManager.StartAutoAttack(ObjectManager.Target);
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
                nextActionTime[Hamstring] = DateTime.Now;
                nextActionTime[Rend] = DateTime.Now;
            }

            private void CastSpell(string spell, ref int rage, int rageCosts, double cooldown, bool gcd)
            {
                HookManager.CastSpell(spell);
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
                HookManager.CastSpell(stance);
                rage = UpdateRage();
                NextStance = DateTime.Now.AddSeconds(1);
                IsInBerserkerStance = stance == BerserkerStance;
            }

            private bool IsReady(DateTime nextAction)
            {
                return DateTime.Now > nextAction;
            }

            private bool IsReady(string spell)
            {
                return !nextActionTime.TryGetValue(spell, out DateTime NextSpellAvailable) || IsReady(NextSpellAvailable);
            }

            private int UpdateRage()
            {
                ObjectManager.UpdateObject(ObjectManager.Player.Type, ObjectManager.Player.BaseAddress);
                Player = ObjectManager.Player;
                return Player.Rage;
            }
        }
    }
}