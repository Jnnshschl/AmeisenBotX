using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Talents.Objects;
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

namespace AmeisenBotX.Core.Statemachine.CombatClasses.einTyp
{
    public class PaladinProtection : ICombatClass
    {
        private bool computeNewRoute = false;
        private double distanceToTarget = 0;
        private bool hasTargetMoved = false;
        private bool multipleTargets = false;

        public PaladinProtection(IObjectManager objectManager, ICharacterManager characterManager, IHookManager hookManager, IMovementEngine movement)
        {
            ObjectManager = objectManager;
            CharacterManager = characterManager;
            HookManager = hookManager;
            MovementEngine = movement;
            Jumped = false;
            LastTargetCheck = DateTime.Now;
        }

        public string Author => "einTyp";

        public WowClass Class => WowClass.Paladin;

        public Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public string Description => "...";

        public string Displayname => "Protection Paladin";

        public bool HandlesMovement => true;

        public bool HandlesTargetSelection => true;

        public bool IsMelee => true;

        public IWowItemComparator ItemComparator => new TankItemComparator();

        public bool Jumped { get; set; }

        public List<string> PriorityTargets { get; set; }

        public CombatClassRole Role => CombatClassRole.Tank;

        public TalentTree Talents { get; } = new TalentTree()
        {
            Tree1 = new Dictionary<int, Talent>()
            {
                { 2, new Talent(1, 2, 5) },
                { 4, new Talent(1, 4, 5) },
                { 5, new Talent(1, 5, 2) }
            },
            Tree2 = new Dictionary<int, Talent>()
            {
                { 1, new Talent(2, 1, 5) },
                { 2, new Talent(2, 2, 5) },
                { 3, new Talent(2, 3, 3) },
                { 4, new Talent(2, 4, 2) },
                { 5, new Talent(2, 5, 5) },
                { 6, new Talent(2, 6, 1) },
                { 7, new Talent(2, 7, 3) },
                { 8, new Talent(2, 8, 5) },
                { 9, new Talent(2, 9, 2) },
                { 12, new Talent(2, 12, 1) },
                { 14, new Talent(2, 14, 2) },
                { 16, new Talent(2, 16, 2) },
                { 17, new Talent(2, 17, 1) },
                { 18, new Talent(2, 18, 3) },
                { 19, new Talent(2, 19, 3) },
                { 22, new Talent(2, 22, 1) },
                { 23, new Talent(2, 23, 2) },
                { 24, new Talent(2, 24, 3) }
            },
            Tree3 = new Dictionary<int, Talent>()
            {
                { 1, new Talent(3, 1, 5) },
                { 2, new Talent(3, 2, 5) }
            }
        };

        public string Version => "1.0";

        public bool WalkBehindEnemy => false;

        private ICharacterManager CharacterManager { get; }

        private bool Dancing { get; set; }

        private double GCDTime { get; set; }

        private IHookManager HookManager { get; }

        private DateTime LastAvenger { get; set; }

        private DateTime LastConsecration { get; set; }

        private DateTime LastDivineShield { get; set; }

        private DateTime LastGCD { get; set; }

        private DateTime LastHammer { get; set; }

        private DateTime LastHolyShield { get; set; }

        private Vector3 LastPlayerPosition { get; set; }

        private DateTime LastProtection { get; set; }

        private DateTime LastSacrifice { get; set; }

        private DateTime LastTargetCheck { get; set; }

        private Vector3 LastTargetPosition { get; set; }

        private DateTime LastWisdom { get; set; }

        private IMovementEngine MovementEngine { get; set; }

        private IObjectManager ObjectManager { get; }

        public void Execute()
        {
            ulong targetGuid = ObjectManager.TargetGuid;
            WowUnit target = ObjectManager.WowObjects.OfType<WowUnit>().FirstOrDefault(t => t.Guid == targetGuid);
            SearchNewTarget(ref target, false);
            if (target != null)
            {
                bool targetDistanceChanged = false;
                if (!LastPlayerPosition.Equals(ObjectManager.Player.Position))
                {
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
                    Console.WriteLine("distanceToTarget: " + distanceToTarget);
                }

                HandleMovement(target);
                HandleAttacking(target);
            }
        }

