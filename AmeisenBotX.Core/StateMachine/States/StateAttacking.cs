using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Hook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.StateMachine.States
{
    class StateAttacking : State
    {
        private AmeisenBotConfig Config { get; }

        private ObjectManager ObjectManager { get; }
        private HookManager HookManager { get; }
        private CharacterManager CharacterManager { get; }

        public StateAttacking(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, ObjectManager objectManager, CharacterManager characterManager, HookManager hookManager) : base(stateMachine)
        {
            Config = config;
            ObjectManager = objectManager;
            CharacterManager = characterManager;
            HookManager = hookManager;
        }

        public override void Enter()
        {

        }

        public override void Execute()
        {
            if (ObjectManager != null
                && ObjectManager.Player != null)
            {
                if (!ObjectManager.Player.IsInCombat)
                {
                    AmeisenBotStateMachine.SetState(AmeisenBotState.Idle);
                    return;
                }

                List<WowUnit> WowUnits = ObjectManager.WowObjects.OfType<WowUnit>().ToList();
                if (WowUnits.Count > 0)
                {
                    WowUnit target = WowUnits.FirstOrDefault(t => t.Guid == ObjectManager.TargetGuid);
                    bool validUnit = BotUtils.IsValidUnit(target);

                    if (validUnit && target.IsInCombat)
                    {
                        if (!ObjectManager.Player.IsAutoAttacking)
                        {
                            HookManager.StartAutoAttack();
                        }
                    }
                    else
                    {
                        // find a new target
                        HookManager.TargetNearestEnemy();
                    }
                }
            }
        }

        public override void Exit()
        {

        }
    }
}
