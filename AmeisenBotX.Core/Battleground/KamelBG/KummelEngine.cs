using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
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

        public KummelEngine(WowInterface wowInterface)
        {
            WowInterface = wowInterface;
            wowInterface.EventHookManager.Subscribe("CHAT_MSG_BG_SYSTEM_ALLIANCE", OnFlagAlliance);
            wowInterface.EventHookManager.Subscribe("CHAT_MSG_BG_SYSTEM_HORDE", OnFlagAlliance);
            wowInterface.EventHookManager.Subscribe("CHAT_MSG_BG_SYSTEM_NEUTRAL", OnFlagAlliance);
        }

        public string Author => "Kamel";

        public string Description => "...";

        public bool IsCirclePath => true;

        public string Name => "Kummel Engine";

        private WowInterface WowInterface { get; }

        public void Enter()
        {
        }

        public void Execute()
        {
            WowInterface.CombatClass.OutOfCombatExecute();
            if (WowInterface.ObjectManager.Player.IsCasting)
            {
                return;
            }
            // set new state
            if (hasStateChanged)
            {
                hasStateChanged = false;
                //hasFlag = WowInterface.HookManager.GetBuffs(WowLuaUnit.Player).Any(e => e.Contains("flag") || e.Contains("Flag"));
                hasFlag = WowInterface.ObjectManager.Player.Auras != null && WowInterface.ObjectManager.Player.Auras.Any(e => e.SpellId == 23333 || e.SpellId == 23335);
                FlagCarrier = hasFlag ? WowInterface.ObjectManager.Player : GetFlagCarrier();
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
                        WowInterface.HookManager.LuaSendChatMessage("/y The flag just lies around! Let's take it!");
                    }
                }
                else
                {
                    FlagState = PICKED_UP;
                    if (hasFlag || WowInterface.HookManager.WowGetUnitReaction(WowInterface.ObjectManager.Player, FlagCarrier) == WowUnitReaction.Friendly || WowInterface.HookManager.WowGetUnitReaction(WowInterface.ObjectManager.Player, FlagCarrier) == WowUnitReaction.Neutral)
                    {
                        FlagState |= OWN_TEAM_FLAG;
                        WowInterface.HookManager.LuaSendChatMessage("/y We got it!");
                    }
                    else
                    {
                        WowInterface.HookManager.LuaSendChatMessage("/y They've got the flag!");
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
                            WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Move, ownFlag.Position);
                            if (WowInterface.Player.Position.GetDistance(ownFlag.Position) < 3.5f)
                            {
                                WowInterface.HookManager.WowObjectRightClick(FlagObject);
                            }
                        }
                        else
                        {
                            WowUnit enemyFlagCarrier = GetFlagCarrier();
                            if (enemyFlagCarrier != null)
                            {
                                WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Move, enemyFlagCarrier.Position);
                            }
                            else if (startPosition != default)
                            {
                                WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Move, ausgangAlly);
                            }
                        }
                    }
                    else if (FlagCarrier != null)
                    {
                        FlagCarrier = GetFlagCarrier();
                        if (FlagCarrier != null)
                        {
                            WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Move, FlagCarrier.Position);
                        }
                        else
                        {
                            WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Move, baseHord);
                        }
                    }
                }
                else
                {
                    // enemy team has flag
                    FlagCarrier = GetFlagCarrier();
                    if (FlagCarrier != null)
                    {
                        WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Move, FlagCarrier.Position);
                    }
                    else
                    {
                        WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Move, baseHord);
                    }
                }
            }
            else if (FlagObject != null)
            {
                // flag lies on the ground
                WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Move, FlagObject.Position);
                if (WowInterface.Player.Position.GetDistance(FlagObject.Position) < 3.5f) // limit the executions
                {
                    WowInterface.HookManager.WowObjectRightClick(FlagObject);
                }
            }
            else if (startPosition != default)
            {
                WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Move, ausgangHord);
            }
        }

        public void Leave()
        {
        }

        private WowUnit GetFlagCarrier()
        {
            List<WowUnit> flagCarrierList = WowInterface.ObjectManager.WowObjects.OfType<WowUnit>().Where(e => e.Guid != WowInterface.ObjectManager.Player.Guid && e.Auras != null && e.Auras.Any(en => en.Name.Contains("Flag") || en.Name.Contains("flag"))).ToList();
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
            WowGameobjectDisplayId targetFlag = hasFlag ? (WowInterface.ObjectManager.Player.IsHorde() ? WowGameobjectDisplayId.WsgHordeFlag : WowGameobjectDisplayId.WsgAllianceFlag) : (WowInterface.ObjectManager.Player.IsHorde() ? WowGameobjectDisplayId.WsgAllianceFlag : WowGameobjectDisplayId.WsgHordeFlag);
            List<WowGameobject> flagObjectList = WowInterface.ObjectManager.WowObjects
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
                startPosition = WowInterface.ObjectManager.Player.Position;
            }
        }
    }
}