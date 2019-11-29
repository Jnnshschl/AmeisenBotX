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
    public class RogueAssassination2 : ICombatClass
    {
        public RogueAssassination2(ObjectManager objectManager, CharacterManager characterManager, HookManager hookManager, IPathfindingHandler pathhandler, DefaultMovementEngine movement)
        {
            ObjectManager = objectManager;
            CharacterManager = characterManager;
            HookManager = hookManager;
            PathfindingHandler = pathhandler;
            MovementEngine = movement;
            Spells = new RogueAssassinSpells(hookManager, objectManager);
        }

        public bool HandlesMovement => true;

        public bool HandlesTargetSelection => true;

        public bool IsMelee => true;

        public IWowItemComparator ItemComparator => new AssassinationItemComparator();

        private bool hasTargetMoved = false;
        private bool computeNewRoute = false;
        private bool wasInStealth = false;
        private bool isSneaky = false;
        private bool multipleTargets = false;
        private readonly RogueAssassinSpells Spells;

        private class RogueAssassinSpells
        {
            static readonly string garrote = "Garrote"; // 50 energy, stealthed, behind, 1 combo, bleed
            static readonly string ambush = "Ambush"; // 60 energy, stealthed, behind, 2 combo
            static readonly string hungerForBlood = "Hunger For Blood"; // 15 energy, bleed, last 1 min, 30 range
            static readonly string sliceAndDice = "Slice and Dice"; // 25 energy needs 5 combo
            static readonly string mutilate = "Mutilate"; // 60 energy, poisoned, 2 combo
            static readonly string envenom = "Envenom"; // 35 energy
            static readonly string vanish = "Vanish"; // 
            static readonly string overkill = "Overkill"; // 
            static readonly string coldBlood = "Cold Blood"; // 
            static readonly string stealth = "Stealth"; // 
            static readonly string sprint = "Sprint"; // 
            static readonly string sinisterStrike = "Sinister Strike"; // 
            static readonly string rupture = "Rupture"; // 
            static readonly string eviscerate = "Eviscerate"; // 
            static readonly string deadlyThrow = "Deadly Throw"; // 
            static readonly string throwAttack = "Throw"; // 
            static readonly string kick = "Kick"; // 
            private HookManager HookManager { get; set; }
            private ObjectManager ObjectManager { get; set; }
            private WowPlayer Player { get; set; }
            private readonly Dictionary<string, DateTime> NextActionTime = new Dictionary<string, DateTime>()
            {
                { garrote, DateTime.Now },
                { ambush, DateTime.Now },
                { hungerForBlood, DateTime.Now },
                { sliceAndDice, DateTime.Now },
                { mutilate, DateTime.Now },
                { envenom, DateTime.Now },
                { vanish, DateTime.Now },
                { overkill, DateTime.Now },
                { coldBlood, DateTime.Now },
                { stealth, DateTime.Now },
                { sprint, DateTime.Now },
                { sinisterStrike, DateTime.Now },
                { rupture, DateTime.Now },
                { eviscerate, DateTime.Now },
                { deadlyThrow, DateTime.Now },
                { kick, DateTime.Now }
            };
            private DateTime NextGCDSpell { get; set; }
            private DateTime NextStance { get; set; }
            private DateTime NextCast { get; set; }
            private bool IsInStealth()
            {
                return HookManager.GetBuffs(WowLuaUnit.Player).Any(e => e.Contains("tealth"));
            }
            private bool isTargetBleeding()
            {
                return HookManager.GetDebuffs(WowLuaUnit.Target).Any(e => e.Contains("acerate") || e.Contains("Bleed") || e.Contains("bleed") || e.Contains("Rip") || e.Contains("rip")
                 || e.Contains("Rake") || e.Contains("rake") || e.Contains("iercing") || e.Contains("arrote") || e.Contains("emorrhage") || e.Contains("upture") || e.Contains("Wounds") || e.Contains("wounds"));
            }
            private bool IsTargetPoisoned()
            {
                return HookManager.GetDebuffs(WowLuaUnit.Target).Any(e => e.Contains("Poison") || e.Contains("poison"));
            }
            private bool askedForHelp = false;
            private bool askedForHeal = false;
            private int comboCnt = 0;
            public RogueAssassinSpells(HookManager hookManager, ObjectManager objectManager)
            {
                HookManager = hookManager;
                ObjectManager = objectManager;
                Player = ObjectManager.Player;
                NextGCDSpell = DateTime.Now;
                NextStance = DateTime.Now;
                NextCast = DateTime.Now;
            }
            public void CastNextSpell(double distanceToTarget, WowUnit target, bool multipleTargets)
            {
                if (!IsReady(NextCast) || !IsReady(NextGCDSpell))
                {
                    return;
                }
                if (!ObjectManager.Player.IsAutoAttacking && !IsInStealth())
                {
                    HookManager.StartAutoAttack();
                }
                Player = ObjectManager.Player;
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
                    HookManager.SendChatMessage("/helpme");
                    askedForHelp = true;
                }
                else if (mediumHealth && !askedForHeal)
                {
                    HookManager.SendChatMessage("/healme");
                    askedForHeal = true;
                }
                // -- stealth --
                if(!IsInStealth())
                {
                    if(!Player.IsInCombat)
                    {
                        CastSpell(stealth, ref energy, 0, 1, false);
                    }
                    else if (lowHealth)
                    {
                        if (IsReady(vanish))
                        {
                            CastSpell(vanish, ref energy, 0, 180, false);
                            HookManager.ClearTarget();
                            return;
                        }
                    }
                }

                // combat
                if (distanceToTarget < 29)
                {
                    // in range
                    if(energy > 15 && IsReady(hungerForBlood) && isTargetBleeding() && !IsInStealth())
                    {
                        CastSpell(hungerForBlood, ref energy, 15, 60, true);
                    }
                    if (distanceToTarget < 24)
                    {
                        if (distanceToTarget > 9)
                        {
                            // 9 < distance < 24
                            // run?
                            if (energy > 15 && IsReady(sprint) && isTargetBleeding())
                            {
                                CastSpell(sprint, ref energy, 15, 180, true);
                            }
                        }
                        else if (distanceToTarget < 6)
                        {
                            // distance <= 9
                            // close combat
                            if(IsInStealth())
                            {
                                if(energy > 50 && IsReady(garrote) && !isTargetBleeding())
                                {
                                    CastSpell(garrote, ref energy, 50, 3, true);
                                    comboCnt++;
                                }
                                else if (energy > 60)
                                {
                                    CastSpell(ambush, ref energy, 60, 0, true);
                                    comboCnt += 2;
                                }
                            }
                            else
                            {
                                if(HookManager.GetUnitCastingInfo(WowLuaUnit.Target).Item2 > 0 && energy > 25 && IsReady(kick))
                                {
                                    CastSpell(kick, ref energy, 25, 10, true);
                                }
                                else if (comboCnt > 4 && energy > 35 && IsTargetPoisoned())
                                {
                                    if(IsReady(coldBlood))
                                    {
                                        CastSpell(coldBlood, ref energy, 0, 180, false);
                                    }
                                    CastSpell(envenom, ref energy, 35, 0, true);
                                    comboCnt -= 5;
                                }
                                else if (comboCnt > 4 && energy > 35)
                                {
                                    if (IsReady(coldBlood))
                                    {
                                        CastSpell(coldBlood, ref energy, 0, 180, false);
                                    }
                                    CastSpell(eviscerate, ref energy, 35, 0, true);
                                    comboCnt -= 5;
                                }
                                else if (comboCnt > 0 && energy > 25 && IsReady(sliceAndDice))
                                {
                                    int comboCntUsed = Math.Min(5, comboCnt);
                                    CastSpell(sliceAndDice, ref energy, 25, 6 + (3 * comboCntUsed), true);
                                    comboCnt -= comboCntUsed;
                                }
                                else if (energy > 60 && IsTargetPoisoned())
                                {
                                    CastSpell(mutilate, ref energy, 60, 0, true);
                                    comboCnt += 2;
                                }
                                else if (energy > 45)
                                {
                                    CastSpell(sinisterStrike, ref energy, 45, 0, true);
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
                            if (comboCnt > 4 && energy > 35 && IsReady(deadlyThrow))
                            {
                                CastSpell(deadlyThrow, ref energy, 35, 1.5, true);
                                comboCnt -= 5;
                            }
                            else
                            {
                                CastSpell(throwAttack, ref energy, 0, 2.1, false);
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
                NextActionTime[hungerForBlood] = DateTime.Now;
                NextActionTime[garrote] = DateTime.Now;
            }
            private bool IsReady(DateTime nextAction)
            {
                return DateTime.Now > nextAction;
            }
            private bool IsReady(string spell)
            {
                bool result = true; // begin with neutral element of AND
                if(spell.Equals(hungerForBlood) || spell.Equals(sliceAndDice) || spell.Equals(garrote))
                {
                    // only use these spells in a certain interval
                    result &= (!NextActionTime.TryGetValue(spell, out DateTime NextSpellAvailable) || IsReady(NextSpellAvailable));
                }
                result &= (HookManager.GetSpellCooldown(spell) <= 0 && HookManager.GetUnitCastingInfo(WowLuaUnit.Player).Item2 <= 0);
                return result;
            }
            private int UpdateEnergy()
            {
                ObjectManager.UpdateObject(ObjectManager.Player.Type, ObjectManager.Player.BaseAddress);
                Player = ObjectManager.Player;
                return Player.Energy;
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
        }

        private CharacterManager CharacterManager { get; }

        private HookManager HookManager { get; }
        private bool standing = false;

        private Vector3 LastPlayerPosition { get; set; }
        private Vector3 LastTargetPosition { get; set; }
        private Vector3 LastBehindTargetPosition { get; set; }
        private float LastTargetRotation { get; set; }

        private double distanceToTarget = 0;
        private double distanceToBehindTarget = 0;
        private double distanceTraveled = 0;
        readonly string[] runningEmotes = { "/train", "/fart", "/burp", "/moo", "/lost", "/puzzled", "/cackle", "/silly", "/question", "/talk" };
        readonly string[] standingEmotes = { "/chug", "/pick", "/whistle", "/shimmy", "/dance", "/twiddle", "/bored", "/violin", "/highfive", "/bow" };

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
                Dancing = false;
                bool targetDistanceChanged = false;
                if(!LastPlayerPosition.Equals(ObjectManager.Player.Position))
                {
                    distanceTraveled = ObjectManager.Player.Position.GetDistance2D(LastPlayerPosition);
                    LastPlayerPosition = new Vector3(ObjectManager.Player.Position.X, ObjectManager.Player.Position.Y, ObjectManager.Player.Position.Z);
                    targetDistanceChanged = true;
                }
                if(LastTargetRotation != target.Rotation)
                {
                    hasTargetMoved = true;
                    LastTargetRotation = target.Rotation;
                }
                if (!LastTargetPosition.Equals(target.Position))
                {
                    hasTargetMoved = true;
                    LastTargetPosition = new Vector3(target.Position.X, target.Position.Y, target.Position.Z);
                    LastBehindTargetPosition = new Vector3(LastTargetPosition.X - (9 * (float)Math.Cos(LastTargetRotation)), LastTargetPosition.Y, LastTargetPosition.Z - (9 * (float)Math.Sin(LastTargetRotation)));
                    targetDistanceChanged = true;
                }
                else if(hasTargetMoved)
                {
                    hasTargetMoved = false;
                    computeNewRoute = true;
                }
                if(targetDistanceChanged)
                {
                    distanceToTarget = LastPlayerPosition.GetDistance2D(LastTargetPosition);
                    distanceToBehindTarget = LastPlayerPosition.GetDistance2D(LastBehindTargetPosition);
                }
                HandleMovement(target);
                HandleAttacking(target);
            }
            else if (!Dancing)
            {
                if(distanceTraveled < 0.001)
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
            if(!HookManager.GetBuffs(WowLuaUnit.Player).Any(e => e.Contains("tealth")))
            {
                HookManager.CastSpell("Stealth");
                Spells.ResetAfterTargetDeath();
            }
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

        private bool SearchNewTarget (ref WowUnit? target, bool grinding)
        {
            if ((target != null && !(target.IsDead || target.Health == 0)) || (HookManager.GetBuffs(WowLuaUnit.Player).Any(e => e.Contains("tealth")) && ObjectManager.Player.HealthPercentage <= 20))
            {
                return false;
            }
            List<WowUnit> wowUnits = ObjectManager.WowObjects.OfType<WowUnit>().Where(e => HookManager.GetUnitReaction(ObjectManager.Player, e) != WowUnitReaction.Friendly && HookManager.GetUnitReaction(ObjectManager.Player, e) != WowUnitReaction.Neutral).ToList();
            bool newTargetFound = false;
            int targetHealth = (target == null || target.IsDead) ? 0 : target.Health;
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
                        if(tmpDistance < 6.0)
                        {
                            targetCount++;
                        }
                        if ((unit.IsInCombat && unit.Health > targetHealth) || (!inCombat && grinding && unit.Health > targetHealth))
                        {
                            target = unit;
                            targetHealth = unit.Health;
                            newTargetFound = true;
                            inCombat = unit.IsInCombat;
                        }
                    }
                }
            }
            if(target == null || target.IsDead)
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
                Spells.ResetAfterTargetDeath();
            }
            return newTargetFound;
        }

        private void HandleAttacking(WowUnit target)
        {
            Spells.CastNextSpell(distanceToTarget, target, multipleTargets);
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
            if(HookManager.GetBuffs(WowLuaUnit.Player).Any(e => e.Contains("tealth")))
            {
                if(!wasInStealth || hasTargetMoved)
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
            if(distanceToBehindTarget < 3.0)
            {
                isSneaky = false;
            }
            bool closeToTarget = distanceToTarget < 10.0;
            if(hasTargetMoved)
            {
                CharacterManager.MoveToPosition(LastBehindTargetPosition);
            }
            else if(closeToTarget)
            {
                if (isSneaky)
                {
                    CharacterManager.MoveToPosition(LastBehindTargetPosition);
                }
                else if (!BotMath.IsFacing(LastPlayerPosition, ObjectManager.Player.Rotation, LastTargetPosition, 0.75, 1.25))
                {
                    CharacterManager.MoveToPosition(LastTargetPosition);
                }
            }
            else
            {
                if (computeNewRoute || MovementEngine.CurrentPath?.Count == 0)
                {
                    List<Vector3> path = PathfindingHandler.GetPath(ObjectManager.MapId, LastPlayerPosition, LastBehindTargetPosition);
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