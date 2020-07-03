using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.Statemachine.Enums;
using AmeisenBotX.Core.Statemachine.Utils;
using AmeisenBotX.Core.Statemachine.Utils.TargetSelectionLogic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Statemachine.CombatClasses.Kamel
{
    public class DeathknightBlood : ICombatClass
    {
        public DeathknightBlood(WowInterface wowInterface)
        {
            ObjectManager = wowInterface.ObjectManager;
            HookManager = wowInterface.HookManager;
            TargetManager = new TargetManager(new DpsTargetSelectionLogic(wowInterface), TimeSpan.FromMilliseconds(250));//Heal/Tank/DPS
        }

        public string Author => "Kamel";

        public WowClass Class => WowClass.Deathknight;

        public Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public string Description => "FCFS based CombatClass for the Blood Deathknight spec.";

        public string Displayname => "[WIP] Blood Deathknight";

        public bool HandlesMovement => false;

        public bool HandlesTargetSelection => false;

        public bool IsMelee => true;

        public IWowItemComparator ItemComparator => null;

        public List<string> PriorityTargets { get; set; }

        public CombatClassRole Role => CombatClassRole.Dps;

        public TalentTree Talents { get; } = null;

        public TargetManager TargetManager { get; internal set; }

        public string Version => "1.0";

        public bool WalkBehindEnemy => false;

        private IHookManager HookManager { get; }

        private IObjectManager ObjectManager { get; }

        public bool TargetInLineOfSight { get; set; }

        public void Execute()
        {
            ulong targetGuid = ObjectManager.TargetGuid;
            WowUnit target = ObjectManager.WowObjects.OfType<WowUnit>().FirstOrDefault(t => t.Guid == targetGuid);
            if (target != null)
            {
                // make sure we're auto attacking
                if (!ObjectManager.Player.IsAutoAttacking)
                {
                    HookManager.StartAutoAttack(ObjectManager.Target);
                }

                HandleAttacking(target);
            }
        }

        public void OutOfCombatExecute()
        {
        }

        private void HandleAttacking(WowUnit target)
        {
            if (TargetManager.GetUnitToTarget(out List<WowUnit> targetToTarget))
            {
                ulong guid = targetToTarget.First().Guid;

                if (ObjectManager.Player.TargetGuid != guid)
                {
                    HookManager.TargetGuid(guid);
                    ObjectManager.UpdateObject(ObjectManager.Player);
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
            (string, int) targetCastingInfo = HookManager.GetUnitCastingInfo(WowLuaUnit.Target);
            //List<string> myBuffs = HookManager.GetBuffs(WowLuaUnit.Player.ToString());
            //myBuffs.Any(e => e.Equals("Chains of Ice"))

            if (HookManager.GetSpellCooldown("Death Grip") <= 0 && distanceToTarget <= 30)
            {
                HookManager.CastSpell("Death Grip");
                return;
            }
            if (target.IsFleeing && distanceToTarget <= 30)
            {
                HookManager.CastSpell("Chains of Ice");
                return;
            }

            if (HookManager.GetSpellCooldown("Army of the Dead") <= 0 &&
                IsOneOfAllRunesReady())
            {
                HookManager.CastSpell("Army of the Dead");
                return;
            }

            List<WowUnit> unitsNearPlayer = ObjectManager.WowObjects
                .OfType<WowUnit>()
                .Where(e => e.Position.GetDistance(ObjectManager.Player.Position) <= 10)
                .ToList();

            if (unitsNearPlayer.Count > 2 &&
                HookManager.GetSpellCooldown("Blood Boil") <= 0 &&
                HookManager.IsRuneReady(0) ||
                HookManager.IsRuneReady(1))
            {
                HookManager.CastSpell("Blood Boil");
                return;
            }

            List<WowUnit> unitsNearTarget = ObjectManager.WowObjects
                .OfType<WowUnit>()
                .Where(e => e.Position.GetDistance(target.Position) <= 30)
                .ToList();

            if (unitsNearTarget.Count > 2 &&
                HookManager.GetSpellCooldown("Death and Decay") <= 0 &&
                IsOneOfAllRunesReady())
            {
                HookManager.CastSpell("Death and Decay");
                HookManager.ClickOnTerrain(target.Position);
                return;
            }

            if (HookManager.GetSpellCooldown("Icy Touch") <= 0 &&
                HookManager.IsRuneReady(2) ||
                HookManager.IsRuneReady(3))
            {
                HookManager.CastSpell("Icy Touch");
                return;
            }
        }

        private bool IsOneOfAllRunesReady()
        {
            return HookManager.IsRuneReady(0)
                       || HookManager.IsRuneReady(1)
                       && HookManager.IsRuneReady(2)
                       || HookManager.IsRuneReady(3)
                       && HookManager.IsRuneReady(4)
                       || HookManager.IsRuneReady(5);
        }
    }
}