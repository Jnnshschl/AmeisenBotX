using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Engines.Character.Comparators;
using AmeisenBotX.Core.Engines.Character.Talents.Objects;
using AmeisenBotX.Core.Engines.Combat.Helpers.Targets;
using AmeisenBotX.Core.Engines.Combat.Helpers.Targets.Logics;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace AmeisenBotX.Core.Engines.Combat.Classes.Kamel
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

        public Dictionary<string, dynamic> ConfigurableThresholds { get; set; } = new Dictionary<string, dynamic>();

        public string Description => "FCFS based CombatClass for the Blood Deathknight spec.";

        public string DisplayName => "[WIP] Blood Deathknight";

        public bool HandlesFacing => false;

        public bool HandlesMovement => false;

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
            IWowUnit target = Bot.Target;
            if (target == null)
            {
                return;
            }

            if (Bot.Player.Position.GetDistance(target.Position) <= 3.0)
            {
                Bot.Wow.StopClickToMove();
                Bot.Movement.Reset();
                Bot.Wow.InteractWithUnit(target.BaseAddress);
            }
            else
            {
                Bot.Movement.SetMovementAction(MovementAction.Move, target.Position);
            }
        }

        public void Execute()
        {
            ulong targetGuid = Bot.Wow.TargetGuid;
            IWowUnit target = Bot.Objects.WowObjects.OfType<IWowUnit>().FirstOrDefault(t => t.Guid == targetGuid);
            if (target != null)
            {
                // make sure we're auto attacking
                if (!Bot.Objects.Player.IsAutoAttacking)
                {
                    Bot.Wow.StartAutoAttack();
                }

                HandleAttacking(target);
            }
        }

        public void Load(Dictionary<string, JsonElement> objects)
        { 
            ConfigurableThresholds = objects["Configureables"].ToDyn();
        }

        public void OutOfCombatExecute()
        {
        }

        public Dictionary<string, object> Save()
        {
            return new()
            {
                { "configureables", ConfigurableThresholds }
            };
        }

        private void HandleAttacking(IWowUnit target)
        {
            if (TargetProvider.Get(out IEnumerable<IWowUnit> targetToTarget))
            {
                ulong guid = targetToTarget.First().Guid;

                if (Bot.Objects.Player.TargetGuid != guid)
                {
                    Bot.Wow.ChangeTarget(guid);
                }
            }

            if (Bot.Objects.Target == null
                || Bot.Objects.Target.IsDead
                || !IWowUnit.IsValidUnit(Bot.Objects.Target))
            {
                return;
            }

            double playerRunePower = Bot.Objects.Player.Runeenergy;
            double distanceToTarget = Bot.Objects.Player.Position.GetDistance(target.Position);
            double targetHealthPercent = (target.Health / (double)target.MaxHealth) * 100;
            double playerHealthPercent = (Bot.Objects.Player.Health / (double)Bot.Objects.Player.MaxHealth) * 100.0;
            (string, int) targetCastingInfo = Bot.Wow.GetUnitCastingInfo(WowLuaUnit.Target);
            //List<string> myBuffs = Bot.NewBot.GetBuffs(WowLuaUnit.Player.ToString());
            //myBuffs.Any(e => e.Equals("Chains of Ice"))

            if (Bot.Wow.GetSpellCooldown("Death Grip") <= 0 && distanceToTarget <= 30)
            {
                Bot.Wow.CastSpell("Death Grip");
                return;
            }
            if (target.IsFleeing && distanceToTarget <= 30)
            {
                Bot.Wow.CastSpell("Chains of Ice");
                return;
            }

            if (Bot.Wow.GetSpellCooldown("Army of the Dead") <= 0 &&
                IsOneOfAllRunesReady())
            {
                Bot.Wow.CastSpell("Army of the Dead");
                return;
            }

            List<IWowUnit> unitsNearPlayer = Bot.Objects.WowObjects
                .OfType<IWowUnit>()
                .Where(e => e.Position.GetDistance(Bot.Objects.Player.Position) <= 10)
                .ToList();

            if (unitsNearPlayer.Count > 2 &&
                Bot.Wow.GetSpellCooldown("Blood Boil") <= 0 &&
                Bot.Wow.IsRuneReady(0) ||
                Bot.Wow.IsRuneReady(1))
            {
                Bot.Wow.CastSpell("Blood Boil");
                return;
            }

            List<IWowUnit> unitsNearTarget = Bot.Objects.WowObjects
                .OfType<IWowUnit>()
                .Where(e => e.Position.GetDistance(target.Position) <= 30)
                .ToList();

            if (unitsNearTarget.Count > 2 &&
                Bot.Wow.GetSpellCooldown("Death and Decay") <= 0 &&
                IsOneOfAllRunesReady())
            {
                Bot.Wow.CastSpell("Death and Decay");
                Bot.Wow.ClickOnTerrain(target.Position);
                return;
            }

            if (Bot.Wow.GetSpellCooldown("Icy Touch") <= 0 &&
                Bot.Wow.IsRuneReady(2) ||
                Bot.Wow.IsRuneReady(3))
            {
                Bot.Wow.CastSpell("Icy Touch");
                return;
            }
        }

        private bool IsOneOfAllRunesReady()
        {
            return Bot.Wow.IsRuneReady(0)
                       || Bot.Wow.IsRuneReady(1)
                       && Bot.Wow.IsRuneReady(2)
                       || Bot.Wow.IsRuneReady(3)
                       && Bot.Wow.IsRuneReady(4)
                       || Bot.Wow.IsRuneReady(5);
        }
    }
}