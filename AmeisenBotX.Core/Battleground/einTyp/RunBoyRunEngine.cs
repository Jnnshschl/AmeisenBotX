using System;
using System.Collections.Generic;
using System.Linq;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Logging;

namespace AmeisenBotX.Core.Battleground.einTyp
{
    public class RunBoyRunEngine : IBattlegroundEngine
    {
        private WowInterface WowInterface;
        private ulong EnemyFlagCarrierGuid;
        private ulong TeamFlagCarrierGuid;
        private WowObject enemyFlag;
        private WowObject ownFlag;
        private bool hasStateChanged = true;
        private bool hasFlag = false;
        private bool ownTeamHasFlag = false;
        private bool enemyTeamHasFlag = false;

        public RunBoyRunEngine(WowInterface wowInterface)
        {
            this.WowInterface = wowInterface;
            wowInterface.EventHookManager.Subscribe("CHAT_MSG_BG_SYSTEM_ALLIANCE", onFlagAlliance);
            wowInterface.EventHookManager.Subscribe("CHAT_MSG_BG_SYSTEM_HORDE", onFlagAlliance);
            wowInterface.EventHookManager.Subscribe("CHAT_MSG_BG_SYSTEM_NEUTRAL", onFlagAlliance);
            wowInterface.EventHookManager.Subscribe("UPDATE_BATTLEFIELD_SCORE", onFlagAlliance);
        }

        public string Name => "RunBoyRunEngine";

        public string Description => "...";

        public string Author => "einTyp";

        private void onFlagAlliance(long timestamp, List<string> args)
        {
            hasStateChanged = true;
        }

        public void Enter()
        {

        }

        public void Exit()
        {

        }

