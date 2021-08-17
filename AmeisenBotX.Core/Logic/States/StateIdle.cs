using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Core.Logic.Enums;
using AmeisenBotX.Core.Logic.Idle;
using AmeisenBotX.Core.Logic.Idle.Actions;
using AmeisenBotX.Wow.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Logic.States
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
                new SitToChairIdleAction(bot, Config.MinFollowDistance),
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

                    Bot.Wow.LuaDoString($"SetCVar(\"maxfps\", {Config.MaxFps});SetCVar(\"maxfpsbk\", {Config.MaxFps})");
                    Bot.Wow.EnableClickToMove();
                }

                if (Config.AutoSetUlowGfxSettings)
                {
                    SetUlowGfxSettings();
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
            // we are on a battleground
            if (Bot.Memory.Read(Bot.Wow.Offsets.BattlegroundStatus, out int bgStatus)
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

            // do i need to complete/get quests
            // if (Config.AutoTalkToNearQuestgivers)
            // {
            //     IWowUnit target = Bot.GetWowObjectByGuid<IWowUnit>(unitToFollow.TargetGuid);
            // 
            //     if (target != null && unitToFollow.DistanceTo(target) < 5.0f && (target.IsQuestgiver || target.IsGossip))
            //     {
            //         StateMachine.SetState(BotState.StateTalkToQuestgivers);
            //         return;
            //     }
            // }

            // do buffing etc...
            if (Bot.CombatClass != null)
            {
                Bot.CombatClass.OutOfCombatExecute();
            }

            if (StateMachine.StateOverride is not BotState.Idle and not BotState.None)
            {
                StateMachine.SetState(StateMachine.StateOverride);
                return;
            }

            if (Config.IdleActions && IdleActionEvent.Run())
            {
                IdleActionManager.Tick(Config.Autopilot);
            }
        }

        public override void Leave()
        {
            Bot.Character.ItemSlotsToSkip.Clear();
        }

        private void SetUlowGfxSettings()
        {
            Bot.Wow.LuaDoString("SetCVar(\"gxcolorbits\",\"16\");SetCVar(\"gxdepthbits\",\"16\");SetCVar(\"skycloudlod\",\"0\");SetCVar(\"particledensity\",\"0.3\");SetCVar(\"lod\",\"0\");SetCVar(\"mapshadows\",\"0\");SetCVar(\"maxlights\",\"0\");SetCVar(\"specular\",\"0\");SetCVar(\"waterlod\",\"0\");SetCVar(\"basemip\",\"1\");SetCVar(\"shadowlevel\",\"1\")");
        }
    }
}