﻿using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Battleground.KamelBG
{
    internal class KummelEngine : IBattlegroundEngine
    {
        private const int OWN_TEAM_FLAG = 2;
        private const int PICKED_UP = 1;
        private Vector3 ausgangAlly = new(1055, 1395, 340);

        private Vector3 ausgangHord = new(1051, 1398, 340);

        private Vector3 baseAlly = new(1539, 1481, 352);

        private Vector3 baseHord = new(916, 1434, 346);

        private WowUnit FlagCarrier;

        private WowObject FlagObject;

        private int FlagState = 0;

        private bool hasFlag = false;

        private bool hasStateChanged = true;

        private Vector3 startPosition;

        public KummelEngine(AmeisenBotInterfaces bot)
        {
            Bot = bot;
            bot.Events.Subscribe("CHAT_MSG_BG_SYSTEM_ALLIANCE", OnFlagAlliance);
            bot.Events.Subscribe("CHAT_MSG_BG_SYSTEM_HORDE", OnFlagAlliance);
            bot.Events.Subscribe("CHAT_MSG_BG_SYSTEM_NEUTRAL", OnFlagAlliance);
        }

        public string Author => "Kamel";

        public string Description => "...";

        public bool IsCirclePath => true;

        public string Name => "Kummel Engine";

        private AmeisenBotInterfaces Bot { get; }

        public void Enter()
        {
        }

        public void Execute()
        {
            Bot.CombatClass.OutOfCombatExecute();
            if (Bot.Player.IsCasting)
            {
                return;
            }
            // set new state
            if (hasStateChanged)
            {
                hasStateChanged = false;
                //hasFlag = Bot.NewBot.GetBuffs(WowLuaUnit.Player).Any(e => e.Contains("flag") || e.Contains("Flag"));
                hasFlag = Bot.Player.Auras != null && Bot.Player.Auras.Any(e => e.SpellId == 23333 || e.SpellId == 23335);
                FlagCarrier = hasFlag ? Bot.Player : GetFlagCarrier();
                if (FlagCarrier == null)
                {
                    FlagObject = GetFlagObject();
                    if (FlagObject == null)
                    {
                        hasStateChanged = true;
                    }
                    else
                    {
                        FlagState = 0;
                        Bot.Wow.LuaSendChatMessage("/y The flag just lies around! Let's take it!");
                    }
                }
                else
                {
                    FlagState = PICKED_UP;
                    if (hasFlag || Bot.Db.GetReaction(Bot.Player, FlagCarrier) == WowUnitReaction.Friendly || Bot.Db.GetReaction(Bot.Player, FlagCarrier) == WowUnitReaction.Neutral)
                    {
                        FlagState |= OWN_TEAM_FLAG;
                        Bot.Wow.LuaSendChatMessage("/y We got it!");
                    }
                    else
                    {
                        Bot.Wow.LuaSendChatMessage("/y They've got the flag!");
                    }
                }
            }

            // reaction
            if ((FlagState & PICKED_UP) > 0)
            {
                if ((FlagState & OWN_TEAM_FLAG) > 0)
                {
                    // own team has flag
                    if (hasFlag)
                    {
                        WowObject ownFlag = GetFlagObject();
                        if (ownFlag != null)
                        {
                            Bot.Movement.SetMovementAction(Movement.Enums.MovementAction.Move, ownFlag.Position);
                            if (Bot.Player.Position.GetDistance(ownFlag.Position) < 3.5f)
                            {
                                Bot.Wow.WowObjectRightClick(FlagObject.BaseAddress);
                            }
                        }
                        else
                        {
                            WowUnit enemyFlagCarrier = GetFlagCarrier();
                            if (enemyFlagCarrier != null)
                            {
                                Bot.Movement.SetMovementAction(Movement.Enums.MovementAction.Move, enemyFlagCarrier.Position);
                            }
                            else if (startPosition != default)
                            {
                                Bot.Movement.SetMovementAction(Movement.Enums.MovementAction.Move, ausgangAlly);
                            }
                        }
                    }
                    else if (FlagCarrier != null)
                    {
                        FlagCarrier = GetFlagCarrier();
                        if (FlagCarrier != null)
                        {
                            Bot.Movement.SetMovementAction(Movement.Enums.MovementAction.Move, FlagCarrier.Position);
                        }
                        else
                        {
                            Bot.Movement.SetMovementAction(Movement.Enums.MovementAction.Move, baseHord);
                        }
                    }
                }
                else
                {
                    // enemy team has flag
                    FlagCarrier = GetFlagCarrier();
                    if (FlagCarrier != null)
                    {
                        Bot.Movement.SetMovementAction(Movement.Enums.MovementAction.Move, FlagCarrier.Position);
                    }
                    else
                    {
                        Bot.Movement.SetMovementAction(Movement.Enums.MovementAction.Move, baseHord);
                    }
                }
            }
            else if (FlagObject != null)
            {
                // flag lies on the ground
                Bot.Movement.SetMovementAction(Movement.Enums.MovementAction.Move, FlagObject.Position);
                if (Bot.Player.Position.GetDistance(FlagObject.Position) < 3.5f) // limit the executions
                {
                    Bot.Wow.WowObjectRightClick(FlagObject.BaseAddress);
                }
            }
            else if (startPosition != default)
            {
                Bot.Movement.SetMovementAction(Movement.Enums.MovementAction.Move, ausgangHord);
            }
        }

        public void Leave()
        {
        }

        private WowUnit GetFlagCarrier()
        {
            List<WowUnit> flagCarrierList = Bot.Objects.WowObjects.OfType<WowUnit>().Where(e => e.Guid != Bot.Wow.PlayerGuid && e.Auras != null && e.Auras.Any(en => Bot.Db.GetSpellName(en.SpellId).Contains("Flag") || Bot.Db.GetSpellName(en.SpellId).Contains("flag"))).ToList();
            if (flagCarrierList.Count > 0)
            {
                return flagCarrierList[0];
            }
            else
            {
                return null;
            }
        }

        private WowObject GetFlagObject()
        {
            WowGameobjectDisplayId targetFlag = hasFlag ? (Bot.Player.IsHorde() ? WowGameobjectDisplayId.WsgHordeFlag : WowGameobjectDisplayId.WsgAllianceFlag) : (Bot.Player.IsHorde() ? WowGameobjectDisplayId.WsgAllianceFlag : WowGameobjectDisplayId.WsgHordeFlag);
            List<WowGameobject> flagObjectList = Bot.Objects.WowObjects
                .OfType<WowGameobject>() // only WowGameobjects
                .Where(x => Enum.IsDefined(typeof(WowGameobjectDisplayId), x.DisplayId)
                         && targetFlag == (WowGameobjectDisplayId)x.DisplayId).ToList();
            if (flagObjectList.Count > 0)
            {
                return flagObjectList[0];
            }
            else
            {
                return null;
            }
        }

        private void OnFlagAlliance(long timestamp, List<string> args)
        {
            hasStateChanged = true;
            if (startPosition == default)
            {
                startPosition = Bot.Player.Position;
            }
        }
    }
}