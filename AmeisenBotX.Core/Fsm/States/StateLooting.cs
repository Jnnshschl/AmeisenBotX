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
        private const float MaxLootDistance = 5.0f;

        public StateLooting(AmeisenBotFsm stateMachine, AmeisenBotConfig config, AmeisenBotInterfaces bot) : base(stateMachine, config, bot)
        {
            UnitLootQueue = new();
            UnitsAlreadyLootedList = new();
            LastOpenLootTry = new(TimeSpan.FromSeconds(1));
        }

        public List<ulong> UnitsAlreadyLootedList { get; private set; }

        private TimegatedEvent LastOpenLootTry { get; set; }

        private float LootTryCount { get; set; }

        private Queue<ulong> UnitLootQueue { get; set; }

        public override void Enter()
        {
            Bot.Movement.Reset();
        }

        public override void Execute()
        {
            if (Bot.Player.IsCasting)
            {
                return;
            }

            // add nearby Units to the loot List
            if (Config.LootUnits)
            {
                IEnumerable<IWowUnit> wowUnits = GetNearLootableUnits().OrderBy(e => e.DistanceTo(Bot.Player));

                for (int i = 0; i < wowUnits.Count(); ++i)
                {
                    IWowUnit lootableUnit = wowUnits.ElementAt(i);

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
                IWowUnit selectedUnit = Bot.Objects.WowObjects.OfType<IWowUnit>()
                    .Where(e => e.IsLootable)
                    .OrderBy(e => e.Position.GetDistance(Bot.Player.Position))
                    .FirstOrDefault(e => e.Guid == UnitLootQueue.Peek());

                if (selectedUnit != null && LootTryCount < 3)
                {
                    // If enemies are nearby kill them first
                    // var path = Bot.PathfindingHandler.GetPath((int)Bot.ObjectManager.MapId,
                    //     Bot.Player.Position, selectedUnit.Position);
                    // if (path != null)
                    // {
                    //     IEnumerable<IWowUnit> nearbyEnemies =
                    //         Bot.ObjectManager.GetEnemiesInPath<IWowUnit>(path, 10.0);
                    //     if (nearbyEnemies.Any())
                    //     {
                    //         var enemy = nearbyEnemies.FirstOrDefault();
                    //         Bot.NewBot.WowTargetGuid(enemy.Guid);
                    //         Bot.CombatClass.AttackTarget();
                    //         return;
                    //     }
                    // }

                    Bot.Movement.SetMovementAction(MovementAction.Move, selectedUnit.Position);

                    if (LastOpenLootTry.Run()
                        && Bot.Player.Position.GetDistance(selectedUnit.Position) < MaxLootDistance)
                    {
                        Bot.Wow.WowStopClickToMove();
                        Loot(selectedUnit);
                        ++LootTryCount;
                    }
                }
                else if (UnitLootQueue.Count > 0)
                {
                    LootTryCount = 0;
                    UnitLootQueue.Dequeue();
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

        private void Loot(IWowUnit unit)
        {
            Bot.Wow.WowUnitRightClick(unit.BaseAddress);

            // if AutoLoot is enabled, the unit will be dequeued after it is looted because it will no longer be IsLootable
            // there is no need to handle the dequeing here
            if (Bot.Wow.LuaAutoLootEnabled()
                  && Bot.Memory.Read(Bot.Offsets.LootWindowOpen, out byte lootOpen)
                  && lootOpen > 0)
            {
                Bot.Wow.LuaLootEveryThing();
                UnitsAlreadyLootedList.Add(UnitLootQueue.Dequeue());
            }
        }
    }
}