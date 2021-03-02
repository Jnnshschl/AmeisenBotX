using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Utils;
using AmeisenBotX.Core.Utils.TargetSelection;
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
            ObjectManager = wowInterface.ObjectManager;
            HookManager = wowInterface.HookManager;
            TargetManager = new TargetManager(new DpsTargetSelectionLogic(wowInterface), TimeSpan.FromMilliseconds(250));//Heal/Tank/DPS
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

        public TargetManager TargetManager { get; internal set; }

        public string Version => "1.0";

        public bool WalkBehindEnemy => false;

        public WowClass WowClass => WowClass.Deathknight;

        private IHookManager HookManager { get; }

        private IObjectManager ObjectManager { get; }

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
                WowInterface.HookManager.WowStopClickToMove();
                WowInterface.MovementEngine.Reset();
                WowInterface.HookManager.WowUnitRightClick(target);
            }
            else
            {
                WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, target.Position);
            }
        }

        public void Execute()
        {
            ulong targetGuid = ObjectManager.TargetGuid;
            WowUnit target = ObjectManager.WowObjects.OfType<WowUnit>().FirstOrDefault(t => t.Guid == targetGuid);
            if (target != null)
            {
                // make sure we're auto attacking
                if (!ObjectManager.Player.IsAutoAttacking)
                {
                    HookManager.LuaStartAutoAttack();
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
            if (TargetManager.GetUnitToTarget(out IEnumerable<WowUnit> targetToTarget))
            {
                ulong guid = targetToTarget.First().Guid;

                if (ObjectManager.Player.TargetGuid != guid)
                {
                    HookManager.WowTargetGuid(guid);
                }
            }

            if (ObjectManager.Target == null
                || ObjectManager.Target.IsDead
                || !BotUtils.IsValidUnit(ObjectManager.Target))
            {
                return;
            }

            double playerRunePower = ObjectManager.Player.Runeenergy;
            double distanceToTarget = ObjectManager.Player.Position.GetDistance(target.Position);
            double targetHealthPercent = (target.Health / (double)target.MaxHealth) * 100;
            double playerHealthPercent = (ObjectManager.Player.Health / (double)ObjectManager.Player.MaxHealth) * 100.0;
            (string, int) targetCastingInfo = HookManager.LuaGetUnitCastingInfo(WowLuaUnit.Target);
            //List<string> myBuffs = HookManager.GetBuffs(WowLuaUnit.Player.ToString());
            //myBuffs.Any(e => e.Equals("Chains of Ice"))

            if (HookManager.LuaGetSpellCooldown("Death Grip") <= 0 && distanceToTarget <= 30)
            {
                HookManager.LuaCastSpell("Death Grip");
                return;
            }
            if (target.IsFleeing && distanceToTarget <= 30)
            {
                HookManager.LuaCastSpell("Chains of Ice");
                return;
            }

            if (HookManager.LuaGetSpellCooldown("Army of the Dead") <= 0 &&
                IsOneOfAllRunesReady())
            {
                HookManager.LuaCastSpell("Army of the Dead");
                return;
            }

            List<WowUnit> unitsNearPlayer = ObjectManager.WowObjects
                .OfType<WowUnit>()
                .Where(e => e.Position.GetDistance(ObjectManager.Player.Position) <= 10)
                .ToList();

            if (unitsNearPlayer.Count > 2 &&
                HookManager.LuaGetSpellCooldown("Blood Boil") <= 0 &&
                HookManager.WowIsRuneReady(0) ||
                HookManager.WowIsRuneReady(1))
            {
                HookManager.LuaCastSpell("Blood Boil");
                return;
            }

            List<WowUnit> unitsNearTarget = ObjectManager.WowObjects
                .OfType<WowUnit>()
                .Where(e => e.Position.GetDistance(target.Position) <= 30)
                .ToList();

            if (unitsNearTarget.Count > 2 &&
                HookManager.LuaGetSpellCooldown("Death and Decay") <= 0 &&
                IsOneOfAllRunesReady())
            {
                HookManager.LuaCastSpell("Death and Decay");
                HookManager.WowClickOnTerrain(target.Position);
                return;
            }

            if (HookManager.LuaGetSpellCooldown("Icy Touch") <= 0 &&
                HookManager.WowIsRuneReady(2) ||
                HookManager.WowIsRuneReady(3))
            {
                HookManager.LuaCastSpell("Icy Touch");
                return;
            }
        }

        private bool IsOneOfAllRunesReady()
        {
            return HookManager.WowIsRuneReady(0)
                       || HookManager.WowIsRuneReady(1)
                       && HookManager.WowIsRuneReady(2)
                       || HookManager.WowIsRuneReady(3)
                       && HookManager.WowIsRuneReady(4)
                       || HookManager.WowIsRuneReady(5);
        }
    }
}