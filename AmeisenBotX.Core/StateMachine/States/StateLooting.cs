using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Movement.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.Statemachine.States
{
    public class StateLooting : BasicState
    {
        public StateLooting(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, WowInterface wowInterface, Queue<ulong> unitLootList) : base(stateMachine, config, wowInterface)
        {
            UnitLootList = unitLootList;
            UnitsAlreadyLootedList = new List<ulong>();
        }

        private int LootTryCount { get; set; }

        private Queue<ulong> UnitLootList { get; }

        private List<ulong> UnitsAlreadyLootedList { get; }

        public override void Enter()
        {
            LootTryCount = 0;
        }

        public override void Execute()
        {
            if (UnitLootList.Count == 0)
            {
                StateMachine.SetState(BotState.Idle);
            }
            else
            {
                if (UnitsAlreadyLootedList.Contains(UnitLootList.Peek()))
                {
                    UnitLootList.Dequeue();
                    return;
                }

                WowUnit selectedUnit = WowInterface.ObjectManager.WowObjects.OfType<WowUnit>().FirstOrDefault(e => e.Guid == UnitLootList.Peek());
                if (selectedUnit != null && selectedUnit.IsDead && selectedUnit.IsLootable)
                {
                    if (WowInterface.ObjectManager.Player.Position.GetDistance(selectedUnit.Position) > 3.0)
                    {
                        WowInterface.MovementEngine.SetState(MovementEngineState.Moving, selectedUnit.Position);
                        WowInterface.MovementEngine.Execute();
                        LootTryCount++;

                        if (LootTryCount == 64)
                        {
                            UnitsAlreadyLootedList.Add(UnitLootList.Dequeue());
                        }
                    }
                    else
                    {
                        do
                        {
                            WowInterface.HookManager.UnitOnRightClick(selectedUnit);
                            LootTryCount++;
                            Task.Delay(1000).GetAwaiter().GetResult();
                        } while (WowInterface.XMemory.ReadByte(WowInterface.OffsetList.LootWindowOpen, out byte lootOpen)
                                 && lootOpen == 0
                                 && LootTryCount < 2);

                        UnitsAlreadyLootedList.Add(UnitLootList.Dequeue());
                    }
                }
                else
                {
                    UnitLootList.Dequeue();
                }
            }
        }

        public override void Exit()
        {
        }
    }
}