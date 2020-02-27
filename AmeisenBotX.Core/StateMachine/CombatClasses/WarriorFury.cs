using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.Movement;
using AmeisenBotX.Core.StateMachine.Enums;
using AmeisenBotX.Pathfinding;
using AmeisenBotX.Pathfinding.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.StateMachine.CombatClasses
{
    public class WarriorFury : ICombatClass
    {
        private readonly string[] runningEmotes = { "/train", "/fart", "/burp", "/moo", "/lost", "/puzzled", "/cackle", "/silly", "/question", "/talk" };
        private readonly WarriorFurySpells spells;
        private readonly string[] standingEmotes = { "/chug", "/pick", "/whistle", "/shimmy", "/dance", "/twiddle", "/bored", "/violin", "/highfive", "/bow" };

        private bool computeNewRoute = false;
        private double distanceToTarget = 0;

        private double distanceTraveled = 0;

        private bool hasTargetMoved = false;
        private bool multipleTargets = false;
        private bool standing = false;

        public WarriorFury(ObjectManager objectManager, CharacterManager characterManager, HookManager hookManager, IPathfindingHandler pathhandler, DefaultMovementEngine movement)
        {
            ObjectManager = objectManager;
            CharacterManager = characterManager;
            HookManager = hookManager;
            PathfindingHandler = pathhandler;
            MovementEngine = movement;
            spells = new WarriorFurySpells(hookManager, objectManager);
        }

        public string Author => "einTyp";

        public WowClass Class => WowClass.Warrior;

        public Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public string Description => "...";

        public string Displayname => "Fury Warrior";

        public bool HandlesMovement => true;

        public bool HandlesTargetSelection => true;

        public bool IsMelee => true;

        public IWowItemComparator ItemComparator => new FurySwordItemComparator();

        public CombatClassRole Role => CombatClassRole.Dps;

        public string Version => "1.0";

        private CharacterManager CharacterManager { get; }

        private bool Dancing { get; set; }

        private HookManager HookManager { get; }

        private Vector3 LastPlayerPosition { get; set; }

        private Vector3 LastTargetPosition { get; set; }

        private DefaultMovementEngine MovementEngine { get; set; }

        private ObjectManager ObjectManager { get; }

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
                Dancing = false;
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
            else if (!Dancing)
            {
                if (distanceTraveled < 0.001)
                {
                    HookManager.ClearTarget();
                    HookManager.SendChatMessage(standingEmotes[new Random().Next(standingEmotes.Length)]);
                    Dancing = true;
                }
                else
                {
                    HookManager.ClearTarget();
                    HookManager.SendChatMessage(runningEmotes[new Random().Next(runningEmotes.Length)]);
                    Dancing = true;
                }
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

                    Dancing = false;
                    HandleMovement(target);
                    HandleAttacking(target);
                }
                else if (!Dancing || standing)
                {
                    standing = false;
                    HookManager.ClearTarget();
                    HookManager.SendChatMessage(standingEmotes[new Random().Next(standingEmotes.Length)]);
                    Dancing = true;
                }
            }
            else
            {
                if (!Dancing || !standing)
                {
                    standing = true;
                    HookManager.ClearTarget();
                    HookManager.SendChatMessage(runningEmotes[new Random().Next(runningEmotes.Length)]);
                    Dancing = true;
                }
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
                spells.ResetAfterTargetDeath();
            }

            return newTargetFound;
        }

        private class WarriorFurySpells
        {
            private static readonly string BattleShout = "Battle Shout";
            private static readonly string BattleStance = "Battle Stance";
            private static readonly string BerserkerRage = "Berserker Rage";
            private static readonly string BerserkerStance = "Berserker Stance";
            private static readonly string Bloodthirst = "Bloodthirst";
            private static readonly string Charge = "Charge";
            private static readonly string DeathWish = "Death Wish";
            private static readonly string EnragedRegeneration = "Enraged Regeneration";
            private static readonly string Execute = "Execute";
            private static readonly string Hamstring = "Hamstring";
            private static readonly string HeroicStrike = "Heroic Strike";
            private static readonly string HeroicThrow = "Heroic Throw";
            private static readonly string Intercept = "Intercept";
            private static readonly string Recklessness = "Recklessness";
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
                { HeroicStrike, DateTime.Now }
            };

            private bool askedForHeal = false;
            private bool askedForHelp = false;

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

            private HookManager HookManager { get; set; }

            private bool IsInBerserkerStance { get; set; }

            private DateTime NextCast { get; set; }

            private DateTime NextGCDSpell { get; set; }

            private DateTime NextStance { get; set; }

            private ObjectManager ObjectManager { get; set; }

            private WowPlayer Player { get; set; }

            public void CastNextSpell(double distanceToTarget, WowUnit target, bool multipleTargets)
            {
                if (!IsReady(NextCast) || !IsReady(NextGCDSpell))
                {
                    return;
                }

                if (!ObjectManager.Player.IsAutoAttacking)
                {
                    HookManager.StartAutoAttack();
                }

                Player = ObjectManager.Player;
                int rage = Player.Rage;
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

                if (lowHealth && rage > 15 && IsReady(EnragedRegeneration))
                {
                    HookManager.SendChatMessage("/s Oh shit");
                    CastSpell(EnragedRegeneration, ref rage, 15, 180, false);
                }

                if (!lowHealth && rage > 10 && IsReady(DeathWish))
                {
                    CastSpell(DeathWish, ref rage, 10, 120.6, true); // lasts 30 sec
                }
                else if (rage > 10 && IsReady(BattleShout))
                {
                    HookManager.SendChatMessage("/roar");
                    CastSpell(BattleShout, ref rage, 10, 120, true); // lasts 2 min
                }
                else if (IsInBerserkerStance && rage > 10 && Player.HealthPercentage > 50 && IsReady(Recklessness))
                {
                    CastSpell(Recklessness, ref rage, 0, 201, true); // lasts 12 sec
                }
                else if (Player.Health < Player.MaxHealth && IsReady(BerserkerRage))
                {
                    CastSpell(BerserkerRage, ref rage, 0, 20.1, true); // lasts 10 sec
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
                                        if (IsReady(Retaliation))
                                        {
                                            CastSpell(Retaliation, ref rage, 0, 300, false);
                                        }

                                        ChangeToStance(BerserkerStance, out rage);
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
                                        HookManager.SendChatMessage("/incoming");
                                        CastSpell(Charge, ref rage, 0, 15, false);
                                    }
                                }
                            }
                        }
                        else if (distanceToTarget < target.CombatReach)
                        {
                            // -- close combat --
                            // Berserker Stance
                            if (!IsInBerserkerStance && IsReady(NextStance))
                            {
                                if (IsReady(Retaliation))
                                {
                                    CastSpell(Retaliation, ref rage, 0, 300, false);
                                }

                                ChangeToStance(BerserkerStance, out rage);
                            }
                            else if (mediumHealth && rage > 20 && IsReady(Bloodthirst))
                            {
                                CastSpell(Bloodthirst, ref rage, 20, 4, true);
                            }
                            else
                            {
                                List<string> buffs = HookManager.GetBuffs(WowLuaUnit.Player);
                                if (buffs.Any(e => e.Contains("slam")) && rage > 15)
                                {
                                    CastSpell(Slam, ref rage, 15, 0, false);
                                    NextCast = DateTime.Now.AddSeconds(1.5); // casting time
                                    NextGCDSpell = DateTime.Now.AddSeconds(3.0); // 1.5 sec gcd after the 1.5 sec casting time
                                }
                                else if (rage > 10 && IsReady(Hamstring))
                                {
                                    CastSpell(Hamstring, ref rage, 10, 15, true);
                                }
                                else if (target.HealthPercentage <= 20 && rage > 10)
                                {
                                    CastSpell(Execute, ref rage, 10, 0, true);
                                }
                                else if (((multipleTargets && rage > 25) || rage > 50) && IsReady(Whirlwind))
                                {
                                    CastSpell(Whirlwind, ref rage, 25, 10, true);
                                }
                                else if (rage > 12 && IsReady(HeroicStrike))
                                {
                                    CastSpell(HeroicStrike, ref rage, 12, 3.6, false);
                                }
                                else if (!ObjectManager.Player.IsAutoAttacking)
                                {
                                    HookManager.StartAutoAttack();
                                }
                            }
                        }
                    }
                    else
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

            public void ResetAfterTargetDeath()
            {
                nextActionTime[Hamstring] = DateTime.Now;
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
                bool result = true; // begin with neutral element of AND
                if (spell.Equals(Hamstring) || spell.Equals(BattleShout))
                {
                    // only use these spells in a certain interval
                    result &= !nextActionTime.TryGetValue(spell, out DateTime NextSpellAvailable) || IsReady(NextSpellAvailable);
                }

                result &= HookManager.GetSpellCooldown(spell) <= 0 && HookManager.GetUnitCastingInfo(WowLuaUnit.Player).Item2 <= 0;
                return result;
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