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
            WowInterface.MovementEngine.StopMovement();
        }

        public override void Execute()
        {
            if (DateTime.Now.Subtract(LastAction).TotalMilliseconds >= 250.0)
            {
                LastAction = DateTime.Now;

                if ((CurrentlyEating.Length > 0
                        || CurrentlyDrinking.Length > 0)
                    && (WowInterface.ObjectManager.Player.HasBuffByName("Food")
                        || WowInterface.ObjectManager.Player.HasBuffByName("Drink")))
                {
                    if ((WowInterface.ObjectManager.Player.HealthPercentage >= Config.EatUntilPercent
                             || WowInterface.ObjectManager.Player.HealthPercentage >= 95.0)
                         && (WowInterface.ObjectManager.Player.ManaPercentage >= Config.DrinkUntilPercent
                             || WowInterface.ObjectManager.Player.ManaPercentage >= 95.0))
                    {
                        // exit if we are near full hp/power
                        StateMachine.SetState(BotState.Idle);
                        WowInterface.CharacterManager.Jump();
                    }

                    return;
                }
                else if(!NeedToEat())
                {
                    // exit if we are near full hp/power
                    StateMachine.SetState(BotState.Idle);
                    WowInterface.CharacterManager.Jump();
                    return;
                }

                Type t = default;

                if (WowInterface.ObjectManager.Player.HealthPercentage < Config.EatUntilPercent
                    && WowInterface.ObjectManager.Player.MaxMana > 0
                    && WowInterface.ObjectManager.Player.ManaPercentage < Config.DrinkUntilPercent
                    && WowInterface.CharacterManager.HasItemTypeInBag<WowRefreshment>(true))
                {
                    t = typeof(WowRefreshment);
                }
                else if (WowInterface.ObjectManager.Player.HealthPercentage < Config.EatUntilPercent
                         && WowInterface.CharacterManager.HasItemTypeInBag<WowFood>(true))
                {
                    t = typeof(WowFood);
                }
                else if (WowInterface.ObjectManager.Player.MaxMana > 0
                         && WowInterface.ObjectManager.Player.ManaPercentage < Config.DrinkUntilPercent
                         && WowInterface.CharacterManager.HasItemTypeInBag<WowWater>(true))
                {
                    t = typeof(WowWater);
                }

                string itemName = WowInterface.CharacterManager.Inventory.Items.First(e => Enum.IsDefined(t, e.Id)).Name;
                WowInterface.HookManager.LuaUseItemByName(itemName);
                WowInterface.MovementEngine.StopMovement();

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

        public override void Leave()
        {
            WowInterface.MovementEngine.StopMovement();
        }

        internal bool NeedToEat()
        {
            return ((WowInterface.ObjectManager.PartyleaderGuid == 0
                        || WowInterface.ObjectManager.Player.Position.GetDistance(WowInterface.ObjectManager.MeanGroupPosition) < 30.0f)
                    && (WowInterface.ObjectManager.Player.HealthPercentage < Config.EatUntilPercent
                             && WowInterface.ObjectManager.Player.HealthPercentage < 95.0
                             && WowInterface.ObjectManager.Player.ManaPercentage < Config.DrinkUntilPercent
                             && WowInterface.ObjectManager.Player.ManaPercentage < 95.0
                             && WowInterface.CharacterManager.HasItemTypeInBag<WowRefreshment>(true))
                         // Food
                         || (WowInterface.ObjectManager.Player.HealthPercentage < Config.EatUntilPercent
                             && WowInterface.ObjectManager.Player.HealthPercentage < 95.0
                             && WowInterface.CharacterManager.HasItemTypeInBag<WowFood>(true))
                         // Water
                         || (WowInterface.ObjectManager.Player.MaxMana > 0
                             && WowInterface.ObjectManager.Player.ManaPercentage < Config.DrinkUntilPercent
                             && WowInterface.ObjectManager.Player.ManaPercentage < 95.0
                             && WowInterface.CharacterManager.HasItemTypeInBag<WowWater>(true)));
        }
    }
}