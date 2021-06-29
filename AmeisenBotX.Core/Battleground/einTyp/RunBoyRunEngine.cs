﻿using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Battleground.einTyp
{
    public class RunBoyRunEngine : IBattlegroundEngine
    {
        private readonly WowInterface WowInterface;
        private Vector3 baseAlly = new(1539, 1481, 352);
        private Vector3 baseHord = new(916, 1434, 346);
        private WowObject enemyFlag;
        private ulong EnemyFlagCarrierGuid;
        private bool enemyTeamHasFlag = false;
        private bool hasFlag = false;
        private bool hasStateChanged = true;
        private bool isHorde = false;
        private WowObject ownFlag;
        private bool ownTeamHasFlag = false;
        private ulong TeamFlagCarrierGuid;

        public RunBoyRunEngine(WowInterface wowInterface)
        {
            WowInterface = wowInterface;
            wowInterface.EventHookManager.Subscribe("CHAT_MSG_BG_SYSTEM_ALLIANCE", OnFlagAlliance);
            wowInterface.EventHookManager.Subscribe("CHAT_MSG_BG_SYSTEM_HORDE", OnFlagAlliance);
            wowInterface.EventHookManager.Subscribe("CHAT_MSG_BG_SYSTEM_NEUTRAL", OnFlagAlliance);
            wowInterface.EventHookManager.Subscribe("UPDATE_BATTLEFIELD_SCORE", OnFlagAlliance);
        }

        public string Author => "einTyp";

        public string Description => "...";

        public string Name => "RunBoyRunEngine";

        public void Enter()
        {
            isHorde = WowInterface.Player.IsHorde();
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
                hasFlag = WowInterface.Player.Auras != null && WowInterface.Player.Auras.Any(e => e.SpellId == 23333 || e.SpellId == 23335);
                WowUnit teamFlagCarrier = GetTeamFlagCarrier();
                ownTeamHasFlag = teamFlagCarrier != null;
                if (ownTeamHasFlag)
                {
                    TeamFlagCarrierGuid = teamFlagCarrier.Guid;
                }

                WowUnit enemyFlagCarrier = GetEnemyFlagCarrier();
                enemyTeamHasFlag = enemyFlagCarrier != null;
                if (enemyTeamHasFlag)
                {
                    EnemyFlagCarrierGuid = enemyFlagCarrier.Guid;
                }
            }

            // --- reaction ---
            if (hasFlag)
            {
                // you've got the flag!
                WowObject tmpFlag = GetOwnFlagObject();
                ownFlag = tmpFlag ?? ownFlag;
                if (ownFlag != null)
                {
                    // own flag lies around
                    WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Move, ownFlag.Position);
                    if (isAtPosition(ownFlag.Position))
                    {
                        // own flag reached, save it!
                        WowInterface.NewWowInterface.WowObjectRightClick(ownFlag.BaseAddress);
                        hasStateChanged = true;
                    }
                }
                else
                {
                    // bring it outside!
                    WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Move, isHorde ? baseHord : baseAlly);
                }
            }
            else if (ownTeamHasFlag && enemyTeamHasFlag)
            {
                // team mate and enemy got the flag
                if (WowInterface.CombatClass.Role == WowRole.Dps)
                {
                    // run to the enemy
                    WowUnit enemyFlagCarrier = WowInterface.Objects.GetWowObjectByGuid<WowUnit>(EnemyFlagCarrierGuid);
                    if (enemyFlagCarrier != null)
                    {
                        WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Move,
                            BotUtils.MoveAhead(enemyFlagCarrier.Position, enemyFlagCarrier.Rotation, ((float)WowInterface.Player.Position.GetDistance2D(enemyFlagCarrier.Position)) / 2f));
                        if (isInCombatReach(enemyFlagCarrier.Position))
                        {
                            WowInterface.NewWowInterface.WowTargetGuid(enemyFlagCarrier.Guid);
                        }

                        if (isEnemyClose())
                        {
                            WowInterface.Globals.ForceCombat = true;
                            return;
                        }
                    }
                    else
                    {
                        WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Move, isHorde ? baseAlly : baseHord);
                    }

                    WowInterface.CombatClass.OutOfCombatExecute();
                }
                else
                {
                    // run to the own flag carrier
                    WowUnit teamFlagCarrier = WowInterface.Objects.GetWowObjectByGuid<WowUnit>(TeamFlagCarrierGuid);
                    if (teamFlagCarrier != null)
                    {
                        if (WowInterface.CombatClass.Role == WowRole.Dps)
                        {
                            WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Move, BotMath.CalculatePositionBehind(teamFlagCarrier.Position, teamFlagCarrier.Rotation, 1f));
                        }
                        else if (WowInterface.CombatClass.Role == WowRole.Tank)
                        {
                            WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Move, BotUtils.MoveAhead(teamFlagCarrier.Position, teamFlagCarrier.Rotation, 2f));
                        }
                        else if (WowInterface.CombatClass.Role == WowRole.Heal)
                        {
                            WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Move, teamFlagCarrier.Position);
                        }

                        if (isEnemyClose())
                        {
                            WowInterface.Globals.ForceCombat = true;
                            return;
                        }
                    }
                    else
                    {
                        // run to the enemy
                        WowUnit enemyFlagCarrier = WowInterface.Objects.GetWowObjectByGuid<WowUnit>(EnemyFlagCarrierGuid);
                        if (enemyFlagCarrier != null)
                        {
                            WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Move,
                                BotUtils.MoveAhead(enemyFlagCarrier.Position, enemyFlagCarrier.Rotation, ((float)WowInterface.Player.Position.GetDistance2D(enemyFlagCarrier.Position)) / 2f));
                            if (WowInterface.CombatClass.Role != WowRole.Heal && isInCombatReach(enemyFlagCarrier.Position))
                            {
                                WowInterface.NewWowInterface.WowTargetGuid(enemyFlagCarrier.Guid);
                            }

                            if (isEnemyClose())
                            {
                                WowInterface.Globals.ForceCombat = true;
                                return;
                            }
                        }
                        else
                        {
                            WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Move, isHorde ? baseHord : baseAlly);
                        }

                        WowInterface.CombatClass.OutOfCombatExecute();
                    }
                }
            }
            else if (ownTeamHasFlag)
            {
                // a team mate got the flag
                WowUnit teamFlagCarrier = WowInterface.Objects.GetWowObjectByGuid<WowUnit>(TeamFlagCarrierGuid);
                if (teamFlagCarrier != null)
                {
                    if (WowInterface.CombatClass.Role == WowRole.Dps)
                    {
                        WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Move, BotMath.CalculatePositionBehind(teamFlagCarrier.Position, teamFlagCarrier.Rotation, 1f));
                    }
                    else if (WowInterface.CombatClass.Role == WowRole.Tank)
                    {
                        WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Move, BotUtils.MoveAhead(teamFlagCarrier.Position, teamFlagCarrier.Rotation, 2f));
                    }
                    else if (WowInterface.CombatClass.Role == WowRole.Heal)
                    {
                        WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Move, teamFlagCarrier.Position);
                    }

                    if (isEnemyClose())
                    {
                        WowInterface.Globals.ForceCombat = true;
                        return;
                    }
                }
                else
                {
                    WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Move, isHorde ? baseHord : baseAlly);
                }

                if (WowInterface.CombatClass.Role == WowRole.Dps)
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
                if (WowInterface.CombatClass.Role == WowRole.Tank)
                {
                    WowObject tmpFlag = getEnemyFlagObject();
                    enemyFlag = tmpFlag == null ? enemyFlag : tmpFlag;
                    if (enemyFlag != null)
                    {
                        // flag lies around
                        WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Move, enemyFlag.Position);
                        if (isAtPosition(enemyFlag.Position))
                        {
                            // flag reached, save it!
                            hasStateChanged = true;
                            WowInterface.NewWowInterface.WowObjectRightClick(enemyFlag.BaseAddress);
                        }
                    }
                    else
                    {
                        // go outside!
                        WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Move, isHorde ? baseAlly : baseHord);
                        WowInterface.CombatClass.OutOfCombatExecute();
                    }
                }
                else
                {
                    WowUnit enemyFlagCarrier = WowInterface.Objects.GetWowObjectByGuid<WowUnit>(EnemyFlagCarrierGuid);
                    if (enemyFlagCarrier != null)
                    {
                        WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Move,
                            BotUtils.MoveAhead(enemyFlagCarrier.Position, enemyFlagCarrier.Rotation, ((float)WowInterface.Player.Position.GetDistance2D(enemyFlagCarrier.Position)) / 2f));
                        if (WowInterface.CombatClass.Role != WowRole.Heal && isInCombatReach(enemyFlagCarrier.Position))
                        {
                            WowInterface.NewWowInterface.WowTargetGuid(enemyFlagCarrier.Guid);
                        }

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
                enemyFlag = tmpFlag ?? enemyFlag;
                if (enemyFlag != null)
                {
                    // flag lies around
                    WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Move, enemyFlag.Position);
                    if (isAtPosition(enemyFlag.Position))
                    {
                        // flag reached, save it!
                        hasStateChanged = true;
                        WowInterface.NewWowInterface.WowObjectRightClick(enemyFlag.BaseAddress);
                    }
                }
                else
                {
                    // go outside!
                    WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Move, isHorde ? baseAlly : baseHord);
                    WowInterface.CombatClass.OutOfCombatExecute();
                }
            }
            if (WowInterface.MovementEngine.Status == Movement.Enums.MovementAction.None)
            {
                hasStateChanged = true;
                WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Move, isHorde ? baseAlly : baseHord);
                if (isEnemyClose())
                {
                    WowInterface.Globals.ForceCombat = true;
                    return;
                }
                WowInterface.CombatClass.OutOfCombatExecute();
            }
        }

        public void Leave()
        {
        }

        private WowUnit GetEnemyFlagCarrier()
        {
            List<WowUnit> flagCarrierList = WowInterface.Objects.WowObjects.OfType<WowUnit>().Where(e => WowInterface.Db.GetReaction(WowInterface.Player, e) != WowUnitReaction.Friendly && WowInterface.Db.GetReaction(WowInterface.Player, e) != WowUnitReaction.Neutral && !e.IsDead && e.Guid != WowInterface.Player.Guid && e.Auras != null && e.Auras.Any(en => en.Name.Contains("Flag") || en.Name.Contains("flag"))).ToList();

            if (flagCarrierList.Count > 0)
            {
                return flagCarrierList[0];
            }
            else
            {
                return null;
            }
        }

        private WowObject getEnemyFlagObject()
        {
            WowGameobjectDisplayId targetFlag = WowInterface.Player.IsHorde() ? WowGameobjectDisplayId.WsgAllianceFlag : WowGameobjectDisplayId.WsgHordeFlag;
            List<WowGameobject> flagObjectList = WowInterface.Objects.WowObjects
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

        private WowObject GetOwnFlagObject()
        {
            WowGameobjectDisplayId targetFlag = WowInterface.Player.IsHorde() ? WowGameobjectDisplayId.WsgHordeFlag : WowGameobjectDisplayId.WsgAllianceFlag;
            List<WowGameobject> flagObjectList = WowInterface.Objects.WowObjects
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

        private WowUnit GetTeamFlagCarrier()
        {
            List<WowUnit> flagCarrierList = WowInterface.Objects.WowObjects.OfType<WowUnit>().Where(e => (WowInterface.Db.GetReaction(WowInterface.Player, e) == WowUnitReaction.Friendly || WowInterface.Db.GetReaction(WowInterface.Player, e) == WowUnitReaction.Neutral) && !e.IsDead && e.Guid != WowInterface.Player.Guid && e.Auras != null && e.Auras.Any(en => en.Name.Contains("Flag") || en.Name.Contains("flag"))).ToList();
            if (flagCarrierList.Count > 0)
            {
                return flagCarrierList[0];
            }
            else
            {
                return null;
            }
        }

        private bool isAtPosition(Vector3 position)
        {
            return WowInterface.Player.Position.GetDistance(position) < (WowInterface.Player.CombatReach * 0.75f);
        }

        private bool isEnemyClose()
        {
            return WowInterface.Objects.WowObjects.OfType<WowUnit>() != null && WowInterface.Objects.WowObjects.OfType<WowUnit>().Any(e => WowInterface.Player.Position.GetDistance(e.Position) < 49 && !e.IsDead && !(e.Health < 1) && WowInterface.Db.GetReaction(WowInterface.Player, e) != WowUnitReaction.Friendly && WowInterface.Db.GetReaction(WowInterface.Player, e) != WowUnitReaction.Neutral);
        }

        private bool IsGateOpen()
        {
            if (WowInterface.Player.IsAlliance())
            {
                WowGameobject obj = WowInterface.Objects.WowObjects.OfType<WowGameobject>()
                                    .Where(e => e.GameobjectType == WowGameobjectType.Door && e.DisplayId == 411)
                                    .FirstOrDefault();

                return obj == null || obj.Bytes0 == 0;
            }
            else
            {
                WowGameobject obj = WowInterface.Objects.WowObjects.OfType<WowGameobject>()
                                    .Where(e => e.GameobjectType == WowGameobjectType.Door && e.DisplayId == 850)
                                    .FirstOrDefault();

                return obj == null || obj.Bytes0 == 0;
            }
        }

        private bool isInCombatReach(Vector3 position)
        {
            return WowInterface.Player.Position.GetDistance(position) < 50;
        }

        private void OnFlagAlliance(long timestamp, List<string> args)
        {
            hasStateChanged = true;
        }
    }
}