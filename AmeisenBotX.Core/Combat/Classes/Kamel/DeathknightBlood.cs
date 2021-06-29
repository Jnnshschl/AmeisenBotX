using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Utils;
using AmeisenBotX.Core.Utils.TargetSelection;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Combat.Classes.Kamel
{
    public class DeathknightBlood : ICombatClass
    {
        public DeathknightBlood(WowInterface wowInterface)
        {
            WowInterface = wowInterface;
            TargetProvider = new TargetManager(new DpsTargetSelectionLogic(wowInterface), TimeSpan.FromMilliseconds(250));//Heal/Tank/DPS
        }

        public string Author => "Kamel";

        public IEnumerable<int> BlacklistedTargetDisplayIds { get; set; }

        public Dictionary<string, dynamic> C { get; set; } = new Dictionary<string, dynamic>();

        public string Description => "FCFS based CombatClass for the Blood Deathknight spec.";

        public string Displayname => "[WIP] Blood Deathknight";

        public bool HandlesFacing => false;

        public bool HandlesMovement => false;

        public bool HandlesTargetSelection => false;

        public bool IsMelee => true;

        public IItemComparator ItemComparator => null;

        public IEnumerable<int> PriorityTargetDisplayIds { get; set; }

        public WowRole Role => WowRole.Dps;

        public TalentTree Talents { get; } = null;

        public bool TargetInLineOfSight { get; set; }

        public ITargetProvider TargetProvider { get; internal set; }

        public string Version => "1.0";

        public bool WalkBehindEnemy => false;

        public WowClass WowClass => WowClass.Deathknight;

        private WowInterface WowInterface { get; }

        public void AttackTarget()
        {
            WowUnit target = WowInterface.Target;
            if (target == null)
            {
                return;
            }

            if (WowInterface.Player.Position.GetDistance(target.Position) <= 3.0)
            {
                WowInterface.NewWowInterface.WowStopClickToMove();
                WowInterface.MovementEngine.Reset();
                WowInterface.NewWowInterface.WowUnitRightClick(target.BaseAddress);
            }
            else
            {
                WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, target.Position);
            }
        }

        public void Execute()
        {
            ulong targetGuid = WowInterface.Objects.Target.Guid;
            WowUnit target = WowInterface.Objects.WowObjects.OfType<WowUnit>().FirstOrDefault(t => t.Guid == targetGuid);
            if (target != null)
            {
                // make sure we're auto attacking
                if (!WowInterface.Objects.Player.IsAutoAttacking)
                {
                    WowInterface.NewWowInterface.LuaStartAutoAttack();
                }

                HandleAttacking(target);
            }
        }

        public bool IsTargetAttackable(WowUnit target)
        {
            return true;
        }

        public void OutOfCombatExecute()
        {
        }

        private void HandleAttacking(WowUnit target)
        {
            if (TargetProvider.Get(out IEnumerable<WowUnit> targetToTarget))
            {
                ulong guid = targetToTarget.First().Guid;

                if (WowInterface.Objects.Player.TargetGuid != guid)
                {
                    WowInterface.NewWowInterface.WowTargetGuid(guid);
                }
            }

            if (WowInterface.Objects.Target == null
                || WowInterface.Objects.Target.IsDead
                || !WowUnit.IsValidUnit(WowInterface.Objects.Target))
            {
                return;
            }

            double playerRunePower = WowInterface.Objects.Player.Runeenergy;
            double distanceToTarget = WowInterface.Objects.Player.Position.GetDistance(target.Position);
            double targetHealthPercent = (target.Health / (double)target.MaxHealth) * 100;
            double playerHealthPercent = (WowInterface.Objects.Player.Health / (double)WowInterface.Objects.Player.MaxHealth) * 100.0;
            (string, int) targetCastingInfo = WowInterface.NewWowInterface.LuaGetUnitCastingInfo(WowLuaUnit.Target);
            //List<string> myBuffs = WowInterface.NewWowInterface.GetBuffs(WowLuaUnit.Player.ToString());
            //myBuffs.Any(e => e.Equals("Chains of Ice"))

            if (WowInterface.NewWowInterface.LuaGetSpellCooldown("Death Grip") <= 0 && distanceToTarget <= 30)
            {
                WowInterface.NewWowInterface.LuaCastSpell("Death Grip");
                return;
            }
            if (target.IsFleeing && distanceToTarget <= 30)
            {
                WowInterface.NewWowInterface.LuaCastSpell("Chains of Ice");
                return;
            }

            if (WowInterface.NewWowInterface.LuaGetSpellCooldown("Army of the Dead") <= 0 &&
                IsOneOfAllRunesReady())
            {
                WowInterface.NewWowInterface.LuaCastSpell("Army of the Dead");
                return;
            }

            List<WowUnit> unitsNearPlayer = WowInterface.Objects.WowObjects
                .OfType<WowUnit>()
                .Where(e => e.Position.GetDistance(WowInterface.Objects.Player.Position) <= 10)
                .ToList();

            if (unitsNearPlayer.Count > 2 &&
                WowInterface.NewWowInterface.LuaGetSpellCooldown("Blood Boil") <= 0 &&
                WowInterface.NewWowInterface.WowIsRuneReady(0) ||
                WowInterface.NewWowInterface.WowIsRuneReady(1))
            {
                WowInterface.NewWowInterface.LuaCastSpell("Blood Boil");
                return;
            }

            List<WowUnit> unitsNearTarget = WowInterface.Objects.WowObjects
                .OfType<WowUnit>()
                .Where(e => e.Position.GetDistance(target.Position) <= 30)
                .ToList();

            if (unitsNearTarget.Count > 2 &&
                WowInterface.NewWowInterface.LuaGetSpellCooldown("Death and Decay") <= 0 &&
                IsOneOfAllRunesReady())
            {
                WowInterface.NewWowInterface.LuaCastSpell("Death and Decay");
                WowInterface.NewWowInterface.WowClickOnTerrain(target.Position);
                return;
            }

            if (WowInterface.NewWowInterface.LuaGetSpellCooldown("Icy Touch") <= 0 &&
                WowInterface.NewWowInterface.WowIsRuneReady(2) ||
                WowInterface.NewWowInterface.WowIsRuneReady(3))
            {
                WowInterface.NewWowInterface.LuaCastSpell("Icy Touch");
                return;
            }
        }

        private bool IsOneOfAllRunesReady()
        {
            return WowInterface.NewWowInterface.WowIsRuneReady(0)
                       || WowInterface.NewWowInterface.WowIsRuneReady(1)
                       && WowInterface.NewWowInterface.WowIsRuneReady(2)
                       || WowInterface.NewWowInterface.WowIsRuneReady(3)
                       && WowInterface.NewWowInterface.WowIsRuneReady(4)
                       || WowInterface.NewWowInterface.WowIsRuneReady(5);
        }
    }
}