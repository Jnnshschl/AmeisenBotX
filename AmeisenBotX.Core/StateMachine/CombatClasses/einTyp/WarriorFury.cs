using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObjects;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Core.Statemachine.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Statemachine.CombatClasses.einTyp
{
    public class WarriorFury : ICombatClass
    {
        private readonly string[] runningEmotes = { "/train", "/cackle", "/silly" };
        private readonly string[] standingEmotes = { "/shimmy", "/dance", "/twiddle", "/highfive" };
        private readonly WarriorFurySpells spells;

        private bool computeNewRoute = false;
        private double distanceToTarget = 0;

        private double distanceTraveled = 0;

        private bool multipleTargets = false;
        private bool standing = false;
        private WowInterface WowInterface;

        public WarriorFury(WowInterface wowInterface)
        {
            WowInterface = wowInterface;
            spells = new WarriorFurySpells(wowInterface);
        }

        public string Author => "einTyp";

        public WowClass WowClass => WowClass.Warrior;

        public Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public string Description => "...";

        public string Displayname => "Fury Warrior";

        public bool HandlesMovement => true;

        public bool HandlesTargetSelection => true;

        public bool IsMelee => true;

        public IWowItemComparator ItemComparator => new FuryItemComparator(WowInterface);

        public IEnumerable<int> PriorityTargetDisplayIds { get; set; }

        public IEnumerable<int> BlacklistedTargetDisplayIds { get; set; }

        public bool TargetInLineOfSight { get; set; }

        public CombatClassRole Role => CombatClassRole.Dps;

        public TalentTree Talents { get; } = new TalentTree()
        {
            Tree1 = new Dictionary<int, Talent>()
            {
                { 1, new Talent(1, 1, 3) },
                { 2, new Talent(1, 2, 2) },
                { 4, new Talent(1, 4, 2) },
                { 6, new Talent(1, 6, 3) },
                { 8, new Talent(1, 8, 1) },
                { 9, new Talent(1, 9, 2) },
                { 10, new Talent(1, 10, 3) }
            },
            Tree2 = new Dictionary<int, Talent>()
            {
                { 3, new Talent(2, 3, 5) },
                { 5, new Talent(2, 5, 5) },
                { 9, new Talent(2, 9, 5) },
                { 10, new Talent(2, 10, 5) },
                { 11, new Talent(2, 11, 2) },
                { 12, new Talent(2, 12, 5) },
                { 13, new Talent(2, 13, 3) },
                { 14, new Talent(2, 14, 1) },
                { 17, new Talent(2, 17, 5) },
                { 18, new Talent(2, 18, 3) },
                { 19, new Talent(2, 19, 1) },
                { 22, new Talent(2, 22, 5) },
                { 24, new Talent(2, 24, 1) },
                { 25, new Talent(2, 25, 3) },
                { 26, new Talent(2, 26, 5) },
                { 27, new Talent(2, 27, 1) }
            },
            Tree3 = new Dictionary<int, Talent>()
        };

        public string Version => "1.0";

        public bool WalkBehindEnemy => false;

        private bool Dancing { get; set; }

        private Vector3 LastPlayerPosition { get; set; }

        private Vector3 LastTargetPosition { get; set; }

        public void Execute()
        {
            computeNewRoute = false;
            WowUnit target = WowInterface.ObjectManager.Target;
            if ((WowInterface.ObjectManager.TargetGuid != 0 && target != null && !(target.IsDead || target.Health < 1)) || SearchNewTarget(ref target, false))
            {
                Dancing = false;
                bool targetDistanceChanged = false;
                if (!LastPlayerPosition.Equals(WowInterface.ObjectManager.Player.Position))
                {
                    distanceTraveled = WowInterface.ObjectManager.Player.Position.GetDistance(LastPlayerPosition);
                    LastPlayerPosition = new Vector3(WowInterface.ObjectManager.Player.Position.X, WowInterface.ObjectManager.Player.Position.Y, WowInterface.ObjectManager.Player.Position.Z);
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
            else if (!Dancing)
            {
                if (distanceTraveled < 0.001)
                {
                    WowInterface.HookManager.WowClearTarget();
                    WowInterface.HookManager.LuaSendChatMessage(standingEmotes[new Random().Next(standingEmotes.Length)]);
                    Dancing = true;
                    WowInterface.Globals.ForceCombat = false;
                }
                else
                {
                    WowInterface.HookManager.WowClearTarget();
                    WowInterface.HookManager.LuaSendChatMessage(runningEmotes[new Random().Next(runningEmotes.Length)]);
                    Dancing = true;
                }
            }
        }

        public void OutOfCombatExecute()
        {
            computeNewRoute = false;
            if (!LastPlayerPosition.Equals(WowInterface.ObjectManager.Player.Position))
            {
                distanceTraveled = WowInterface.ObjectManager.Player.Position.GetDistance(LastPlayerPosition);
                LastPlayerPosition = new Vector3(WowInterface.ObjectManager.Player.Position.X, WowInterface.ObjectManager.Player.Position.Y, WowInterface.ObjectManager.Player.Position.Z);
            }

            if (distanceTraveled < 0.001)
            {
                ulong leaderGuid = WowInterface.ObjectManager.PartyleaderGuid;
                WowUnit target = WowInterface.ObjectManager.Target;
                WowUnit leader = null;
                if (leaderGuid != 0)
                    leader = WowInterface.ObjectManager.GetWowObjectByGuid<WowUnit>(leaderGuid);
                if (leaderGuid != 0 && leaderGuid != WowInterface.ObjectManager.PlayerGuid && leader != null && !(leader.IsDead || leader.Health < 1))
                {
                    WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving, WowInterface.ObjectManager.GetWowObjectByGuid<WowUnit>(leaderGuid).Position);
                }
                else if ((WowInterface.ObjectManager.TargetGuid != 0 && target != null && !(target.IsDead || target.Health < 1)) || SearchNewTarget(ref target, true))
                {
                    if (!LastTargetPosition.Equals(target.Position))
                    {
                        computeNewRoute = true;
                        LastTargetPosition = new Vector3(target.Position.X, target.Position.Y, target.Position.Z);
                        distanceToTarget = LastPlayerPosition.GetDistance(LastTargetPosition);
                    }

                    Dancing = false;
                    HandleMovement(target);
                    WowInterface.Globals.ForceCombat = true;
                    HandleAttacking(target);
                }
                else if (!Dancing || standing)
                {
                    standing = false;
                    WowInterface.HookManager.WowClearTarget();
                    WowInterface.HookManager.LuaSendChatMessage(standingEmotes[new Random().Next(standingEmotes.Length)]);
                    Dancing = true;
                }
            }
            else
            {
                if (!Dancing || !standing)
                {
                    standing = true;
                    WowInterface.HookManager.WowClearTarget();
                    WowInterface.HookManager.LuaSendChatMessage(runningEmotes[new Random().Next(runningEmotes.Length)]);
                    Dancing = true;
                }
            }
        }

        private void HandleAttacking(WowUnit target)
        {
            WowInterface.HookManager.WowTargetGuid(target.Guid);
            spells.CastNextSpell(distanceToTarget, target, multipleTargets);
            if (target.IsDead || target.Health < 1)
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

            if(WowInterface.MovementEngine.MovementAction != Movement.Enums.MovementAction.None && distanceToTarget < 0.75f * (WowInterface.ObjectManager.Player.CombatReach + target.CombatReach))
            {
                WowInterface.MovementEngine.StopMovement();
            }

            if(computeNewRoute)
            {
                if(!BotMath.IsFacing(LastPlayerPosition, WowInterface.ObjectManager.Player.Rotation, LastTargetPosition, 0.5f))
                    WowInterface.HookManager.WowFacePosition(WowInterface.ObjectManager.Player, target.Position);
                WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving, target.Position, target.Rotation);
            }
        }

        private bool SearchNewTarget(ref WowUnit target, bool grinding)
        {
            if (WowInterface.ObjectManager.TargetGuid != 0 && target != null && !(target.IsDead || target.Health < 1 || target.Auras.Any(e => e.Name.Contains("Spirit of Redem"))))
            {
                return false;
            }

            List<WowUnit> wowUnits = WowInterface.ObjectManager.WowObjects.OfType<WowUnit>().Where(e => WowInterface.HookManager.WowGetUnitReaction(WowInterface.ObjectManager.Player, e) != WowUnitReaction.Friendly && WowInterface.HookManager.WowGetUnitReaction(WowInterface.ObjectManager.Player, e) != WowUnitReaction.Neutral).ToList();
            bool newTargetFound = false;
            int targetHealth = (target == null || target.IsDead || target.Health < 1) ? 2147483647 : target.Health;
            bool inCombat = target == null ? false : target.IsInCombat;
            int targetCount = 0;
            multipleTargets = false;
            foreach (WowUnit unit in wowUnits)
            {
                if (BotUtils.IsValidUnit(unit) && unit != target && !(unit.IsDead || unit.Health < 1 || unit.Auras.Any(e => e.Name.Contains("Spirit of Redem"))))
                {
                    double tmpDistance = WowInterface.ObjectManager.Player.Position.GetDistance(unit.Position);
                    if (tmpDistance < 100.0 || grinding)
                    {
                        if (tmpDistance < 6.0)
                        {
                            targetCount++;
                        }

                        if (((unit.IsInCombat && unit.Health < targetHealth) || (!inCombat && grinding && unit.Health < targetHealth)) && WowInterface.HookManager.WowIsInLineOfSight(WowInterface.ObjectManager.Player.Position, unit.Position))
                        {
                            target = unit;
                            targetHealth = unit.Health;
                            newTargetFound = true;
                            inCombat = unit.IsInCombat;
                        }
                    }
                }
            }

            if (target == null || !(target.IsDead || target.Health < 1 || target.Auras.Any(e => e.Name.Contains("Spirit of Redem"))))
            {
                WowInterface.HookManager.WowClearTarget();
                newTargetFound = false;
                target = null;
            }
            else if (targetCount > 1)
            {
                multipleTargets = true;
            }

            if (newTargetFound)
            {
                WowInterface.HookManager.WowTargetGuid(target.Guid);
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
            private WowInterface WowInterface;

            public WarriorFurySpells(WowInterface wowInterface)
            {
                WowInterface = wowInterface;
                Player = WowInterface.ObjectManager.Player;
                IsInBerserkerStance = false;
                NextGCDSpell = DateTime.Now;
                NextStance = DateTime.Now;
                NextCast = DateTime.Now;
            }


            private bool IsInBerserkerStance { get; set; }

            private DateTime NextCast { get; set; }

            private DateTime NextGCDSpell { get; set; }

            private DateTime NextStance { get; set; }

            private WowPlayer Player { get; set; }

            public void CastNextSpell(double distanceToTarget, WowUnit target, bool multipleTargets)
            {
                if (!IsReady(NextCast) || !IsReady(NextGCDSpell))
                {
                    return;
                }

                if (!WowInterface.ObjectManager.Player.IsAutoAttacking)
                {
                    WowInterface.HookManager.LuaStartAutoAttack();
                }

                Player = WowInterface.ObjectManager.Player;
                int rage = Player.Rage;
                bool lowHealth = Player.HealthPercentage <= 25;
                bool mediumHealth = !lowHealth && Player.HealthPercentage <= 75;
                if (!(lowHealth || mediumHealth))
                {
                    askedForHelp = false;
                    askedForHeal = false;
                }
                else if (lowHealth && !askedForHelp)
                {
                    WowInterface.HookManager.LuaSendChatMessage("/helpme");
                    askedForHelp = true;
                }
                else if (mediumHealth && !askedForHeal)
                {
                    WowInterface.HookManager.LuaSendChatMessage("/healme");
                    askedForHeal = true;
                }

                if (lowHealth && rage > 15 && IsReady(EnragedRegeneration))
                {
                    WowInterface.HookManager.LuaSendChatMessage("/s Oh shit");
                    CastSpell(EnragedRegeneration, ref rage, 15, 180, false);
                }

                if (!lowHealth && rage > 10 && IsReady(DeathWish))
                {
                    CastSpell(DeathWish, ref rage, 10, 120.6, true); // lasts 30 sec
                }
                else if (rage > 10 && IsReady(BattleShout))
                {
                    WowInterface.HookManager.LuaSendChatMessage("/roar");
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

                if (distanceToTarget < (26 + target.CombatReach))
                {
                    if (distanceToTarget < (21 + target.CombatReach))
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
                                        WowInterface.HookManager.LuaSendChatMessage("/incoming");
                                        CastSpell(Charge, ref rage, 0, 15, false);
                                    }
                                }
                            }
                        }
                        else if (distanceToTarget <= 0.75f * (Player.CombatReach + target.CombatReach))
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
                                List<string> buffs = WowInterface.ObjectManager.Player.Auras.Select(e => e.Name).ToList();
                                if (buffs.Any(e => e.Contains("slam") || e.Contains("Slam")) && rage > 15)
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
                                else if (!WowInterface.ObjectManager.Player.IsAutoAttacking)
                                {
                                    WowInterface.HookManager.LuaStartAutoAttack();
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
                nextActionTime[Hamstring].AddSeconds(-15.0);
                nextActionTime[HeroicStrike].AddSeconds(-3.6);
            }

            private void CastSpell(string spell, ref int rage, int rageCosts, double cooldown, bool gcd)
            {
                WowInterface.HookManager.LuaCastSpell(spell);
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
                WowInterface.HookManager.LuaCastSpell(stance);
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

                result &= WowInterface.HookManager.LuaGetSpellCooldown(spell) <= 0 && WowInterface.HookManager.LuaGetUnitCastingInfo(WowLuaUnit.Player).Item2 <= 0;
                return result;
            }

            private int UpdateRage()
            {
                Player = WowInterface.ObjectManager.Player;
                return Player.Rage;
            }
        }
    }
}