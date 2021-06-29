﻿using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Fsm.Enums;

namespace AmeisenBotX.Core.Fsm.States
{
    public class StateDead : BasicState
    {
        public StateDead(AmeisenBotFsm stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine, config, wowInterface)
        {
        }

        public bool SetMapAndPosition { get; set; }

        public override void Enter()
        {
            WowInterface.MovementEngine.StopMovement();
        }

        public override void Execute()
        {
            if (WowInterface.Player.IsDead)
            {
                if (!SetMapAndPosition) // prevent re-setting the stuff in loading screen
                {
                    SetMapAndPosition = true;
                    StateMachine.LastDiedMap = WowInterface.Objects.MapId;

                    if (StateMachine.LastDiedMap.IsDungeonMap())
                    {
                        // when we died in a dungeon, we need to return to its portal
                        StateMachine.LastDiedPosition = WowInterface.DungeonEngine.Profile.WorldEntry;
                    }
                    else
                    {
                        StateMachine.LastDiedPosition = WowInterface.Player.Position;
                    }
                }

                if (Config.ReleaseSpirit || WowInterface.Objects.MapId.IsBattlegroundMap())
                {
                    WowInterface.NewWowInterface.LuaRepopMe();
                }
            }
            else if (WowInterface.Player.IsGhost)
            {
                StateMachine.SetState(BotState.Ghost);
            }
            else
            {
                StateMachine.SetState(BotState.Idle);
            }
        }

        public override void Leave()
        {
            SetMapAndPosition = false;
        }
    }
}