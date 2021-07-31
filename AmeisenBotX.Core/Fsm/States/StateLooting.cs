using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Core.Fsm.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Fsm.States
{
    public class StateLooting : BasicState
    {
        public StateLooting(AmeisenBotFsm stateMachine, AmeisenBotConfig config, AmeisenBotInterfaces bot) : base(stateMachine, config, bot)
        {
            UnitLootQueue = new();
            UnitsAlreadyLootedList = new();
            LastOpenLootTry = new(TimeSpan.FromMilliseconds(1000));
        }

        public List<ulong> UnitsAlreadyLootedList { get; private set; }

        private ulong LastGuid { get; set; }

        private TimegatedEvent LastOpenLootTry { get; set; }

        private float LootTryCount { get; set; }

        private Queue<ulong> UnitLootQueue { get; set; }

        public override void Enter()
        {
            LastGuid = 0;
            Bot.Movement.Reset();

            foreach (IWowUnit unit in GetNearLootableUnits().OrderBy(e => e.DistanceTo(Bot.Player)))
            {
                if (!UnitLootQueue.Contains(unit.Guid) && !UnitsAlreadyLootedList.Contains(unit.Guid))
                {
                    UnitLootQueue.Enqueue(unit.Guid);
                }
            }
        }

        public override void Execute()
        {
            if (Bot.Player.IsCasting)
            {
                return;
            }

            if (UnitLootQueue.Count == 0)
            {
                StateMachine.SetState(BotState.Idle);
            }
            else
            {
                ulong guidToLoot = UnitLootQueue.Peek();

                if (guidToLoot != LastGuid)
                {
                    LootTryCount = 0;
                }

                IWowUnit selectedUnit = Bot.GetWowObjectByGuid<IWowUnit>(guidToLoot);

                if (selectedUnit == null || !selectedUnit.IsLootable)
                {
                    UnitsAlreadyLootedList.Add(UnitLootQueue.Dequeue());
                    return;
                }

                if (LootTryCount < 3)
                {
                    if (Bot.Player.DistanceTo(selectedUnit) > 5.0f)
                    {
                        Bot.Movement.SetMovementAction(MovementAction.Move, selectedUnit.Position);
                    }
                    else if (LastOpenLootTry.Run())
                    {
                        if (Bot.Memory.Read(Bot.Wow.Offsets.LootWindowOpen, out byte lootOpen)
                            && lootOpen > 0)
                        {
                            Bot.Wow.LootEverything();
                            UnitsAlreadyLootedList.Add(UnitLootQueue.Dequeue());
                            Bot.Wow.ClickUiElement("LootCloseButton");
                        }
                        else
                        {
                            Bot.Wow.StopClickToMove();
                            Bot.Wow.InteractWithUnit(selectedUnit.BaseAddress);
                        }

                        ++LootTryCount;
                    }
                }
                else if (UnitLootQueue.Count > 0)
                {
                    UnitsAlreadyLootedList.Add(UnitLootQueue.Dequeue());
                }
            }
        }

        public override void Leave()
        {
        }

        internal IEnumerable<IWowUnit> GetNearLootableUnits()
        {
            return Bot.Objects.WowObjects.OfType<IWowUnit>()
                .Where(e => e.IsLootable
                    && !UnitsAlreadyLootedList.Contains(e.Guid)
                    && e.Position.GetDistance(Bot.Player.Position) < Config.LootUnitsRadius);
        }
    }
}