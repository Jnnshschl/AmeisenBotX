using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Objects.WowObjects;
using AmeisenBotX.Core.Movement.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Statemachine.States
{
    public class StateLooting : BasicState
    {
        public StateLooting(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine, config, wowInterface)
        {
            UnitLootQueue = new Queue<ulong>();
            UnitsAlreadyLootedList = new List<ulong>();
            LastOpenLootTry = new TimegatedEvent(TimeSpan.FromSeconds(1));
        }

        public List<ulong> UnitsAlreadyLootedList { get; private set; }

        private TimegatedEvent LastOpenLootTry { get; set; }

        private Queue<ulong> UnitLootQueue { get; set; }

        public override void Enter()
        {
            WowInterface.MovementEngine.Reset();
        }

        public override void Execute()
        {
            if (WowInterface.ObjectManager.Player.IsCasting)
            {
                return;
            }

            // add nearby Units to the loot List
            if (Config.LootUnits)
            {
                IEnumerable<WowUnit> wowUnits = StateMachine.GetNearLootableUnits();

                for (int i = 0; i < wowUnits.Count(); ++i)
                {
                    WowUnit lootableUnit = wowUnits.ElementAt(i);

                    if (!UnitLootQueue.Contains(lootableUnit.Guid))
                    {
                        UnitLootQueue.Enqueue(lootableUnit.Guid);
                    }
                }
            }

            if (UnitLootQueue.Count == 0)
            {
                StateMachine.SetState(BotState.Idle);
            }
            else
            {
                if (UnitsAlreadyLootedList.Contains(UnitLootQueue.Peek()))
                {
                    if (UnitLootQueue.Count > 0)
                    {
                        UnitLootQueue.Dequeue();
                    }

                    return;
                }

                WowUnit selectedUnit = WowInterface.ObjectManager.WowObjects.OfType<WowUnit>()
                    .OrderBy(e => e.Position.GetDistance(WowInterface.ObjectManager.Player.Position))
                    .FirstOrDefault(e => e.Guid == UnitLootQueue.Peek());

                if (selectedUnit != null && selectedUnit.IsDead && selectedUnit.IsLootable)
                {
                    WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, selectedUnit.Position);

                    if (LastOpenLootTry.Run())
                    {
                        if (WowInterface.MovementEngine.IsAtTargetPosition)
                        {
                            WowInterface.HookManager.StopClickToMoveIfActive();
                        }

                        WowInterface.HookManager.UnitOnRightClick(selectedUnit);
                        if (WowInterface.XMemory.Read(WowInterface.OffsetList.LootWindowOpen, out byte lootOpen)
                             && lootOpen > 0)
                        {
                            WowInterface.HookManager.LootEveryThing();
                            UnitsAlreadyLootedList.Add(UnitLootQueue.Dequeue());

                            if (UnitLootQueue.Count > 0)
                            {
                                UnitLootQueue.Dequeue();
                            }
                        }
                    }
                }
                else
                {
                    if (UnitLootQueue.Count > 0)
                    {
                        UnitLootQueue.Dequeue();
                    }
                }
            }
        }

        public override void Leave()
        {
        }
    }
}