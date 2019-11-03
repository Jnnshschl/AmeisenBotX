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
    public class PaladinProtection : ICombatClass
    {
        public PaladinProtection(ObjectManager objectManager, CharacterManager characterManager, HookManager hookManager, IPathfindingHandler pathhandler, DefaultMovementEngine movement)
        {
            ObjectManager = objectManager;
            CharacterManager = characterManager;
            HookManager = hookManager;
            PathfindingHandler = pathhandler;
            MovementEngine = movement;
            Jumped = false;
        }

        public bool HandlesMovement => true;

        public bool HandlesTargetSelection => true;

        public bool IsMelee => true;

        public bool Jumped { get; set; }

        private CharacterManager CharacterManager { get; }

        private HookManager HookManager { get; }

        private Vector3 LastPosition { get; set; }

        private ObjectManager ObjectManager { get; }

        private DateTime LastSacrifice { get; set; }
        private DateTime LastAvenger { get; set; }
        private DateTime LastHammer { get; set; }
        private DateTime LastHolyShield { get; set; }
        private DateTime LastDivineShield { get; set; }
        private DateTime LastProtection { get; set; }
        private bool Dancing { get; set; }

        private IPathfindingHandler PathfindingHandler { get; set; }

        private DefaultMovementEngine MovementEngine { get; set; }
        private DateTime LastGCD { get; set; }
        private double GCDTime { get; set; }

        private bool IsGCD()
        {
            return DateTime.Now.Subtract(LastGCD).TotalSeconds > GCDTime;
        }
        private void SetGCD(double GCDinSec)
        {
            GCDTime = GCDinSec;
            LastGCD = DateTime.Now;
        }

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
                if (target.IsDead)
                {
                    ulong leaderGuid = ObjectManager.ReadPartyLeaderGuid();
                    if (SearchNewTarget(ref target, leaderGuid == ObjectManager.PlayerGuid))
                    {
                        HandleMovement(target);
                        HandleAttacking(target);
                    }
                    else
                    {
                        HookManager.SendChatMessage("yeah, baby!");
                        HookManager.SendChatMessage("/dance");
                    }
                }
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
            int areaToLookAt = grinding ? 500 : 100;
            bool inCombat = target == null ? false : target.IsInCombat;
            int memberHealth = 2147483647;
            AmeisenLogger.Instance.Log(JsonConvert.SerializeObject(wowUnits));
            foreach (WowUnit unit in wowUnits)
            {
                if (BotUtils.IsValidUnit(unit) && unit != target && !unit.IsDead && ObjectManager.Player.Position.GetDistance(unit.Position) < areaToLookAt)
                {
                    int compHealth = 2147483647;
                    if (unit.IsInCombat)
                    {
                        WowUnit member = ObjectManager.WowObjects.OfType<WowUnit>().FirstOrDefault(t => t.Guid == unit.TargetGuid);
                        if(member != null)
                        {
                            compHealth = member.Health;
                        }
                    }
                    if ((unit.IsInCombat && compHealth < memberHealth) || !inCombat && grinding && (target == null || target.IsDead))
                    {
                        target = unit;
                        HookManager.TargetGuid(target.Guid);
                        newTargetFound = true;
                        inCombat = unit.IsInCombat;
                        memberHealth = compHealth;
                    }
                }
            }
            if(target == null || target.IsDead)
            {
                HookManager.ClearTarget();
                ulong leaderGuid = ObjectManager.ReadPartyLeaderGuid();
                if (leaderGuid != ObjectManager.PlayerGuid)
                {
                    WowUnit leader = ObjectManager.WowObjects.OfType<WowUnit>().FirstOrDefault(t => t.Guid == leaderGuid);
                    HandleMovement(leader);
                }

            }
            return newTargetFound;
        }

        private void HandleAttacking(WowUnit target)
        {
            if(IsGCD())
            {
                return;
            }
            double playerMana = ObjectManager.Player.Mana;
            double distanceToTarget = ObjectManager.Player.Position.GetDistance(target.Position);
            double targetHealthPercent = (target.Health / (double)target.MaxHealth) * 100.0;
            double playerHealthPercent = (ObjectManager.Player.Health / (double)ObjectManager.Player.MaxHealth) * 100.0;
            (string, int) targetCastingInfo = HookManager.GetUnitCastingInfo(WowLuaUnit.Target);
            List<string> Debuffs = HookManager.GetDebuffs(WowLuaUnit.Target.ToString());
            List<string> Buffs = HookManager.GetBuffs(WowLuaUnit.Player.ToString());

            // buffs
            if (!Buffs.Any(e => e.Contains("evotion")))
            {
                HookManager.CastSpell("Devotion Aura");
            }
            if (!Buffs.Any(e => e.Contains("ury")))
            {
                HookManager.CastSpell("Righteous Fury");
                SetGCD(1.5);
                return;
            }
            if (!Buffs.Any(e => e.Contains("ighteousness")))
            {
                HookManager.CastSpell("Seal of Righteousness");
            }
            if (playerHealthPercent > 50 && DateTime.Now.Subtract(LastSacrifice).TotalSeconds > 120)
            {
                HookManager.CastSpell("Divine Sacrifice");
                LastSacrifice = DateTime.Now;
                SetGCD(1.5);
                return;
            }

            // distance attack
            if (distanceToTarget > 10 && distanceToTarget < 30)
            {
                if (DateTime.Now.Subtract(LastAvenger).TotalSeconds > 30 && playerMana >= 1027)
                {
                    HookManager.CastSpell("Avenger's Shield");
                    LastAvenger = DateTime.Now;
                    HookManager.SendChatMessage("/s and i'm like.. bam!");
                    playerMana -= 1027;
                    SetGCD(1.5);
                    return;
                }
            }
            else
            {
                // close combat
                if (distanceToTarget < 10)
                {
                    if (DateTime.Now.Subtract(LastHammer).TotalSeconds > 60 && playerMana >= 117)
                    {
                        HookManager.CastSpell("Hammer of Justice");
                        LastHammer = DateTime.Now;
                        HookManager.SendChatMessage("/s STOP! hammertime!");
                        playerMana -= 117;
                        SetGCD(1.5);
                        return;
                    }
                }
            }

            // support members
            int lowHealth = 2147483647;
            WowUnit lowMember = null;
            foreach (ulong memberGuid in ObjectManager.PartymemberGuids)
            {
                WowUnit member = ObjectManager.WowObjects.OfType<WowUnit>().FirstOrDefault(t => t.Guid == memberGuid);
                if (member != null && member.Health < lowHealth)
                {
                    lowHealth = member.Health;
                    lowMember = member;
                }
            }
            if(lowMember != null)
            {
                HookManager.TargetGuid(lowMember.Guid);
                if(lowMember.IsDazed || lowMember.IsConfused || lowMember.IsFleeing || lowMember.IsSilenced)
                {
                    if (playerMana >= 276)
                    {
                        HookManager.CastSpell("Blessing of Sanctuary");
                        playerMana -= 276;
                        SetGCD(1.5);
                        return;
                    }
                    if (playerMana >= 236)
                    {
                        HookManager.CastSpell("Hand of Freedom");
                        playerMana -= 236;
                        SetGCD(1.5);
                        return;
                    }
                }
                if (lowMember.HealthPercentage < 20 && playerMana >= 117)
                {
                    HookManager.CastSpell("Divine Shield");
                    LastDivineShield = DateTime.Now;
                    playerMana -= 117;
                    SetGCD(1.5);
                    return;
                }
                else if (lowMember.HealthPercentage < 50 && playerMana >= 117)
                {
                    HookManager.CastSpell("Divine Protection");
                    LastProtection = DateTime.Now;
                    playerMana -= 117;
                }
            }

            // self-casts
            if (DateTime.Now.Subtract(LastHolyShield).TotalSeconds > 8 && playerMana >= 395)
            {
                HookManager.ClearTarget();
                HookManager.CastSpell("Holy Shield");
                LastHolyShield = DateTime.Now;
                playerMana -= 395;
                SetGCD(1.5);
                return;
            }

            // back to attack
            HookManager.TargetGuid(target.Guid);
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