using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Core.Fsm.Enums;
using System;
using System.Linq;

namespace AmeisenBotX.Core.Fsm.States
{
    public class StateEating : BasicState
    {
        public StateEating(AmeisenBotFsm stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine, config, wowInterface)
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
            if (DateTime.UtcNow.Subtract(LastAction).TotalMilliseconds >= 250.0)
            {
                LastAction = DateTime.UtcNow;

                if ((CurrentlyEating.Length > 0 || CurrentlyDrinking.Length > 0)
                    && WowInterface.Player.HasBuffByName("Food")
                    && WowInterface.Player.HasBuffByName("Drink"))
                {
                    if (WowInterface.Player.HealthPercentage >= 95.0
                        && WowInterface.Player.ManaPercentage >= 95.0
                        && (WowInterface.Objects.Partyleader.Guid == 0
                            || WowInterface.Player.Position.GetDistance(WowInterface.Objects.MeanGroupPosition) < 30.0f))
                    {
                        // exit if we are near full hp and power
                        StateMachine.SetState(BotState.Idle);
                        WowInterface.CharacterManager.Jump();
                    }

                    return;
                }
                else if (CurrentlyEating.Length > 0 && WowInterface.Player.HasBuffByName("Food"))
                {
                    if (WowInterface.Player.HealthPercentage >= 95.0
                        && (WowInterface.Objects.Partyleader.Guid == 0
                            || WowInterface.Player.Position.GetDistance(WowInterface.Objects.MeanGroupPosition) < 30.0f))
                    {
                        // exit if we are near full hp
                        StateMachine.SetState(BotState.Idle);
                        WowInterface.CharacterManager.Jump();
                    }

                    return;
                }
                else if (CurrentlyDrinking.Length > 0 && WowInterface.Player.HasBuffByName("Drink"))
                {
                    if (WowInterface.Player.ManaPercentage >= 95.0
                        && (WowInterface.Objects.Partyleader.Guid == 0
                            || WowInterface.Player.Position.GetDistance(WowInterface.Objects.MeanGroupPosition) < 30.0f))
                    {
                        // exit if we are near full power
                        StateMachine.SetState(BotState.Idle);
                        WowInterface.CharacterManager.Jump();
                    }

                    return;
                }
                else if (!NeedToEat())
                {
                    // exit if we are near full hp/power
                    StateMachine.SetState(BotState.Idle);
                    WowInterface.CharacterManager.Jump();
                    return;
                }

                Type t = default;

                if (WowInterface.Player.HealthPercentage < Config.EatUntilPercent
                    && WowInterface.Player.MaxMana > 0
                    && WowInterface.Player.ManaPercentage < Config.DrinkUntilPercent
                    && WowInterface.CharacterManager.HasItemTypeInBag<WowRefreshment>(true))
                {
                    t = typeof(WowRefreshment);
                }
                else if (WowInterface.Player.HealthPercentage < Config.EatUntilPercent
                         && WowInterface.CharacterManager.HasItemTypeInBag<WowFood>(true))
                {
                    t = typeof(WowFood);
                }
                else if (WowInterface.Player.MaxMana > 0
                         && WowInterface.Player.ManaPercentage < Config.DrinkUntilPercent
                         && WowInterface.CharacterManager.HasItemTypeInBag<WowWater>(true))
                {
                    t = typeof(WowWater);
                }

                string itemName = WowInterface.CharacterManager.Inventory.Items.First(e => Enum.IsDefined(t, e.Id)).Name;
                WowInterface.NewWowInterface.LuaUseItemByName(itemName);
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
            return ((WowInterface.Objects.Partyleader.Guid == 0
                        || WowInterface.Player.Position.GetDistance(WowInterface.Objects.MeanGroupPosition) < 30.0f)
                    && (WowInterface.Player.HealthPercentage < Config.EatUntilPercent
                             && WowInterface.Player.HealthPercentage < 95.0
                             && WowInterface.Player.ManaPercentage < Config.DrinkUntilPercent
                             && WowInterface.Player.ManaPercentage < 95.0
                             && WowInterface.CharacterManager.HasItemTypeInBag<WowRefreshment>(true))
                         // Food
                         || (WowInterface.Player.HealthPercentage < Config.EatUntilPercent
                             && WowInterface.Player.HealthPercentage < 95.0
                             && WowInterface.CharacterManager.HasItemTypeInBag<WowFood>(true))
                         // Water
                         || (WowInterface.Player.MaxMana > 0
                             && WowInterface.Player.ManaPercentage < Config.DrinkUntilPercent
                             && WowInterface.Player.ManaPercentage < 95.0
                             && WowInterface.CharacterManager.HasItemTypeInBag<WowWater>(true)));
        }
    }
}