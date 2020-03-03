using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Dungeon.Objects;
using AmeisenBotX.Core.Jobs.Profiles;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Statemachine;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Dungeon
{
    public class DungeonEngine
    {
        public DungeonEngine(WowInterface wowInterface, AmeisenBotStateMachine stateMachine)
        {
            WowInterface = wowInterface;
            StateMachine = stateMachine;

            CurrentNodes = new Queue<DungeonNode>();
            CompletedNodes = new List<DungeonNode>();

            Reset();
        }

        public bool AllPlayersArrived { get; private set; }

        public Queue<DungeonNode> CurrentNodes { get; private set; }

        public IDungeonProfile DungeonProfile { get; private set; }

        public bool HasFinishedDungeon { get; private set; }

        public double Progress { get; private set; }

        public int TotalNodes { get; private set; }

        private List<DungeonNode> CompletedNodes { get; set; }

        private AmeisenBotStateMachine StateMachine { get; }

        private WowInterface WowInterface { get; }

        public void Execute()
        {
            if (DungeonProfile != null)
            {
                // we are fighting
                if (WowInterface.ObjectManager.Player.IsInCombat && StateMachine.IsAnyPartymemberInCombat())
                {
                    return;
                }

                if (Progress == 100.0)
                {
                    // exit dungeon
                    HasFinishedDungeon = true;
                    return;
                }

                // wait for all players to arrive
                if (!AllPlayersArrived)
                {
                    AllPlayersArrived = AreAllPlayersPresent();
                }
                else
                {
                    if (!ShouldWaitForGroup())
                    {
                        if (WowInterface.ObjectManager.Player.Position.GetDistance(CurrentNodes.Peek().Position) > 6)
                        {
                            WowInterface.MovementEngine.SetState(MovementEngineState.Moving, CurrentNodes.Peek().Position);
                            WowInterface.MovementEngine.Execute();
                            return;
                        }
                        else
                        {
                            CompletedNodes.Add(CurrentNodes.Dequeue());
                            Progress = (CompletedNodes.Count / TotalNodes) * 100;
                        }
                    }
                }
            }
        }

        public void LoadProfile(IDungeonProfile profile)
        {
            Reset();

            DungeonProfile = profile;
            foreach (DungeonNode d in DungeonProfile.Path)
            {
                CurrentNodes.Enqueue(d);
            }

            TotalNodes = CurrentNodes.Count;
        }

        public void Reset()
        {
            HasFinishedDungeon = false;
            AllPlayersArrived = false;
            DungeonProfile = null;
            CurrentNodes.Clear();
            Progress = 0.0;
            TotalNodes = 0;
        }

        private bool AreAllPlayersPresent()
            => WowInterface.ObjectManager.GetNearFriends(WowInterface.ObjectManager.Player.Position, 50).Count() >= DungeonProfile.GroupSize;

        private bool ShouldWaitForGroup()
        {
            // we need to be prepared for the bossfight
            double minPercentages = CurrentNodes.Peek().Type == Enums.DungeonNodeType.Boss ? 100.0 : 75.0;

            // do we need to wait for some members to regen life or mana
            if (WowInterface.ObjectManager.Partymembers.OfType<WowUnit>().Any(e => e.HealthPercentage < minPercentages || e.ManaPercentage < minPercentages))
            {
                return true;
            }

            // are my group members not in range of the CurrentNode
            if (WowInterface.ObjectManager.GetNearFriends(CurrentNodes.Peek().Position, 30).Count() < DungeonProfile.GroupSize)
            {
                return true;
            }

            // go ahead
            return false;
        }
    }
}