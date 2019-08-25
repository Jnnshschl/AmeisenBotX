using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.OffsetLists;
using AmeisenBotX.Pathfinding;
using System;
using System.Collections.Generic;

namespace AmeisenBotX.Core.StateMachine.States
{
    public class StateGhost : State
    {
        public StateGhost(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, IOffsetList offsetList, ObjectManager objectManager, CharacterManager characterManager, HookManager hookManager, IPathfindingHandler pathfindingHandler) : base(stateMachine)
        {
            Config = config;
            ObjectManager = objectManager;
            CharacterManager = characterManager;
            HookManager = hookManager;
            OffsetList = offsetList;
            PathfindingHandler = pathfindingHandler;
            CurrentPath = new Queue<WowPosition>();
        }

        private CharacterManager CharacterManager { get; }
        private AmeisenBotConfig Config { get; }
        private Queue<WowPosition> CurrentPath { get; set; }
        private HookManager HookManager { get; }
        private WowPosition LastPosition { get; set; }
        private ObjectManager ObjectManager { get; }
        private IOffsetList OffsetList { get; }
        private IPathfindingHandler PathfindingHandler { get; }
        private int TryCount { get; set; }

        public override void Enter()
        {
            CurrentPath.Clear();
            TryCount = 0;
        }

        public override void Execute()
        {
            if (ObjectManager.Player.Health > 1)
                AmeisenBotStateMachine.SetState(AmeisenBotState.Idle);

            if (AmeisenBotStateMachine.XMemory.ReadStruct(OffsetList.CorpsePosition, out WowPosition corpsePosition)
                && ObjectManager.Player.Position.GetDistance(corpsePosition) > 5)
            {
                if (CurrentPath.Count == 0)
                {
                    BuildNewPath(corpsePosition);
                }
                else
                {
                    WowPosition pos = CurrentPath.Peek();
                    double distance = pos.GetDistance2D(ObjectManager.Player.Position);
                    double distTraveled = LastPosition.GetDistance2D(ObjectManager.Player.Position);

                    if (distance <= 2
                        || TryCount > 5)
                    {
                        CurrentPath.Dequeue();
                        TryCount = 0;
                    }
                    else
                    {
                        CharacterManager.MoveToPosition(pos);

                        if (distTraveled != 0 && distTraveled < 0.08)
                            TryCount++;

                        // if the thing is too far away, drop the whole Path
                        if (pos.Z - ObjectManager.Player.Position.Z > 2
                            && distance > 2)
                            CurrentPath.Clear();

                        // jump if the node is higher than us
                        if (pos.Z - ObjectManager.Player.Position.Z > 1.2
                            && distance < 3)
                            CharacterManager.Jump();
                    }

                    if (distTraveled != 0
                        && distTraveled < 0.08)
                    {
                        // go forward
                        BotUtils.SendKey(AmeisenBotStateMachine.XMemory.Process.MainWindowHandle, new IntPtr(0x26), 500, 750);
                        CharacterManager.Jump();
                    }

                    LastPosition = ObjectManager.Player.Position;
                }
            }
            else
            {
                HookManager.RetrieveCorpse();
            }
        }

        public override void Exit()
        {
        }

        private void BuildNewPath(WowPosition corpsePosition)
        {
            List<WowPosition> path = PathfindingHandler.GetPath(ObjectManager.MapId, ObjectManager.Player.Position, corpsePosition);
            if (path.Count > 0)
                foreach (WowPosition pos in path)
                    CurrentPath.Enqueue(pos);
        }
    }
}