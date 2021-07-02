using AmeisenBotX.Core.Fsm.Enums;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Linq;

namespace AmeisenBotX.Core.Fsm.States
{
    public class StateEating : BasicState
    {
        public StateEating(AmeisenBotFsm stateMachine, AmeisenBotConfig config, AmeisenBotInterfaces bot) : base(stateMachine, config, bot)
        {
        }

        private string CurrentlyDrinking { get; set; }

        private string CurrentlyEating { get; set; }

        private DateTime LastAction { get; set; }

        public override void Enter()
        {
            CurrentlyEating = string.Empty;
            CurrentlyDrinking = string.Empty;
            Bot.Movement.StopMovement();
        }

        public override void Execute()
        {
            if (DateTime.UtcNow.Subtract(LastAction).TotalMilliseconds >= 250.0)
            {
                LastAction = DateTime.UtcNow;

                if ((CurrentlyEating.Length > 0 || CurrentlyDrinking.Length > 0)
                    && Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Food")
                    && Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Drink"))
                {
                    if (Bot.Player.HealthPercentage >= 95.0
                        && Bot.Player.ManaPercentage >= 95.0
                        && (Bot.Objects.Partyleader.Guid == 0
                            || Bot.Player.Position.GetDistance(Bot.Objects.CenterPartyPosition) < 30.0f))
                    {
                        // exit if we are near full hp and power
                        StateMachine.SetState(BotState.Idle);
                        Bot.Character.Jump();
                    }

                    return;
                }
                else if (CurrentlyEating.Length > 0 && Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Food"))
                {
                    if (Bot.Player.HealthPercentage >= 95.0
                        && (Bot.Objects.Partyleader.Guid == 0
                            || Bot.Player.Position.GetDistance(Bot.Objects.CenterPartyPosition) < 30.0f))
                    {
                        // exit if we are near full hp
                        StateMachine.SetState(BotState.Idle);
                        Bot.Character.Jump();
                    }

                    return;
                }
                else if (CurrentlyDrinking.Length > 0 && Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Drink"))
                {
                    if (Bot.Player.ManaPercentage >= 95.0
                        && (Bot.Objects.Partyleader.Guid == 0
                            || Bot.Player.Position.GetDistance(Bot.Objects.CenterPartyPosition) < 30.0f))
                    {
                        // exit if we are near full power
                        StateMachine.SetState(BotState.Idle);
                        Bot.Character.Jump();
                    }

                    return;
                }
                else if (!NeedToEat())
                {
                    // exit if we are near full hp/power
                    StateMachine.SetState(BotState.Idle);
                    Bot.Character.Jump();
                    return;
                }

                Type t = default;

                if (Bot.Player.HealthPercentage < Config.EatUntilPercent
                    && Bot.Player.MaxMana > 0
                    && Bot.Player.ManaPercentage < Config.DrinkUntilPercent
                    && Bot.Character.HasItemTypeInBag<WowRefreshment>(true))
                {
                    t = typeof(WowRefreshment);
                }
                else if (Bot.Player.HealthPercentage < Config.EatUntilPercent
                         && Bot.Character.HasItemTypeInBag<WowFood>(true))
                {
                    t = typeof(WowFood);
                }
                else if (Bot.Player.MaxMana > 0
                         && Bot.Player.ManaPercentage < Config.DrinkUntilPercent
                         && Bot.Character.HasItemTypeInBag<WowWater>(true))
                {
                    t = typeof(WowWater);
                }

                string itemName = Bot.Character.Inventory.Items.First(e => Enum.IsDefined(t, e.Id)).Name;
                Bot.Wow.LuaUseItemByName(itemName);
                Bot.Movement.StopMovement();

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
            Bot.Movement.StopMovement();
        }

        internal bool NeedToEat()
        {
            return Bot.Objects.Partyleader != null
                && ((Bot.Objects.Partyleader.Guid == 0 || Bot.Player.Position.GetDistance(Bot.Objects.CenterPartyPosition) < 30.0f)
                    && (Bot.Player.HealthPercentage < Config.EatUntilPercent
                             && Bot.Player.HealthPercentage < 95.0
                             && Bot.Player.ManaPercentage < Config.DrinkUntilPercent
                             && Bot.Player.ManaPercentage < 95.0
                             && Bot.Character.HasItemTypeInBag<WowRefreshment>(true))
                         // Food
                         || (Bot.Player.HealthPercentage < Config.EatUntilPercent
                             && Bot.Player.HealthPercentage < 95.0
                             && Bot.Character.HasItemTypeInBag<WowFood>(true))
                         // Water
                         || (Bot.Player.MaxMana > 0
                             && Bot.Player.ManaPercentage < Config.DrinkUntilPercent
                             && Bot.Player.ManaPercentage < 95.0
                             && Bot.Character.HasItemTypeInBag<WowWater>(true)));
        }
    }
}