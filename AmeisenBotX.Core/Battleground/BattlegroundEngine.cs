using AmeisenBotX.Core.Battleground.Profiles;
using AmeisenBotX.Core.Battleground.States;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.Movement;
using AmeisenBotX.Core.Movement.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Battleground
{
    public class BattlegroundEngine
    {
        public BattlegroundEngine(HookManager hookManager, ObjectManager objectManager, IMovementEngine movementEngine)
        {
            HookManager = hookManager;
            ObjectManager = objectManager;
            MovementEngine = movementEngine;

            ForceCombat = false;
        }

        public IBattlegroundProfile BattlegroundProfile { get; private set; }

        public KeyValuePair<BattlegroundState, BasicBattlegroundState> CurrentState { get; private set; }

        public BattlegroundState LastState { get; private set; }

        public bool ForceCombat { get; internal set; }

        private HookManager HookManager { get; set; }

        private IMovementEngine MovementEngine { get; set; }

        private ObjectManager ObjectManager { get; set; }

        public void AllianceFlagWasDropped(string playername)
        {
            if (BattlegroundProfile.GetType() == typeof(WarsongGulchProfile))
            {
                ((WarsongGulchProfile)BattlegroundProfile).AllianceFlagWasDropped(playername);
            }
        }

        public void AllianceFlagWasPickedUp(string playername)
        {
            if (BattlegroundProfile.GetType() == typeof(WarsongGulchProfile))
            {
                ((WarsongGulchProfile)BattlegroundProfile).AllianceFlagWasPickedUp(playername);
            }
        }

        public bool AttackNearEnemies(double range = 25)
        {
            IEnumerable<WowPlayer> nearEnemies = ObjectManager.GetNearEnemies(ObjectManager.Player.Position, range);

            if (nearEnemies.Count() > 0)
            {
                WowPlayer target = nearEnemies.FirstOrDefault(e => BotUtils.IsValidUnit(e));

                if (target != null)
                {
                    if (ObjectManager.Player.Position.GetDistance(target.Position) > 10)
                    {
                        MovementEngine.SetState(MovementEngineState.Moving, target.Position);
                        MovementEngine.Execute();
                        return true;
                    }
                    else
                    {
                        HookManager.TargetGuid(target.Guid);
                        HookManager.StartAutoAttack();
                        return true;
                    }
                }
            }

            return false;
        }

        public void Execute()
        {
            List<WowGameobject> flagObjects = ObjectManager.WowObjects
                .OfType<WowGameobject>()
                .OrderBy(e => e.Position.GetDistance(ObjectManager.Player.Position)).ToList();

            if (BattlegroundProfile == null)
            {
                TryLoadProfile(ObjectManager.MapId);

                if (BattlegroundProfile != null)
                {
                    CurrentState = BattlegroundProfile.States.First();
                }
            }
            else
            {
                CurrentState.Value.Execute();
            }
        }

        public void HordeFlagWasDropped(string playername)
        {
            if (BattlegroundProfile.GetType() == typeof(WarsongGulchProfile))
            {
                ((WarsongGulchProfile)BattlegroundProfile).HordeFlagWasDropped(playername);
            }
        }

        public void HordeFlagWasPickedUp(string playername)
        {
            if (BattlegroundProfile.GetType() == typeof(WarsongGulchProfile))
            {
                ((WarsongGulchProfile)BattlegroundProfile).HordeFlagWasPickedUp(playername);
            }
        }

        public void Reset()
        {
            BattlegroundProfile = null;
        }

        public bool TryLoadProfile(int mapId)
        {
            switch (mapId)
            {
                case 30:
                    // Alterac Valley
                    return false;

                case 489:
                    BattlegroundProfile = new WarsongGulchProfile(ObjectManager.Player.IsAlliance(), HookManager, ObjectManager, MovementEngine, this);
                    return true;

                case 529:
                    // Arathi Basin
                    return false;

                case 566:
                    // Eye of the Storm
                    return false;

                case 607:
                    // Strand of the Ancients
                    return false;

                default:
                    return false;
            }
        }

        internal IEnumerable<WowGameobject> GetBattlegroundFlags(bool onlyEnemy = true)
            => ObjectManager.WowObjects
                .OfType<WowGameobject>()
                // 5912 Alliance Flag / 5913 Horde Flag
                .Where(e => onlyEnemy ? (!ObjectManager.Player.IsAlliance() && e.DisplayId == 5912) || (ObjectManager.Player.IsAlliance() && e.DisplayId == 5913) : e.DisplayId == 5912 || e.DisplayId == 5913);

        internal void SetState(BattlegroundState state)
        {
            if (BattlegroundProfile == null || CurrentState.Key == state)
            {
                // we are already in this state
                return;
            }

            LastState = CurrentState.Key;
            CurrentState.Value.Exit();
            CurrentState = BattlegroundProfile.States.First(s => s.Key == state);
            CurrentState.Value.Enter();
        }
    }
}