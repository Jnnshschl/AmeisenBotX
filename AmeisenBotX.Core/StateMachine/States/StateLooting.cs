﻿using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Objects.WowObjects;
using AmeisenBotX.Core.Movement.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

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

                    if (!UnitLootQueue.Contains(lootableUnit.Guid) && !UnitsAlreadyLootedList.Contains(lootableUnit.Guid))
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
                WowUnit selectedUnit = WowInterface.ObjectManager.WowObjects.OfType<WowUnit>()
                    .Where(e => e.IsLootable)
                    .OrderBy(e => e.Position.GetDistance(WowInterface.ObjectManager.Player.Position))
                    .FirstOrDefault(e => e.Guid == UnitLootQueue.Peek());

                if (selectedUnit != null)
                {
                    WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, selectedUnit.Position);

                    if (LastOpenLootTry.Run())
                    {
                        if (WowInterface.MovementEngine.IsAtTargetPosition)
                        {
                            WowInterface.HookManager.WowStopClickToMove();
                            Loot(selectedUnit);
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

        private void Loot(WowUnit unit)
        {
            WowInterface.HookManager.WowUnitRightClick(unit);

            // if AutoLoot is enabled, the unit will be dequeued after it is looted because it will no longer be IsLootable
            // there is no need to handle the dequeing here
            if (WowInterface.HookManager.LuaAutoLootEnabled()
                  && WowInterface.XMemory.Read(WowInterface.OffsetList.LootWindowOpen, out byte lootOpen)
                  && lootOpen > 0)
            {
                WowInterface.HookManager.LuaLootEveryThing();
                UnitsAlreadyLootedList.Add(UnitLootQueue.Dequeue());
            }
        }

        public override void Leave()
        {
        }
    }
}