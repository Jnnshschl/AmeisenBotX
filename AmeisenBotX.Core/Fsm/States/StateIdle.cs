using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Core.Fsm.Enums;
using AmeisenBotX.Core.Fsm.States.Idle;
using AmeisenBotX.Core.Fsm.States.Idle.Actions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Fsm.States
{
    public class StateIdle : BasicState
    {
        public StateIdle(AmeisenBotFsm stateMachine, AmeisenBotConfig config, AmeisenBotInterfaces bot) : base(stateMachine, config, bot)
        {
            FirstStart = true;
            IdleActionManager = new(new List<IIdleAction>()
            {
                new AuctionHouseIdleAction(bot),
                new CheckMailsIdleAction(bot),
                new FishingIdleAction(bot),
                new LookAroundIdleAction(bot),
                new LookAtGroupIdleAction(bot),
                new RandomEmoteIdleAction(bot),
                new SitByCampfireIdleAction(bot),
                new SitToChairIdleAction(bot, stateMachine, Config.MinFollowDistance),
            });

            BagSlotCheckEvent = new(TimeSpan.FromMilliseconds(5000));
            EatCheckEvent = new(TimeSpan.FromMilliseconds(2000));
            LootCheckEvent = new(TimeSpan.FromMilliseconds(2000));
            RepairCheckEvent = new(TimeSpan.FromMilliseconds(5000));
            RefreshCharacterEvent = new(TimeSpan.FromMilliseconds(1000));
            IdleActionEvent = new(TimeSpan.FromMilliseconds(1000));
        }

        public bool FirstStart { get; set; }

        public IdleActionManager IdleActionManager { get; set; }

        private TimegatedEvent BagSlotCheckEvent { get; }

        private TimegatedEvent EatCheckEvent { get; }

        private TimegatedEvent IdleActionEvent { get; }

        private TimegatedEvent LootCheckEvent { get; }

        private TimegatedEvent RefreshCharacterEvent { get; }

        private TimegatedEvent RepairCheckEvent { get; }

        public override void Enter()
        {
            if (Bot.Objects.IsWorldLoaded)
            {
                if (Bot.Memory.Process != null && !Bot.Memory.Process.HasExited && FirstStart)
                {
                    FirstStart = false;

                    if (!Bot.Events.IsActive)
                    {
                        Bot.Events.Start();
                    }

                    Bot.Wow.LuaDoString($"SetCVar(\"maxfps\", {Config.MaxFps});SetCVar(\"maxfpsbk\", {Config.MaxFps})");
                    Bot.Wow.WowEnableClickToMove();
                }

                if (RefreshCharacterEvent.Run())
                {
                    Bot.Character.UpdateAll();
                }

                if (Bot.Player != null)
                {
                    // prevent endless running
                    Bot.Movement.SetMovementAction(MovementAction.Move, Bot.Player.Position);
                }

                IdleActionManager.Reset();
            }
        }

        public override void Execute()
        {
            // Bot.MovementEngine.SetMovementAction(MovementAction.Moving, new Vector3(-4918, -940, 501));
            // return;

            // do we need to loot stuff
            if (LootCheckEvent.Run()
                && Bot.Character.Inventory.FreeBagSlots > 0
                && StateMachine.GetState<StateLooting>().GetNearLootableUnits().Any())
            {
                StateMachine.SetState(BotState.Looting);
                return;
            }

            // do we need to eat something
            if (EatCheckEvent.Run()
                && StateMachine.GetState<StateEating>().NeedToEat())
            {
                StateMachine.SetState(BotState.Eating);
                return;
            }

            // we are on a battleground
            if (Bot.Memory.Read(Bot.Offsets.BattlegroundStatus, out int bgStatus)
                && bgStatus == 3
                && !Config.BattlegroundUsePartyMode)
            {
                StateMachine.SetState(BotState.Battleground);
                return;
            }

            // we are in a dungeon
            if (Bot.Objects.MapId.IsDungeonMap()
                && !Config.DungeonUsePartyMode)
            {
                StateMachine.SetState(BotState.Dungeon);
                return;
            }

            // do we need to repair our equipment
            if (Config.AutoRepair
                && RepairCheckEvent.Run()
                && StateMachine.GetState<StateRepairing>().NeedToRepair()
                && StateMachine.GetState<StateRepairing>().IsRepairNpcNear(out _))
            {
                StateMachine.SetState(BotState.Repairing);
                return;
            }

            // do we need to sell stuff
            if (Config.AutoSell
                && BagSlotCheckEvent.Run()
                && StateMachine.GetState<StateSelling>().NeedToSell()
                && StateMachine.GetState<StateSelling>().IsVendorNpcNear(out _))
            {
                StateMachine.SetState(BotState.Selling);
                return;
            }

            // do i need to complete/get quests
            if (Config.AutoTalkToNearQuestgivers
                && IsUnitToFollowThere(out IWowUnit unitToFollow, true)
                && unitToFollow != null
                && unitToFollow.TargetGuid != 0)
            {
                IWowUnit target = Bot.GetWowObjectByGuid<IWowUnit>(unitToFollow.TargetGuid);

                if (target != null && unitToFollow.DistanceTo(target) < 5.0f && (target.IsQuestgiver || target.IsGossip))
                {
                    StateMachine.SetState(BotState.StateTalkToQuestgivers);
                    return;
                }
            }

            // do i need to follow someone
            if ((!Config.Autopilot || Bot.Objects.MapId.IsDungeonMap()) && IsUnitToFollowThere(out _))
            {
                StateMachine.SetState(BotState.Following);
                return;
            }

            // do buffing etc...
            if (Bot.CombatClass != null)
            {
                Bot.CombatClass.OutOfCombatExecute();
            }

            if (StateMachine.StateOverride != BotState.Idle
                && StateMachine.StateOverride != BotState.None)
            {
                StateMachine.SetState(StateMachine.StateOverride);
            }

            if (Config.IdleActions && IdleActionEvent.Run())
            {
                IdleActionManager.Tick(Config.Autopilot);
            }
        }

        public bool IsUnitToFollowThere(out IWowUnit playerToFollow, bool ignoreRange = false)
        {
            IEnumerable<IWowPlayer> wowPlayers = Bot.Objects.WowObjects.OfType<IWowPlayer>().Where(e => !e.IsDead);

            if (wowPlayers.Any())
            {
                IWowUnit[] playersToTry =
                {
                    Config.FollowSpecificCharacter ? wowPlayers.FirstOrDefault(p => Bot.Db.GetUnitName(p, out string name) && name.Equals(Config.SpecificCharacterToFollow, StringComparison.OrdinalIgnoreCase)) : null,
                    Config.FollowGroupLeader ? Bot.Objects.Partyleader : null,
                    Config.FollowGroupMembers ? Bot.Objects.Partymembers.FirstOrDefault() : null
                };

                for (int i = 0; i < playersToTry.Length; ++i)
                {
                    if (playersToTry[i] != null && (ignoreRange || ShouldIFollowPlayer(playersToTry[i])))
                    {
                        playerToFollow = playersToTry[i];
                        return true;
                    }
                }
            }

            playerToFollow = null;
            return false;
        }

        public override void Leave()
        {
            Bot.Character.ItemSlotsToSkip.Clear();
        }

        private bool ShouldIFollowPlayer(IWowUnit playerToFollow)
        {
            if (playerToFollow != null)
            {
                Vector3 pos = playerToFollow.Position;

                if (Config.FollowPositionDynamic)
                {
                    pos += StateMachine.GetState<StateFollowing>().Offset;
                }

                double distance = pos.GetDistance(Bot.Player.Position);

                if (distance > Config.MinFollowDistance && distance < Config.MaxFollowDistance)
                {
                    return true;
                }
            }

            return false;
        }
    }
}