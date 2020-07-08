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
        private bool isHorde = false;
        private Vector3 baseAlly = new Vector3(1539, 1481, 352);
        private Vector3 baseHord = new Vector3(916, 1434, 346);

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
            this.isHorde = WowInterface.ObjectManager.Player.IsHorde();
        }

        public void Exit()
        {

        }

        public void Execute()
        {
            if (!IsGateOpen())
            {
                WowInterface.CombatClass.OutOfCombatExecute();
                return;
            }

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
                    if (isAtPosition(ownFlag.Position))
                    {
                        // own flag reached, save it!
                        WowInterface.HookManager.WowObjectOnRightClick(ownFlag);
                        hasStateChanged = true;
                    }
                }
                else
                {
                    // bring it outside!
                    WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving, this.isHorde ? baseHord : baseAlly);
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
                        WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving, enemyFlagCarrier.Position, enemyFlagCarrier.Rotation);
                        WowInterface.HookManager.TargetGuid(enemyFlagCarrier.Guid);
                        if (isEnemyClose())
                        {
                            WowInterface.Globals.ForceCombat = true;
                            return;
                        }
                    } else
                        WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving, this.isHorde ? baseAlly : baseHord);
                    WowInterface.CombatClass.OutOfCombatExecute();
                }
                else
                {
                    // run to the own flag carrier
                    WowUnit teamFlagCarrier = WowInterface.ObjectManager.GetWowObjectByGuid<WowUnit>(TeamFlagCarrierGuid);
                    if (teamFlagCarrier != null)
                    {
                        if(WowInterface.CombatClass.Role == Statemachine.Enums.CombatClassRole.Dps)
                            WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving, teamFlagCarrier.Position);
                        else if (WowInterface.CombatClass.Role == Statemachine.Enums.CombatClassRole.Tank)
                            WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving, teamFlagCarrier.Position, teamFlagCarrier.Rotation);
                        else if (WowInterface.CombatClass.Role == Statemachine.Enums.CombatClassRole.Heal)
                            WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving, teamFlagCarrier.Position);
                        if (isEnemyClose())
                        {
                            WowInterface.Globals.ForceCombat = true;
                            return;
                        }
                    }
                    else
                    {
                        WowUnit enemyFlagCarrier = WowInterface.ObjectManager.GetWowObjectByGuid<WowUnit>(EnemyFlagCarrierGuid);
                        if (enemyFlagCarrier != null)
                        {
                            WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving, enemyFlagCarrier.Position, enemyFlagCarrier.Rotation);
                            if (WowInterface.CombatClass.Role != Statemachine.Enums.CombatClassRole.Heal)
                                WowInterface.HookManager.TargetGuid(enemyFlagCarrier.Guid);
                            if (isEnemyClose())
                            {
                                WowInterface.Globals.ForceCombat = true;
                                return;
                            }
                        }
                        else
                            WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving, this.isHorde ? baseHord : baseAlly);
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
                        WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving, teamFlagCarrier.Position);
                    else if (WowInterface.CombatClass.Role == Statemachine.Enums.CombatClassRole.Tank)
                        WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving, teamFlagCarrier.Position, teamFlagCarrier.Rotation);
                    else if (WowInterface.CombatClass.Role == Statemachine.Enums.CombatClassRole.Heal)
                        WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving, teamFlagCarrier.Position);
                    if (isEnemyClose())
                    {
                        WowInterface.Globals.ForceCombat = true;
                        return;
                    }
                }
                else
                    WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving, this.isHorde ? baseHord : baseAlly);
                if (WowInterface.CombatClass.Role == Statemachine.Enums.CombatClassRole.Dps)
                {
                    if (isEnemyClose())
                    {
                        WowInterface.Globals.ForceCombat = true;
                        return;
                    }
                }
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
                        if (isAtPosition(enemyFlag.Position))
                        {
                            // flag reached, save it!
                            hasStateChanged = true;
                            WowInterface.HookManager.WowObjectOnRightClick(enemyFlag);
                        }
                    }
                    else
                    {
                        // go outside!
                        WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving, this.isHorde ? baseAlly : baseHord);
                        WowInterface.CombatClass.OutOfCombatExecute();
                    }
                }
                else
                {
                    WowUnit enemyFlagCarrier = WowInterface.ObjectManager.GetWowObjectByGuid<WowUnit>(EnemyFlagCarrierGuid);
                    if (enemyFlagCarrier != null)
                    {
                        WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving, enemyFlagCarrier.Position, enemyFlagCarrier.Rotation);
                        if (WowInterface.CombatClass.Role != Statemachine.Enums.CombatClassRole.Heal)
                            WowInterface.HookManager.TargetGuid(enemyFlagCarrier.Guid);
                        if (isEnemyClose())
                        {
                            WowInterface.Globals.ForceCombat = true;
                            return;
                        }
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
                    if (isAtPosition(enemyFlag.Position))
                    {
                        // flag reached, save it!
                        hasStateChanged = true;
                        WowInterface.HookManager.WowObjectOnRightClick(enemyFlag);
                    }
                }
                else
                {
                    // go outside!
                    WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving, this.isHorde ? baseAlly : baseHord);
                    WowInterface.CombatClass.OutOfCombatExecute();
                }
            }
            if (WowInterface.MovementEngine.MovementAction == Movement.Enums.MovementAction.None)
            {
                hasStateChanged = true;
                WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving, this.isHorde ? baseAlly : baseHord);
                if (isEnemyClose())
                {
                    WowInterface.Globals.ForceCombat = true;
                    return;
                }
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

        private bool isAtPosition(Vector3 position)
        {
            return WowInterface.ObjectManager.Player.Position.GetDistance(position) < (WowInterface.ObjectManager.Player.CombatReach * 0.75f);
        }
        private bool isInCombatReach(Vector3 position)
        {
            return WowInterface.ObjectManager.Player.Position.GetDistance(position) < 50;
        }
        private bool isEnemyClose()
        {
            return WowInterface.ObjectManager.WowObjects.OfType<WowUnit>() != null && WowInterface.ObjectManager.WowObjects.OfType<WowUnit>().Any(e => WowInterface.ObjectManager.Player.Position.GetDistance(e.Position) < 49 && !e.IsDead && !(e.Health < 1) && WowInterface.HookManager.GetUnitReaction(WowInterface.ObjectManager.Player, e) != WowUnitReaction.Friendly && WowInterface.HookManager.GetUnitReaction(WowInterface.ObjectManager.Player, e) != WowUnitReaction.Neutral);
        }
        private bool IsGateOpen()
        {
            if (WowInterface.ObjectManager.Player.IsAlliance())
            {
                WowGameobject obj = WowInterface.ObjectManager.WowObjects.OfType<WowGameobject>()
                                    .Where(e => e.GameobjectType == WowGameobjectType.Door && e.DisplayId == 411)
                                    .FirstOrDefault();

                return obj == null || obj.Bytes0 == 0;
            }
            else
            {
                WowGameobject obj = WowInterface.ObjectManager.WowObjects.OfType<WowGameobject>()
                                    .Where(e => e.GameobjectType == WowGameobjectType.Door && e.DisplayId == 850)
                                    .FirstOrDefault();

                return obj == null || obj.Bytes0 == 0;
            }
        }

    }
}