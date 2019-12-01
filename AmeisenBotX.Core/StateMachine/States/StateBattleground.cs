using AmeisenBotX.Core.Battleground;
using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Event;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.Movement;
using AmeisenBotX.Core.OffsetLists;
using AmeisenBotX.Core.StateMachine.CombatClasses;
using AmeisenBotX.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.StateMachine.States
{
    public class StateBattleground : State
    {
        public StateBattleground(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, IOffsetList offsetList, ObjectManager objectManager, CharacterManager characterManager, HookManager hookManager, IMovementEngine movementEngine) : base(stateMachine)
        {
            Config = config;
            OffsetList = offsetList;
            ObjectManager = objectManager;
            HookManager = hookManager;
            CharacterManager = characterManager;
            MovementEngine = movementEngine;
            BattlegroundEngine = new BattlegroundEngine(hookManager, objectManager, movementEngine);
        }

        private CharacterManager CharacterManager { get; }

        private AmeisenBotConfig Config { get; }

        private HookManager HookManager { get; }

        private ObjectManager ObjectManager { get; }

        private IOffsetList OffsetList { get; }

        private IMovementEngine MovementEngine { get; }

        private BattlegroundEngine BattlegroundEngine { get; }

        public override void Enter()
        {

        }

        public override void Execute()
        {
            if (AmeisenBotStateMachine.XMemory.Read(OffsetList.BattlegroundStatus, out int bgStatus)
                && bgStatus == 0)
            {
                AmeisenBotStateMachine.SetState(AmeisenBotState.Idle);
                return;
            }

            if (AmeisenBotStateMachine.XMemory.Read(OffsetList.BattlegroundFinished, out int bgFinished)
                && bgFinished == 1)
            {
                HookManager.LeaveBattleground();
                return;
            }

            BattlegroundEngine.Execute();
        }

        public override void Exit()
        {

        }
    }
}
