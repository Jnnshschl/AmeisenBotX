using System;
using System.Collections.Generic;
using System.Linq;
using AmeisenBotX.Core.Battleground;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObjects;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
namespace AmeisenBotX.Core.Battleground.KamelBG
{
    class KummelEngine : IBattlegroundEngine
    {
        private static int PICKED_UP = 1;
        private static int OWN_TEAM_FLAG = 2;

        private WowInterface WowInterface;
        private int FlagState = 0;
        private WowUnit FlagCarrier;
        private WowObject FlagObject;
        private bool hasStateChanged = true;
        private bool hasFlag = false;
        private bool takeFlag = true;
        private Vector3 startPosition;
        private Vector3 ausgangAlly = new Vector3(1055, 1395, 340);
        private Vector3 ausgangHord = new Vector3(1051, 1398, 340);
        private Vector3 baseAlly = new Vector3(1539, 1481, 352);
        private Vector3 baseHord = new Vector3(916, 1434, 346);
        public bool IsCirclePath => true;
        private int CurrentPathCounter { get; set; }

        public KummelEngine(WowInterface wowInterface)
        {
            this.WowInterface = wowInterface;
            wowInterface.EventHookManager.Subscribe("CHAT_MSG_BG_SYSTEM_ALLIANCE", onFlagAlliance);
            wowInterface.EventHookManager.Subscribe("CHAT_MSG_BG_SYSTEM_HORDE", onFlagAlliance);
            wowInterface.EventHookManager.Subscribe("CHAT_MSG_BG_SYSTEM_NEUTRAL", onFlagAlliance);
        }

        public string Name => "Kummel Engine";

        public string Description => "...";

        public string Author => "Kamel";

        private void onFlagAlliance(long timestamp, List<string> args)
        {
            hasStateChanged = true;
            if (startPosition == null)
            {
                startPosition = WowInterface.ObjectManager.Player.Position;
            }
        }

        public void Enter()
        {

        }

        public void Leave()
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
                FlagCarrier = hasFlag ? WowInterface.ObjectManager.Player : getFlagCarrier();
                if (FlagCarrier == null)
                {
                    FlagObject = getFlagObject();
                    if (FlagObject == null)
                    {
                        hasStateChanged = true;
                    }
                    else
                    {
                        FlagState = 0;
                        WowInterface.HookManager.SendChatMessage("/y The flag just lies around! Let's take it!");
                    }
                }
                else
                {
                    FlagState = PICKED_UP;
                    if (hasFlag || WowInterface.HookManager.GetUnitReaction(WowInterface.ObjectManager.Player, FlagCarrier) == WowUnitReaction.Friendly || WowInterface.HookManager.GetUnitReaction(WowInterface.ObjectManager.Player, FlagCarrier) == WowUnitReaction.Neutral)
                    {
                        FlagState |= OWN_TEAM_FLAG;
                        WowInterface.HookManager.SendChatMessage("/y We got it!");

                    }
                    else
                    {
                        WowInterface.HookManager.SendChatMessage("/y They've got the flag!");
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
                        WowObject ownFlag = getFlagObject();
                        if (ownFlag != null)
                        {
                            WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving, ownFlag.Position);
                            if (WowInterface.MovementEngine.IsAtTargetPosition)
                            {
                                WowInterface.HookManager.WowObjectOnRightClick(FlagObject);
                            }
                        }
                        else
                        {
                            WowUnit enemyFlagCarrier = getFlagCarrier();
                            if (enemyFlagCarrier != null)
                                WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving, enemyFlagCarrier.Position);
                            else if (startPosition != null)
                            {
                                WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving, ausgangAlly);
                            }
                        }
                    }
                    else if (FlagCarrier != null)
                    {
                        FlagCarrier = getFlagCarrier();
                        if (FlagCarrier != null)
                        {
                            WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving, FlagCarrier.Position);
                        }
                        else
                        {
                            WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving, baseHord);
                        }
                    }
                }
                else
                {
                    // enemy team has flag
                    FlagCarrier = getFlagCarrier();
                    if (FlagCarrier != null)
                    {
                        WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving, FlagCarrier.Position);
                    }
                    else
                    {
                        WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving, baseHord);
                    }
                }
            }
            else if (FlagObject != null)
            {
                // flag lies on the ground
                WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving, FlagObject.Position);
                if (WowInterface.MovementEngine.IsAtTargetPosition) // limit the executions
                {
                    WowInterface.HookManager.WowObjectOnRightClick(FlagObject);
                }
            }
            else if (startPosition != null)
            {
                WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving, ausgangHord);
            }
        }

        private WowUnit getFlagCarrier()
        {
            List<WowUnit> flagCarrierList = WowInterface.ObjectManager.WowObjects.OfType<WowUnit>().Where(e => e.Guid != WowInterface.ObjectManager.Player.Guid && e.Auras != null && e.Auras.Any(en => en.Name.Contains("Flag") || en.Name.Contains("flag"))).ToList();
            if (flagCarrierList.Count > 0)
                return flagCarrierList[0];
            else
                return null;
        }

        private WowObject getFlagObject()
        {
            GameobjectDisplayId targetFlag = hasFlag ? (WowInterface.ObjectManager.Player.IsHorde() ? GameobjectDisplayId.WsgHordeFlag : GameobjectDisplayId.WsgAllianceFlag) : (WowInterface.ObjectManager.Player.IsHorde() ? GameobjectDisplayId.WsgAllianceFlag : GameobjectDisplayId.WsgHordeFlag);
            List<WowGameobject> flagObjectList = WowInterface.ObjectManager.WowObjects
                .OfType<WowGameobject>() // only WowGameobjects
                .Where(x => Enum.IsDefined(typeof(GameobjectDisplayId), x.DisplayId)
                         && targetFlag == (GameobjectDisplayId)x.DisplayId).ToList();
            if (flagObjectList.Count > 0)
                return flagObjectList[0];
            else
                return null;
        }

        private void muell ()
        { 
        
        }
    }
}
