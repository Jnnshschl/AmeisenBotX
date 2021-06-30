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
        public DeathknightBlood(AmeisenBotInterfaces bot)
        {
            Bot = bot;
            TargetProvider = new TargetManager(new DpsTargetSelectionLogic(bot), TimeSpan.FromMilliseconds(250));//Heal/Tank/DPS
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

        private AmeisenBotInterfaces Bot { get; }

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
            ulong targetGuid = Bot.Wow.TargetGuid;
            WowUnit target = Bot.Objects.WowObjects.OfType<WowUnit>().FirstOrDefault(t => t.Guid == targetGuid);
            if (target != null)
            {
                // make sure we're auto attacking
                if (!Bot.Objects.Player.IsAutoAttacking)
                {
                    Bot.Wow.LuaStartAutoAttack();
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

                if (Bot.Objects.Player.TargetGuid != guid)
                {
                    Bot.Wow.WowTargetGuid(guid);
                }
            }

            if (Bot.Objects.Target == null
                || Bot.Objects.Target.IsDead
                || !WowUnit.IsValidUnit(Bot.Objects.Target))
            {
                return;
            }

            double playerRunePower = Bot.Objects.Player.Runeenergy;
            double distanceToTarget = Bot.Objects.Player.Position.GetDistance(target.Position);
            double targetHealthPercent = (target.Health / (double)target.MaxHealth) * 100;
            double playerHealthPercent = (Bot.Objects.Player.Health / (double)Bot.Objects.Player.MaxHealth) * 100.0;
            (string, int) targetCastingInfo = Bot.Wow.LuaGetUnitCastingInfo(WowLuaUnit.Target);
            //List<string> myBuffs = Bot.NewBot.GetBuffs(WowLuaUnit.Player.ToString());
            //myBuffs.Any(e => e.Equals("Chains of Ice"))

            if (Bot.Wow.LuaGetSpellCooldown("Death Grip") <= 0 && distanceToTarget <= 30)
            {
                Bot.Wow.LuaCastSpell("Death Grip");
                return;
            }
            if (target.IsFleeing && distanceToTarget <= 30)
            {
                Bot.Wow.LuaCastSpell("Chains of Ice");
                return;
            }

            if (Bot.Wow.LuaGetSpellCooldown("Army of the Dead") <= 0 &&
                IsOneOfAllRunesReady())
            {
                Bot.Wow.LuaCastSpell("Army of the Dead");
                return;
            }

            List<WowUnit> unitsNearPlayer = Bot.Objects.WowObjects
                .OfType<WowUnit>()
                .Where(e => e.Position.GetDistance(Bot.Objects.Player.Position) <= 10)
                .ToList();

            if (unitsNearPlayer.Count > 2 &&
                Bot.Wow.LuaGetSpellCooldown("Blood Boil") <= 0 &&
                Bot.Wow.WowIsRuneReady(0) ||
                Bot.Wow.WowIsRuneReady(1))
            {
                Bot.Wow.LuaCastSpell("Blood Boil");
                return;
            }

            List<WowUnit> unitsNearTarget = Bot.Objects.WowObjects
                .OfType<WowUnit>()
                .Where(e => e.Position.GetDistance(target.Position) <= 30)
                .ToList();

            if (unitsNearTarget.Count > 2 &&
                Bot.Wow.LuaGetSpellCooldown("Death and Decay") <= 0 &&
                IsOneOfAllRunesReady())
            {
                Bot.Wow.LuaCastSpell("Death and Decay");
                Bot.Wow.WowClickOnTerrain(target.Position);
                return;
            }

            if (Bot.Wow.LuaGetSpellCooldown("Icy Touch") <= 0 &&
                Bot.Wow.WowIsRuneReady(2) ||
                Bot.Wow.WowIsRuneReady(3))
            {
                Bot.Wow.LuaCastSpell("Icy Touch");
                return;
            }
        }

        private bool IsOneOfAllRunesReady()
        {
            return Bot.Wow.WowIsRuneReady(0)
                       || Bot.Wow.WowIsRuneReady(1)
                       && Bot.Wow.WowIsRuneReady(2)
                       || Bot.Wow.WowIsRuneReady(3)
                       && Bot.Wow.WowIsRuneReady(4)
                       || Bot.Wow.WowIsRuneReady(5);
        }
    }
}