        public void Execute()
        {
            // --- set new state ---
            if (hasStateChanged)
            {
                hasStateChanged = false;
                hasFlag = WowInterface.ObjectManager.Player.Auras != null && WowInterface.ObjectManager.Player.Auras.Any(e => e.SpellId == 23333 || e.SpellId == 23335);
                WowUnit teamFlagCarrier = getTeamFlagCarrier();
                ownTeamHasFlag = teamFlagCarrier != null;
                if (ownTeamHasFlag)
                    TeamFlagCarrierGuid = teamFlagCarrier.Guid;
                WowUnit enemyFlagCarrier = getEnemyFlagCarrier();
                enemyTeamHasFlag = enemyFlagCarrier != null;
                if (enemyTeamHasFlag)
                    EnemyFlagCarrierGuid = enemyFlagCarrier.Guid;
            }
            
            // --- reaction ---
            if(hasFlag)
            {
                // you've got the flag!
                WowObject tmpFlag = getOwnFlagObject();
                ownFlag = tmpFlag == null ? ownFlag : tmpFlag;
                if (ownFlag != null)
                {
                    // own flag lies around
                    WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving, ownFlag.Position);
                    if (WowInterface.MovementEngine.IsAtTargetPosition)
                    {
                        // own flag reached, save it!
                        WowInterface.HookManager.WowObjectOnRightClick(ownFlag);
                        hasStateChanged = true;
                    }
                }
                else
                {
                    // bring it outside!
                    WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving, new Vector3(1051, 1398, 340));
                }
            }
            else if(ownTeamHasFlag && enemyTeamHasFlag)
            {
                // team mate and enemy got the flag
                if(WowInterface.CombatClass.Role == Statemachine.Enums.CombatClassRole.Dps)
                {
                    // run to the enemy
                    WowUnit enemyFlagCarrier = WowInterface.ObjectManager.GetWowObjectByGuid<WowUnit>(EnemyFlagCarrierGuid);
                    if(enemyFlagCarrier != null)
                    {
                        WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Chasing, enemyFlagCarrier.Position, enemyFlagCarrier.Rotation);
                        WowInterface.HookManager.TargetGuid(enemyFlagCarrier.Guid);
                    } else
                        WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving, new Vector3(1051, 1398, 340));
                    WowInterface.CombatClass.OutOfCombatExecute();
                }
                else
                {
                    // run to the own flag carrier
                    WowUnit teamFlagCarrier = WowInterface.ObjectManager.GetWowObjectByGuid<WowUnit>(TeamFlagCarrierGuid);
                    if (teamFlagCarrier != null)
                    {
                        if(WowInterface.CombatClass.Role == Statemachine.Enums.CombatClassRole.Dps)
                            WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Following, teamFlagCarrier.Position);
                        else if (WowInterface.CombatClass.Role == Statemachine.Enums.CombatClassRole.Tank)
                            WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Chasing, teamFlagCarrier.Position, teamFlagCarrier.Rotation);
                        else if (WowInterface.CombatClass.Role == Statemachine.Enums.CombatClassRole.Heal)
                            WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving, teamFlagCarrier.Position);
                    }
                    else
                    {
                        WowUnit enemyFlagCarrier = WowInterface.ObjectManager.GetWowObjectByGuid<WowUnit>(EnemyFlagCarrierGuid);
                        if (enemyFlagCarrier != null)
                        {
                            WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Chasing, enemyFlagCarrier.Position, enemyFlagCarrier.Rotation);
                            if (WowInterface.CombatClass.Role != Statemachine.Enums.CombatClassRole.Heal)
                                WowInterface.HookManager.TargetGuid(enemyFlagCarrier.Guid);
                        }
                        else
                            WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving, new Vector3(1051, 1398, 340));
                        WowInterface.CombatClass.OutOfCombatExecute();
                    }
                }
            }
            else if (ownTeamHasFlag)
            {
                // a team mate got the flag
                WowUnit teamFlagCarrier = WowInterface.ObjectManager.GetWowObjectByGuid<WowUnit>(TeamFlagCarrierGuid);
                if (teamFlagCarrier != null)
                {
                    if (WowInterface.CombatClass.Role == Statemachine.Enums.CombatClassRole.Dps)
                        WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Following, teamFlagCarrier.Position);
                    else if (WowInterface.CombatClass.Role == Statemachine.Enums.CombatClassRole.Tank)
                        WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Chasing, teamFlagCarrier.Position, teamFlagCarrier.Rotation);
                    else if (WowInterface.CombatClass.Role == Statemachine.Enums.CombatClassRole.Heal)
                        WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving, teamFlagCarrier.Position);
                }
                else
                    WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving, new Vector3(1055, 1395, 340));
                if (WowInterface.CombatClass.Role == Statemachine.Enums.CombatClassRole.Dps)
                    WowInterface.CombatClass.OutOfCombatExecute();
            }
            else if (enemyTeamHasFlag)
            {
                // the enemy got the flag
                if (WowInterface.CombatClass.Role == Statemachine.Enums.CombatClassRole.Tank)
                {
                    WowObject tmpFlag = getEnemyFlagObject();
                    enemyFlag = tmpFlag == null ? enemyFlag : tmpFlag;
                    if (enemyFlag != null)
                    {
                        // flag lies around
                        WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving, enemyFlag.Position);
                        if (WowInterface.MovementEngine.IsAtTargetPosition)
                        {
                            // flag reached, save it!
                            hasStateChanged = true;
                            WowInterface.HookManager.WowObjectOnRightClick(enemyFlag);
                        }
                    }
                    else
                    {
                        // go outside!
                        WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving, new Vector3(1055, 1395, 340));
                        WowInterface.CombatClass.OutOfCombatExecute();
                    }
                }
                else
                {
                    WowUnit enemyFlagCarrier = WowInterface.ObjectManager.GetWowObjectByGuid<WowUnit>(EnemyFlagCarrierGuid);
                    if (enemyFlagCarrier != null)
                    {
                        WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Chasing, enemyFlagCarrier.Position, enemyFlagCarrier.Rotation);
                        if (WowInterface.CombatClass.Role != Statemachine.Enums.CombatClassRole.Heal)
                            WowInterface.HookManager.TargetGuid(enemyFlagCarrier.Guid);
                    }
                    WowInterface.CombatClass.OutOfCombatExecute();
                }
            }
            else
            {
                // go and get the enemy flag!!!
                WowObject tmpFlag = getEnemyFlagObject();
                enemyFlag = tmpFlag == null ? enemyFlag : tmpFlag;
                if (enemyFlag != null)
                {
                    // flag lies around
                    WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving, enemyFlag.Position);
                    if (WowInterface.MovementEngine.IsAtTargetPosition)
                    {
                        // flag reached, save it!
                        hasStateChanged = true;
                        WowInterface.HookManager.WowObjectOnRightClick(enemyFlag);
                    }
                }
                else
                {
                    // go outside!
                    WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving, new Vector3(1055, 1395, 340));
                    WowInterface.CombatClass.OutOfCombatExecute();
                }
            }
            if (WowInterface.MovementEngine.IsAtTargetPosition || WowInterface.MovementEngine.MovementAction == Movement.Enums.MovementAction.None)
            {
                hasStateChanged = true;
                WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving, new Vector3(1055, 1395, 340));
                WowInterface.CombatClass.OutOfCombatExecute();
            }
        }

        private WowUnit getEnemyFlagCarrier()
        {
            List<WowUnit> flagCarrierList = WowInterface.ObjectManager.WowObjects.OfType<WowUnit>().Where(e => WowInterface.HookManager.GetUnitReaction(WowInterface.ObjectManager.Player, e) != WowUnitReaction.Friendly && WowInterface.HookManager.GetUnitReaction(WowInterface.ObjectManager.Player, e) != WowUnitReaction.Neutral && !e.IsDead && e.Guid != WowInterface.ObjectManager.Player.Guid && e.Auras != null && e.Auras.Any(en => en.Name.Contains("Flag") || en.Name.Contains("flag"))).ToList();
            if (flagCarrierList.Count > 0)
                return flagCarrierList[0];
            else
                return null;
        }

        private WowUnit getTeamFlagCarrier()
        {
            List<WowUnit> flagCarrierList = WowInterface.ObjectManager.WowObjects.OfType<WowUnit>().Where(e => (WowInterface.HookManager.GetUnitReaction(WowInterface.ObjectManager.Player, e) == WowUnitReaction.Friendly || WowInterface.HookManager.GetUnitReaction(WowInterface.ObjectManager.Player, e) == WowUnitReaction.Neutral) && !e.IsDead && e.Guid != WowInterface.ObjectManager.Player.Guid && e.Auras != null && e.Auras.Any(en => en.Name.Contains("Flag") || en.Name.Contains("flag"))).ToList();
            if (flagCarrierList.Count > 0)
                return flagCarrierList[0];
            else
                return null;
        }

        private WowObject getOwnFlagObject()
        {
            GameobjectDisplayId targetFlag = WowInterface.ObjectManager.Player.IsHorde() ? GameobjectDisplayId.WsgHordeFlag : GameobjectDisplayId.WsgAllianceFlag;
            List<WowGameobject> flagObjectList = WowInterface.ObjectManager.WowObjects
                .OfType<WowGameobject>() // only WowGameobjects
                .Where(x => Enum.IsDefined(typeof(GameobjectDisplayId), x.DisplayId)
                         && targetFlag == (GameobjectDisplayId)x.DisplayId).ToList();
            if (flagObjectList.Count > 0)
                return flagObjectList[0];
            else
                return null;
        }

        private WowObject getEnemyFlagObject()
        {
            GameobjectDisplayId targetFlag = WowInterface.ObjectManager.Player.IsHorde() ? GameobjectDisplayId.WsgAllianceFlag : GameobjectDisplayId.WsgHordeFlag;
            List<WowGameobject> flagObjectList = WowInterface.ObjectManager.WowObjects
                .OfType<WowGameobject>() // only WowGameobjects
                .Where(x => Enum.IsDefined(typeof(GameobjectDisplayId), x.DisplayId)
                         && targetFlag == (GameobjectDisplayId)x.DisplayId).ToList();
            if (flagObjectList.Count > 0)
                return flagObjectList[0];
            else
                return null;
        }

    }
}