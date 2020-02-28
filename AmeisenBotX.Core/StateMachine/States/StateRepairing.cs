using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.Movement;
using AmeisenBotX.Core.Movement.Enums;
using System.Linq;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.StateMachine.States
{
    public class StateRepairing : BasicState
    {
        public StateRepairing(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, ObjectManager objectManager, HookManager hookmanager, CharacterManager characterManager, IMovementEngine movementEngine) : base(stateMachine)
        {
            Config = config;
            ObjectManager = objectManager;
            HookManager = hookmanager;
            CharacterManager = characterManager;
            MovementEngine = movementEngine;
        }

        private CharacterManager CharacterManager { get; }

        private AmeisenBotConfig Config { get; }

        private HookManager HookManager { get; }

        private IMovementEngine MovementEngine { get; set; }

        private ObjectManager ObjectManager { get; }

        public override void Enter()
        {
        }

        public override void Execute()
        {
            if (CharacterManager.Equipment.Equipment.Any(e => ((double)e.Value.MaxDurability / (double)e.Value.Durability) > 0.2))
            {
                AmeisenBotStateMachine.SetState(BotState.Idle);
                return;
            }

            WowUnit selectedUnit = ObjectManager.WowObjects.OfType<WowUnit>()
                .OrderBy(e => e.Position.GetDistance(ObjectManager.Player.Position))
                .FirstOrDefault(e => e.GetType() != typeof(WowPlayer)
                    && HookManager.GetUnitReaction(ObjectManager.Player, e) == WowUnitReaction.Friendly
                    && e.IsRepairVendor
                    && e.Position.GetDistance(ObjectManager.Player.Position) < 50);

            if (selectedUnit != null && !selectedUnit.IsDead)
            {
                double distance = ObjectManager.Player.Position.GetDistance(selectedUnit.Position);
                if (distance > 5.0)
                {
                    MovementEngine.SetState(MovementEngineState.Moving, selectedUnit.Position);
                    MovementEngine.Execute();
                }
                else
                {
                    if (distance > 3)
                    {
                        CharacterManager.InteractWithUnit(selectedUnit, 20.9f, 0.2f);
                    }
                    else
                    {
                        HookManager.RightClickUnit(selectedUnit);
                        Task.Delay(1000).GetAwaiter().GetResult();

                        HookManager.RepairAllItems();
                        HookManager.SellAllGrayItems();
                        Task.Delay(1000).GetAwaiter().GetResult();
                    }
                }
            }
            else
            {
                AmeisenBotStateMachine.SetState(BotState.Idle);
            }
        }

        public override void Exit()
        {
        }
    }
}