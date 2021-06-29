using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Fsm.Enums;
using AmeisenBotX.Core.Fsm.States.Idle;
using AmeisenBotX.Core.Fsm.States.Idle.Actions;
using AmeisenBotX.Core.Movement.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Fsm.States
{
    public class StateIdle : BasicState
    {
        public StateIdle(AmeisenBotFsm stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine, config, wowInterface)
        {
            FirstStart = true;
            IdleActionManager = new IdleActionManager(config.IdleActionsMaxCooldown, config.IdleActionsMinCooldown, new List<IIdleAction>()
            {
                new AuctionHouseIdleAction(wowInterface),
                new CheckMailsIdleAction(wowInterface),
                new FishingIdleAction(wowInterface),
                new LookAroundIdleAction(wowInterface),
                new LookAtGroupIdleAction(wowInterface),
                new RandomEmoteIdleAction(wowInterface),
                new SitByCampfireIdleAction(wowInterface),
                new SitToChairIdleAction(wowInterface, stateMachine, Config.MinFollowDistance),
            });

            BagSlotCheckEvent = new(TimeSpan.FromMilliseconds(5000));
            EatCheckEvent = new(TimeSpan.FromMilliseconds(2000));
            LootCheckEvent = new(TimeSpan.FromMilliseconds(2000));
            RepairCheckEvent = new(TimeSpan.FromMilliseconds(5000));
            QuestgiverCheckEvent = new(TimeSpan.FromMilliseconds(2000));
            QuestgiverRightClickEvent = new(TimeSpan.FromMilliseconds(3000));
            RefreshCharacterEvent = new(TimeSpan.FromMilliseconds(1000));
            IdleActionEvent = new(TimeSpan.FromMilliseconds(1000));
        }

        public bool FirstStart { get; set; }

        public IdleActionManager IdleActionManager { get; set; }

        private TimegatedEvent BagSlotCheckEvent { get; }

        private TimegatedEvent EatCheckEvent { get; }

        private TimegatedEvent IdleActionEvent { get; }

        private TimegatedEvent LootCheckEvent { get; }

        private TimegatedEvent QuestgiverCheckEvent { get; }

        private TimegatedEvent QuestgiverRightClickEvent { get; }

        private TimegatedEvent RefreshCharacterEvent { get; }

        private TimegatedEvent RepairCheckEvent { get; }

        public override void Enter()
        {
            if (WowInterface.Objects.IsWorldLoaded)
            {
                if (WowInterface.WowProcess != null && !WowInterface.WowProcess.HasExited && FirstStart)
                {
                    FirstStart = false;

                    if (!WowInterface.EventHookManager.IsActive)
                    {
                        WowInterface.EventHookManager.Start();
                    }

                    WowInterface.NewWowInterface.LuaDoString($"SetCVar(\"maxfps\", {Config.MaxFps});SetCVar(\"maxfpsbk\", {Config.MaxFps})");
                    WowInterface.NewWowInterface.WowEnableClickToMove();
                }

                if (RefreshCharacterEvent.Run())
                {
                    WowInterface.CharacterManager.UpdateAll();
                }

                if (WowInterface.Player != null)
                {
                    // prevent endless running
                    WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, WowInterface.Player.Position);
                }

                IdleActionManager.Reset();
            }
        }

        public override void Execute()
        {
            // WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, new Vector3(-4918, -940, 501));
            // return;

            // do we need to loot stuff
            if (LootCheckEvent.Run()
                && WowInterface.CharacterManager.Inventory.FreeBagSlots > 0
                && StateMachine.GetNearLootableUnits().Any())
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
            if (WowInterface.XMemory.Read(WowInterface.OffsetList.BattlegroundStatus, out int bgStatus)
                && bgStatus == 3
                && !Config.BattlegroundUsePartyMode)
            {
                StateMachine.SetState(BotState.Battleground);
                return;
            }

            // we are in a dungeon
            if (WowInterface.Objects.MapId.IsDungeonMap()
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
                && IsUnitToFollowThere(out WowUnit unitToFollow, true)
                && unitToFollow != null
                && unitToFollow.TargetGuid != 0)
            {
                if (QuestgiverCheckEvent.Run()
                    && HandleAutoQuestMode(unitToFollow))
                {
                    return;
                }
            }

            // do i need to follow someone
            if ((!Config.Autopilot || WowInterface.Objects.MapId.IsDungeonMap()) && IsUnitToFollowThere(out _))
            {
                StateMachine.SetState(BotState.Following);
                return;
            }

            // do buffing etc...
            if (WowInterface.CombatClass != null)
            {
                WowInterface.CombatClass.OutOfCombatExecute();
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

        public bool IsUnitToFollowThere(out WowUnit playerToFollow, bool ignoreRange = false)
        {
            IEnumerable<WowPlayer> wowPlayers = WowInterface.Objects.WowObjects.OfType<WowPlayer>().Where(e => !e.IsDead);

            if (wowPlayers.Any())
            {
                WowUnit[] playersToTry =
                {
                    Config.FollowSpecificCharacter ? wowPlayers.FirstOrDefault(p => WowInterface.Db.GetUnitName(p, out string name) && name.Equals(Config.SpecificCharacterToFollow, StringComparison.OrdinalIgnoreCase)) : null,
                    Config.FollowGroupLeader ? WowInterface.Objects.Partyleader : null,
                    Config.FollowGroupMembers ? WowInterface.Objects.Partymembers.FirstOrDefault() : null
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
            WowInterface.CharacterManager.ItemSlotsToSkip.Clear();
        }

        private void CheckForBattlegroundInvites()
        {
            if (WowInterface.XMemory.Read(WowInterface.OffsetList.BattlegroundStatus, out int bgStatus)
                && bgStatus == 2)
            {
                WowInterface.NewWowInterface.LuaAcceptBattlegroundInvite();
            }
        }

        private bool HandleAutoQuestMode(WowUnit wowPlayer)
        {
            WowUnit possibleQuestgiver = WowInterface.Objects.GetWowObjectByGuid<WowUnit>(wowPlayer.TargetGuid);

            if (possibleQuestgiver != null && (possibleQuestgiver.IsQuestgiver || possibleQuestgiver.IsGossip))
            {
                double distance = WowInterface.Player.Position.GetDistance(possibleQuestgiver.Position);

                if (distance > 32.0)
                {
                    return false;
                }

                if (distance > 4.0)
                {
                    WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, possibleQuestgiver.Position);
                    return true;
                }
                else
                {
                    if (QuestgiverRightClickEvent.Run())
                    {
                        if (!BotMath.IsFacing(WowInterface.Player.Position, WowInterface.Player.Rotation, possibleQuestgiver.Position))
                        {
                            WowInterface.NewWowInterface.WowFacePosition(WowInterface.Player.BaseAddress, WowInterface.Player.Position, possibleQuestgiver.Position);
                        }

                        WowInterface.NewWowInterface.WowUnitRightClick(possibleQuestgiver.BaseAddress);
                        return true;
                    }
                }
            }

            return false;
        }

        private bool ShouldIFollowPlayer(WowUnit playerToFollow)
        {
            if (playerToFollow != null)
            {
                Vector3 pos = playerToFollow.Position;

                if (Config.FollowPositionDynamic)
                {
                    pos += StateMachine.GetState<StateFollowing>().Offset;
                }

                double distance = pos.GetDistance(WowInterface.Player.Position);

                if (distance > Config.MinFollowDistance && distance < Config.MaxFollowDistance)
                {
                    return true;
                }
            }

            return false;
        }
    }
}