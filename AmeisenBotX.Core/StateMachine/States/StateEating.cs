using AmeisenBotX.Core.Data.Enums;
using System;
using System.Linq;

namespace AmeisenBotX.Core.Statemachine.States
{
    public class StateEating : BasicState
    {
        public StateEating(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine, config, wowInterface)
        {
        }

        private string CurrentlyDrinking { get; set; }

        private string CurrentlyEating { get; set; }

        private DateTime LastAction { get; set; }

        public override void Enter()
        {
            CurrentlyEating = string.Empty;
            CurrentlyDrinking = string.Empty;
        }

        public override void Execute()
        {
            if (DateTime.Now - LastAction > TimeSpan.FromSeconds(1))
            {
                Type t = default;
                if (WowInterface.ObjectManager.Player.HealthPercentage < 95 && WowInterface.ObjectManager.Player.ManaPercentage < 95 && WowInterface.CharacterManager.HasRefreshmentInBag())
                {
                    t = typeof(WowRefreshment);
                    if ((CurrentlyEating.Length > 0 || CurrentlyDrinking.Length > 0)
                        && WowInterface.ObjectManager.Player.HasBuffByName("Food")
                        && WowInterface.ObjectManager.Player.HasBuffByName("Drink"))
                    {
                        return;
                    }
                }
                else if (WowInterface.ObjectManager.Player.HealthPercentage < 95 && WowInterface.CharacterManager.HasFoodInBag())
                {
                    t = typeof(WowFood);
                    if (CurrentlyEating.Length > 0
                        && WowInterface.ObjectManager.Player.HasBuffByName("Food"))
                    {
                        return;
                    }
                }
                else if (WowInterface.ObjectManager.Player.ManaPercentage < 95 && WowInterface.CharacterManager.HasWaterInBag())
                {
                    t = typeof(WowWater);
                    if (CurrentlyDrinking.Length > 0
                        && WowInterface.ObjectManager.Player.HasBuffByName("Drink"))
                    {
                        return;
                    }
                }
                else
                {
                    // exit if we have no more food left or are near full hp/power
                    StateMachine.SetState((int)BotState.Idle);
                    return;
                }

                LastAction = DateTime.Now;
                string itemName = WowInterface.CharacterManager.Inventory.Items.First(e => Enum.IsDefined(t, e.Id)).Name;
                WowInterface.HookManager.UseItemByName(itemName);

                if (t == typeof(WowRefreshment))
                {
                    CurrentlyEating = itemName;
                    CurrentlyDrinking = itemName;
                }
                else if (t == typeof(WowFood))
                {
                    CurrentlyEating = itemName;
                }
                else if (t == typeof(WowWater))
                {
                    CurrentlyDrinking = itemName;
                }
            }
        }

        public override void Exit()
        {
        }
    }
}