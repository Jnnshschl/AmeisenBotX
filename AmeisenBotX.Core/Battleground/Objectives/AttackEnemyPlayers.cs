using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.Movement;
using AmeisenBotX.Core.Movement.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.Battleground.Objectives
{
    class AttackEnemyPlayers : IBattlegroundObjective
    {
        public AttackEnemyPlayers(int priority, HookManager hookManager, ObjectManager objectManager, IMovementEngine movementEngine, ref bool forceCombat)
        {
            Priority = priority;
            HookManager = hookManager;
            ObjectManager = objectManager;
            MovementEngine = movementEngine;
            ForceCombat = forceCombat;
        }

        public int Priority { get; private set; }

        public bool IsAvailable => ObjectManager.GetNearEnemies(50.0).Count() > 0;

        private HookManager HookManager { get; }

        private ObjectManager ObjectManager { get; }

        private IMovementEngine MovementEngine { get; }

        private bool ForceCombat { get; set; }

        public void Enter()
        {
            MovementEngine.Reset();
        }

        public void Execute()
        {
            IEnumerable<WowPlayer> nearEnemies = ObjectManager.GetNearEnemies(50.0);

            if (nearEnemies.Count() == 0)
            {
                return;
            }

            WowPlayer selectedTarget = nearEnemies.OrderBy(e => e.HealthPercentage).FirstOrDefault();

            if (ObjectManager.Player.Position.GetDistance(selectedTarget.Position) > 3)
            {
                MovementEngine.SetState(MovementEngineState.Moving, selectedTarget.Position);
                MovementEngine.Execute();
            }
            else
            {
                ForceCombat = true;
                HookManager.TargetGuid(selectedTarget.Guid);
                HookManager.StartAutoAttack();
                MovementEngine.Reset();
            }
        }

        public void Exit()
        {
            MovementEngine.Reset();
        }
    }
}