        public void OutOfCombatExecute()
        {
            double distanceTraveled = ObjectManager.Player.Position.GetDistance(LastPlayerPosition);
            if (distanceTraveled < 0.001)
            {
                ulong leaderGuid = ObjectManager.PartyleaderGuid;
                WowUnit target = null;
                if (leaderGuid == ObjectManager.PlayerGuid && SearchNewTarget(ref target, true))
                {
                    HandleMovement(target);
                    HandleAttacking(target);
                }
                else if (!Dancing)
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

        private void HandleAttacking(WowUnit target)
        {
            bool gcdWaiting = IsGCD();
            bool targetAimed = true;
            double playerMana = ObjectManager.Player.Mana;
            double targetHealthPercent = target.HealthPercentage;
            double playerHealthPercent = ObjectManager.Player.HealthPercentage;
            List<string> buffs = HookManager.GetBuffs(WowLuaUnit.Player);

            // buffs
            if (!buffs.Any(e => e.Contains("evotion")))
            {
                HookManager.CastSpell("Devotion Aura");
            }

            if (!gcdWaiting && !buffs.Any(e => e.Contains("ury")))
            {
                HookManager.CastSpell("Righteous Fury");
                SetGCD(1.5);
                return;
            }

            if (!buffs.Any(e => e.Contains("ighteousness")))
            {
                HookManager.CastSpell("Seal of Righteousness");
            }

            if (!gcdWaiting && playerHealthPercent > 50 && DateTime.Now.Subtract(LastSacrifice).TotalSeconds > 120)
            {
                HookManager.CastSpell("Divine Sacrifice");
                LastSacrifice = DateTime.Now;
                SetGCD(1.5);
                return;
            }

            // distance attack
            if (!gcdWaiting && distanceToTarget > (10 + target.CombatReach) && distanceToTarget < (30 + target.CombatReach))
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
                if (!gcdWaiting && distanceToTarget < target.CombatReach)
                {
                    if (multipleTargets && DateTime.Now.Subtract(LastConsecration).TotalSeconds > 8 && playerMana >= 869)
                    {
                        HookManager.CastSpell("Consecration");
                        LastConsecration = DateTime.Now;
                        HookManager.SendChatMessage("/s MOVE BITCH!!!!!11");
                        playerMana -= 869;
                        SetGCD(1.5);
                        return;
                    }

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

            if (lowMember != null)
            {
                if (!gcdWaiting && (lowMember.IsDazed || lowMember.IsConfused || lowMember.IsFleeing || lowMember.IsSilenced))
                {
                    if (playerMana >= 276)
                    {
                        HookManager.TargetGuid(lowMember.Guid);
                        targetAimed = false;
                        HookManager.CastSpell("Blessing of Sanctuary");
                        playerMana -= 276;
                        SetGCD(1.5);
                        return;
                    }

                    if (playerMana >= 236)
                    {
                        HookManager.TargetGuid(lowMember.Guid);
                        targetAimed = false;
                        HookManager.CastSpell("Hand of Freedom");
                        playerMana -= 236;
                        SetGCD(1.5);
                        return;
                    }
                }

                if (lowMember.HealthPercentage > 1)
                {
                    if (!gcdWaiting && DateTime.Now.Subtract(LastDivineShield).TotalSeconds > 240 && lowMember.HealthPercentage < 20 && playerMana >= 117)
                    {
                        HookManager.TargetGuid(lowMember.Guid);
                        targetAimed = false;
                        HookManager.CastSpell("Divine Shield");
                        LastDivineShield = DateTime.Now;
                        playerMana -= 117;
                        SetGCD(1.5);
                        return;
                    }
                    else if (lowMember.HealthPercentage < 50 && DateTime.Now.Subtract(LastProtection).TotalSeconds > 120 && playerMana >= 117)
                    {
                        HookManager.TargetGuid(lowMember.Guid);
                        targetAimed = false;
                        HookManager.CastSpell("Divine Protection");
                        LastProtection = DateTime.Now;
                        playerMana -= 117;
                    }
                }
            }

            // self-casts
            if (!gcdWaiting && DateTime.Now.Subtract(LastHolyShield).TotalSeconds > 8 && playerMana >= 395)
            {
                HookManager.ClearTarget();
                targetAimed = false;
                HookManager.CastSpell("Holy Shield");
                LastHolyShield = DateTime.Now;
                playerMana -= 395;
                SetGCD(1.5);
                return;
            }

            if (!gcdWaiting && DateTime.Now.Subtract(LastWisdom).TotalSeconds > 600 && playerMana >= 197)
            {
                HookManager.ClearTarget();
                targetAimed = false;
                HookManager.CastSpell("Blessing of Wisdom");
                LastWisdom = DateTime.Now;
                playerMana -= 197;
                SetGCD(1.5);
                return;
            }

            // back to attack
            if (!targetAimed)
            {
                HookManager.TargetGuid(target.Guid);
            }

            if (!ObjectManager.Player.IsAutoAttacking)
            {
                HookManager.StartAutoAttack(ObjectManager.Target);
            }
        }

        private void HandleMovement(WowUnit target)
        {
            if (target == null)
            {
                return;
            }

            if (hasTargetMoved || (distanceToTarget < target.CombatReach && !BotMath.IsFacing(LastPlayerPosition, ObjectManager.Player.Rotation, LastTargetPosition, 0.5f)))
            {
                HookManager.FacePosition(ObjectManager.Player, target.Position);
                CharacterManager.MoveToPosition(LastTargetPosition);
            }
            else if (distanceToTarget >= target.CombatReach)
            {
                if (computeNewRoute || MovementEngine.Path?.Count == 0)
                {
                    MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving, LastTargetPosition, target.Rotation);
                }
            }
        }

        private bool IsGCD()
        {
            return DateTime.Now.Subtract(LastGCD).TotalSeconds < GCDTime;
        }

        private bool SearchNewTarget(ref WowUnit target, bool grinding)
        {
            if (DateTime.Now.Subtract(LastTargetCheck).TotalSeconds < 1 && target != null && !(target.IsDead || target.Health == 0))
            {
                return false;
            }

            LastTargetCheck = DateTime.Now;
            List<WowUnit> wowUnits = ObjectManager.WowObjects.OfType<WowUnit>().Where(e => HookManager.GetUnitReaction(ObjectManager.Player, e) != WowUnitReaction.Friendly && HookManager.GetUnitReaction(ObjectManager.Player, e) != WowUnitReaction.Neutral).ToList();
            bool newTargetFound = false;
            int areaToLookAt = grinding ? 500 : 100;
            bool inCombat = (target == null || target.IsDead) ? false : target.IsInCombat;
            int targetHealth = (target == null || target.IsDead) ? 2147483647 : target.Health;
            ulong memberGuid = (target == null || target.IsDead) ? 0 : target.TargetGuid;
            WowUnit member = (target == null || target.IsDead) ? null : ObjectManager.WowObjects.OfType<WowUnit>().FirstOrDefault(t => t.Guid == memberGuid);
            int memberHealth = member == null ? 2147483647 : member.Health;
            int targetCount = 0;
            multipleTargets = false;
            foreach (WowUnit unit in wowUnits)
            {
                if (BotUtils.IsValidUnit(unit) && unit != target && !unit.IsDead)
                {
                    double tmpDistance = ObjectManager.Player.Position.GetDistance(unit.Position);
                    if (tmpDistance < areaToLookAt)
                    {
                        int compHealth = 2147483647;
                        if (tmpDistance < 6.0)
                        {
                            targetCount++;
                        }

                        if (unit.IsInCombat)
                        {
                            member = ObjectManager.WowObjects.OfType<WowUnit>().FirstOrDefault(t => t.Guid == unit.TargetGuid);
                            if (member != null)
                            {
                                compHealth = member.Health;
                            }
                        }

                        if (((unit.IsInCombat && compHealth < memberHealth) || (!inCombat && grinding && (target == null || target.IsDead) && unit.Health < targetHealth)) && HookManager.IsInLineOfSight(ObjectManager.Player.Position, unit.Position))
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

            if (target == null || target.IsDead)
            {
                HookManager.ClearTarget();
                ulong leaderGuid = ObjectManager.PartyleaderGuid;
                if (leaderGuid != ObjectManager.PlayerGuid)
                {
                    WowUnit leader = ObjectManager.WowObjects.OfType<WowUnit>().FirstOrDefault(t => t.Guid == leaderGuid);
                    HandleMovement(leader);
                }
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

        private void SetGCD(double gcdInSec)
        {
            GCDTime = gcdInSec;
            LastGCD = DateTime.Now;
        }
    }
}