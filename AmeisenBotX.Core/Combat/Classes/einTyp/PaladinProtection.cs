﻿using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Combat.Classes.einTyp
{
    public class PaladinProtection : ICombatClass
    {
        private readonly string[] runningEmotes = { "/question", "/talk" };
        private readonly string[] standingEmotes = { "/bow" };
        private readonly AmeisenBotInterfaces Bot;
        private bool computeNewRoute = false;
        private double distanceToTarget = 0;
        private bool multipleTargets = false;
        private bool standing = false;

        public PaladinProtection(AmeisenBotInterfaces bot)
        {
            Bot = bot;
        }

        public string Author => "einTyp";

        public IEnumerable<int> BlacklistedTargetDisplayIds { get; set; }

        public Dictionary<string, dynamic> C { get; set; } = new Dictionary<string, dynamic>();

        public string Description => "...";

        public string Displayname => "Protection Paladin";

        public bool HandlesFacing => false;

        public bool HandlesMovement => true;

        public bool HandlesTargetSelection => true;

        public bool IsMelee => true;

        public IItemComparator ItemComparator => new TankItemComparator();

        public IEnumerable<int> PriorityTargetDisplayIds { get; set; }

        public WowRole Role => WowRole.Tank;

        public TalentTree Talents { get; } = new()
        {
            Tree1 = new()
            {
                { 2, new(1, 2, 5) },
                { 4, new(1, 4, 5) },
                { 5, new(1, 5, 2) }
            },
            Tree2 = new()
            {
                { 1, new(2, 1, 5) },
                { 2, new(2, 2, 5) },
                { 3, new(2, 3, 3) },
                { 4, new(2, 4, 2) },
                { 5, new(2, 5, 5) },
                { 6, new(2, 6, 1) },
                { 7, new(2, 7, 3) },
                { 8, new(2, 8, 5) },
                { 9, new(2, 9, 2) },
                { 12, new(2, 12, 1) },
                { 14, new(2, 14, 2) },
                { 16, new(2, 16, 2) },
                { 17, new(2, 17, 1) },
                { 18, new(2, 18, 3) },
                { 19, new(2, 19, 3) },
                { 22, new(2, 22, 1) },
                { 23, new(2, 23, 2) },
                { 24, new(2, 24, 3) }
            },
            Tree3 = new()
            {
                { 1, new(3, 1, 5) },
                { 2, new(3, 2, 5) }
            }
        };

        public string Version => "1.0";

        public bool WalkBehindEnemy => false;

        public WowClass WowClass => WowClass.Paladin;

        private bool Dancing { get; set; }

        private double GCDTime { get; set; }

        private DateTime LastAvenger { get; set; }

        private DateTime LastConsecration { get; set; }

        private DateTime LastDivineShield { get; set; }

        private DateTime LastGCD { get; set; }

        private DateTime LastHammer { get; set; }

        private DateTime LastHolyShield { get; set; }

        private Vector3 LastPlayerPosition { get; set; }

        private DateTime LastProtection { get; set; }

        private DateTime LastSacrifice { get; set; }

        private Vector3 LastTargetPosition { get; set; }

        private DateTime LastWisdom { get; set; }

        public void AttackTarget()
        {
            WowUnit target = Bot.Target;
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
            WowUnit target = Bot.Target;
            if ((Bot.Wow.TargetGuid != 0 && target != null && !(target.IsDead || target.Health < 1)) || SearchNewTarget(ref target, false))
            {
                bool targetDistanceChanged = false;
                if (!LastPlayerPosition.Equals(Bot.Player.Position))
                {
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
            Bot.Globals.ForceCombat = false;
        }

        public bool IsTargetAttackable(WowUnit target)
        {
            return true;
        }

        public void OutOfCombatExecute()
        {
            double distanceTraveled = Bot.Player.Position.GetDistance(LastPlayerPosition);
            computeNewRoute = false;
            if (!LastPlayerPosition.Equals(Bot.Player.Position))
            {
                distanceTraveled = Bot.Player.Position.GetDistance(LastPlayerPosition);
                LastPlayerPosition = new Vector3(Bot.Player.Position.X, Bot.Player.Position.Y, Bot.Player.Position.Z);
            }

            if (distanceTraveled < 0.001)
            {
                ulong leaderGuid = Bot.Objects.Partyleader.Guid;
                WowUnit target = Bot.Target;
                WowUnit leader = null;
                if (leaderGuid != 0)
                {
                    leader = Bot.Objects.GetWowObjectByGuid<WowUnit>(leaderGuid);
                }

                if (leaderGuid != 0 && leaderGuid != Bot.Wow.PlayerGuid && leader != null && !(leader.IsDead || leader.Health < 1))
                {
                    Bot.Movement.SetMovementAction(Movement.Enums.MovementAction.Move, Bot.Objects.GetWowObjectByGuid<WowUnit>(leaderGuid).Position);
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
                    Bot.Wow.LuaSendChatMessage(runningEmotes[new Random().Next(runningEmotes.Length)]);
                    Dancing = true;
                }
            }
        }

        private void HandleAttacking(WowUnit target)
        {
            bool gcdWaiting = IsGCD();
            Bot.Wow.WowTargetGuid(target.Guid);
            bool targetAimed = true;
            double playerMana = Bot.Player.Mana;
            double targetHealthPercent = target.HealthPercentage;
            double playerHealthPercent = Bot.Player.HealthPercentage;
            List<string> buffs = Bot.Player.Auras.Select(e => Bot.Db.GetSpellName(e.SpellId)).ToList();

            // buffs
            if (!buffs.Any(e => e.Contains("evotion")))
            {
                Bot.Wow.LuaCastSpell("Devotion Aura");
            }

            if (!gcdWaiting && !buffs.Any(e => e.Contains("ury")))
            {
                Bot.Wow.LuaCastSpell("Righteous Fury");
                SetGCD(1.5);
                return;
            }

            if (!buffs.Any(e => e.Contains("ighteousness")))
            {
                Bot.Wow.LuaCastSpell("Seal of Righteousness");
            }

            if (!gcdWaiting && playerHealthPercent > 50 && DateTime.Now.Subtract(LastSacrifice).TotalSeconds > 120)
            {
                Bot.Wow.LuaCastSpell("Divine Sacrifice");
                LastSacrifice = DateTime.Now;
                SetGCD(1.5);
                return;
            }

            // distance attack
            if (!gcdWaiting && distanceToTarget > (10 + target.CombatReach) && distanceToTarget < (30 + target.CombatReach))
            {
                if (DateTime.Now.Subtract(LastAvenger).TotalSeconds > 30 && playerMana >= 1027)
                {
                    Bot.Wow.LuaCastSpell("Avenger's Shield");
                    LastAvenger = DateTime.Now;
                    Bot.Wow.LuaSendChatMessage("/s and i'm like.. bam!");
                    playerMana -= 1027;
                    SetGCD(1.5);
                    return;
                }
            }
            else
            {
                // close combat
                if (!gcdWaiting && distanceToTarget <= 0.75f * (Bot.Player.CombatReach + target.CombatReach))
                {
                    if (multipleTargets && DateTime.Now.Subtract(LastConsecration).TotalSeconds > 8 && playerMana >= 869)
                    {
                        Bot.Wow.LuaCastSpell("Consecration");
                        LastConsecration = DateTime.Now;
                        Bot.Wow.LuaSendChatMessage("/s MOVE BITCH!!!!!11");
                        playerMana -= 869;
                        SetGCD(1.5);
                        return;
                    }

                    if (DateTime.Now.Subtract(LastHammer).TotalSeconds > 60 && playerMana >= 117)
                    {
                        Bot.Wow.LuaCastSpell("Hammer of Justice");
                        LastHammer = DateTime.Now;
                        Bot.Wow.LuaSendChatMessage("/s STOP! hammertime!");
                        playerMana -= 117;
                        SetGCD(1.5);
                        return;
                    }
                }
            }

            // support members
            int lowHealth = 2147483647;
            WowUnit lowMember = null;
            foreach (ulong memberGuid in Bot.Objects.PartymemberGuids)
            {
                WowUnit member = Bot.Objects.GetWowObjectByGuid<WowUnit>(memberGuid);
                if (member != null && member.Health < lowHealth)
                {
                    lowHealth = member.Health;
                    lowMember = member;
                }
            }

            if (lowMember != null)
            {
                if (!gcdWaiting && (lowMember.IsDazed || lowMember.IsConfused || lowMember.IsFleeing || lowMember.IsSilenced))
                {
                    if (playerMana >= 276)
                    {
                        Bot.Wow.WowTargetGuid(lowMember.Guid);
                        targetAimed = false;
                        Bot.Wow.LuaCastSpell("Blessing of Sanctuary");
                        playerMana -= 276;
                        SetGCD(1.5);
                        return;
                    }

                    if (playerMana >= 236)
                    {
                        Bot.Wow.WowTargetGuid(lowMember.Guid);
                        targetAimed = false;
                        Bot.Wow.LuaCastSpell("Hand of Freedom");
                        playerMana -= 236;
                        SetGCD(1.5);
                        return;
                    }
                }

                if (lowMember.HealthPercentage > 1)
                {
                    if (!gcdWaiting && DateTime.Now.Subtract(LastDivineShield).TotalSeconds > 240 && lowMember.HealthPercentage < 20 && playerMana >= 117)
                    {
                        Bot.Wow.WowTargetGuid(lowMember.Guid);
                        targetAimed = false;
                        Bot.Wow.LuaCastSpell("Divine Shield");
                        LastDivineShield = DateTime.Now;
                        playerMana -= 117;
                        SetGCD(1.5);
                        return;
                    }
                    else if (lowMember.HealthPercentage < 50 && DateTime.Now.Subtract(LastProtection).TotalSeconds > 120 && playerMana >= 117)
                    {
                        Bot.Wow.WowTargetGuid(lowMember.Guid);
                        targetAimed = false;
                        Bot.Wow.LuaCastSpell("Divine Protection");
                        LastProtection = DateTime.Now;
                        playerMana -= 117;
                    }
                }
            }

            // self-casts
            if (!gcdWaiting && DateTime.Now.Subtract(LastHolyShield).TotalSeconds > 8 && playerMana >= 395)
            {
                Bot.Wow.WowClearTarget();
                targetAimed = false;
                Bot.Wow.LuaCastSpell("Holy Shield");
                LastHolyShield = DateTime.Now;
                playerMana -= 395;
                SetGCD(1.5);
                return;
            }

            if (!gcdWaiting && DateTime.Now.Subtract(LastWisdom).TotalSeconds > 600 && playerMana >= 197)
            {
                Bot.Wow.WowClearTarget();
                targetAimed = false;
                Bot.Wow.LuaCastSpell("Blessing of Wisdom");
                LastWisdom = DateTime.Now;
                playerMana -= 197;
                SetGCD(1.5);
                return;
            }

            // back to attack
            if (!targetAimed)
            {
                Bot.Wow.WowTargetGuid(target.Guid);
            }

            if (!Bot.Player.IsAutoAttacking)
            {
                Bot.Wow.LuaStartAutoAttack();
            }
        }

        private void HandleMovement(WowUnit target)
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

        private bool IsGCD()
        {
            return DateTime.Now.Subtract(LastGCD).TotalSeconds < GCDTime;
        }

        private bool SearchNewTarget(ref WowUnit target, bool grinding)
        {
            if (Bot.Wow.TargetGuid != 0 && target != null && !(target.IsDead || target.Health < 1 || target.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId).Contains("Spirit of Redem"))))
            {
                return false;
            }

            List<WowUnit> wowUnits = Bot.Objects.WowObjects.OfType<WowUnit>().Where(e => Bot.Db.GetReaction(Bot.Player, e) != WowUnitReaction.Friendly && Bot.Db.GetReaction(Bot.Player, e) != WowUnitReaction.Neutral).ToList();
            bool newTargetFound = false;
            int areaToLookAt = grinding ? 100 : 50;
            bool inCombat = (target == null || target.IsDead || target.Health < 1) ? false : target.IsInCombat;
            int targetHealth = (target == null || target.IsDead || target.Health < 1) ? 2147483647 : target.Health;
            ulong memberGuid = (target == null || target.IsDead || target.Health < 1) ? 0 : target.TargetGuid;
            WowUnit member = (target == null || target.IsDead || target.Health < 1) ? null : Bot.Objects.GetWowObjectByGuid<WowUnit>(memberGuid);
            int memberHealth = member == null ? 2147483647 : member.Health;
            int targetCount = 0;
            multipleTargets = false;
            foreach (WowUnit unit in wowUnits)
            {
                if (WowUnit.IsValidUnit(unit) && unit != target && !(unit.IsDead || unit.Health < 1 || unit.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId).Contains("Spirit of Redem"))))
                {
                    double tmpDistance = Bot.Player.Position.GetDistance(unit.Position);
                    if (tmpDistance < areaToLookAt)
                    {
                        int compHealth = 2147483647;
                        if (tmpDistance < 6.0)
                        {
                            targetCount++;
                        }

                        if (unit.IsInCombat)
                        {
                            member = Bot.Objects.GetWowObjectByGuid<WowUnit>(unit.TargetGuid);
                            if (member != null)
                            {
                                compHealth = member.Health;
                            }
                        }

                        if (((unit.IsInCombat && (compHealth < memberHealth || (compHealth == memberHealth && targetHealth < unit.Health))) || (!inCombat && grinding && (target == null || target.IsDead) && unit.Health < targetHealth)) && Bot.Wow.WowIsInLineOfSight(Bot.Player.Position, unit.Position))
                        {
                            target = unit;
                            newTargetFound = true;
                            inCombat = unit.IsInCombat;
                            memberHealth = compHealth;
                            targetHealth = unit.Health;
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
            else if (newTargetFound)
            {
                Bot.Wow.WowTargetGuid(target.Guid);
            }

            if (targetCount > 1)
            {
                multipleTargets = true;
            }

            return newTargetFound;
        }

        private void SetGCD(double gcdInSec)
        {
            GCDTime = gcdInSec;
            LastGCD = DateTime.Now;
        }
    }
}