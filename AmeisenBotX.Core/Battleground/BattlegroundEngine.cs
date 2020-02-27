using AmeisenBotX.Core.Battleground.Profiles;
using AmeisenBotX.Core.Battleground.States;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.Movement;
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
        }

        public IBattlegroundProfile BattlegroundProfile { get; private set; }

        public KeyValuePair<BattlegroundState, BasicBattlegroundState> CurrentState { get; private set; }

        public BattlegroundState LastState { get; private set; }

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

        public void Execute()
        {
            if (BattlegroundProfile == null)
            {
                TryLoadProfile(ObjectManager.MapId);
                CurrentState = BattlegroundProfile.States.First();
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

        internal IEnumerable<WowGameobject> GetBattlegroundFlags()
        {
            IEnumerable<WowGameobject> flagObjects = ObjectManager.WowObjects
                .OfType<WowGameobject>()
                .Where(e => e.GameobjectType == WowGameobjectType.Flagdrop || e.GameobjectType == WowGameobjectType.Flagstand);

            return flagObjects;
        }

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