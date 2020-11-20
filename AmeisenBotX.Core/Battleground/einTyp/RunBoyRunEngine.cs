using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObjects;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Battleground.einTyp
{
    public class RunBoyRunEngine : IBattlegroundEngine
    {
        private Vector3 baseAlly = new Vector3(1539, 1481, 352);
        private Vector3 baseHord = new Vector3(916, 1434, 346);
        private WowObject enemyFlag;
        private ulong EnemyFlagCarrierGuid;
        private bool enemyTeamHasFlag = false;
        private bool hasFlag = false;
        private bool hasStateChanged = true;
        private bool isHorde = false;
        private WowObject ownFlag;
        private bool ownTeamHasFlag = false;
        private ulong TeamFlagCarrierGuid;
        private WowInterface WowInterface;

        public RunBoyRunEngine(WowInterface wowInterface)
        {
            this.WowInterface = wowInterface;
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
            this.isHorde = WowInterface.ObjectManager.Player.IsHorde();
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
                WowUnit teamFlagCarrier = GetTeamFlagCarrier();
                ownTeamHasFlag = teamFlagCarrier != null;
                if (ownTeamHasFlag)
                    TeamFlagCarrierGuid = teamFlagCarrier.Guid;
                WowUnit enemyFlagCarrier = GetEnemyFlagCarrier();
                enemyTeamHasFlag = enemyFlagCarrier != null;
                if (enemyTeamHasFlag)
                    EnemyFlagCarrierGuid = enemyFlagCarrier.Guid;
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
                    WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving, ownFlag.Position);
                    if (isAtPosition(ownFlag.Position))
                    {
                        // own flag reached, save it!
                        WowInterface.HookManager.WowObjectRightClick(ownFlag);
                        hasStateChanged = true;
                    }
                }
                else
                {
                    // bring it outside!
                    WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving, this.isHorde ? baseHord : baseAlly);
                }
            }
            else if (ownTeamHasFlag && enemyTeamHasFlag)
            {
                // team mate and enemy got the flag
                if (WowInterface.CombatClass.Role == Statemachine.Enums.CombatClassRole.Dps)
                {
                    // run to the enemy
                    WowUnit enemyFlagCarrier = WowInterface.ObjectManager.GetWowObjectByGuid<WowUnit>(EnemyFlagCarrierGuid);
                    if (enemyFlagCarrier != null)
                    {
                        WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving,
                            BotUtils.MoveAhead(enemyFlagCarrier.Position, enemyFlagCarrier.Rotation, ((float)WowInterface.ObjectManager.Player.Position.GetDistance2D(enemyFlagCarrier.Position)) / 2f));
                        if (isInCombatReach(enemyFlagCarrier.Position))
                            WowInterface.HookManager.WowTargetGuid(enemyFlagCarrier.Guid);
                        if (isEnemyClose())
                        {
                            WowInterface.Globals.ForceCombat = true;
                            return;
                        }
                    }
                    else
                        WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving, this.isHorde ? baseAlly : baseHord);
                    WowInterface.CombatClass.OutOfCombatExecute();
                }
                else
                {
                    // run to the own flag carrier
                    WowUnit teamFlagCarrier = WowInterface.ObjectManager.GetWowObjectByGuid<WowUnit>(TeamFlagCarrierGuid);
                    if (teamFlagCarrier != null)
                    {
                        if (WowInterface.CombatClass.Role == Statemachine.Enums.CombatClassRole.Dps)
                            WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving, BotMath.CalculatePositionBehind(teamFlagCarrier.Position, teamFlagCarrier.Rotation, 1f));
                        else if (WowInterface.CombatClass.Role == Statemachine.Enums.CombatClassRole.Tank)
                            WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving, BotUtils.MoveAhead(teamFlagCarrier.Position, teamFlagCarrier.Rotation, 2f));
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
                        // run to the enemy
                        WowUnit enemyFlagCarrier = WowInterface.ObjectManager.GetWowObjectByGuid<WowUnit>(EnemyFlagCarrierGuid);
                        if (enemyFlagCarrier != null)
                        {
                            WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving,
                                BotUtils.MoveAhead(enemyFlagCarrier.Position, enemyFlagCarrier.Rotation, ((float)WowInterface.ObjectManager.Player.Position.GetDistance2D(enemyFlagCarrier.Position)) / 2f));
                            if (WowInterface.CombatClass.Role != Statemachine.Enums.CombatClassRole.Heal && isInCombatReach(enemyFlagCarrier.Position))
                                WowInterface.HookManager.WowTargetGuid(enemyFlagCarrier.Guid);
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
                        WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving, BotMath.CalculatePositionBehind(teamFlagCarrier.Position, teamFlagCarrier.Rotation, 1f));
                    else if (WowInterface.CombatClass.Role == Statemachine.Enums.CombatClassRole.Tank)
                        WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving, BotUtils.MoveAhead(teamFlagCarrier.Position, teamFlagCarrier.Rotation, 2f));
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
                            WowInterface.HookManager.WowObjectRightClick(enemyFlag);
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
                        WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving,
                            BotUtils.MoveAhead(enemyFlagCarrier.Position, enemyFlagCarrier.Rotation, ((float)WowInterface.ObjectManager.Player.Position.GetDistance2D(enemyFlagCarrier.Position)) / 2f));
                        if (WowInterface.CombatClass.Role != Statemachine.Enums.CombatClassRole.Heal && isInCombatReach(enemyFlagCarrier.Position))
                            WowInterface.HookManager.WowTargetGuid(enemyFlagCarrier.Guid);
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
                    WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving, enemyFlag.Position);
                    if (isAtPosition(enemyFlag.Position))
                    {
                        // flag reached, save it!
                        hasStateChanged = true;
                        WowInterface.HookManager.WowObjectRightClick(enemyFlag);
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

        public void Leave()
        {
        }

        private WowUnit GetEnemyFlagCarrier()
        {
            List<WowUnit> flagCarrierList = WowInterface.ObjectManager.WowObjects.OfType<WowUnit>().Where(e => WowInterface.HookManager.WowGetUnitReaction(WowInterface.ObjectManager.Player, e) != WowUnitReaction.Friendly && WowInterface.HookManager.WowGetUnitReaction(WowInterface.ObjectManager.Player, e) != WowUnitReaction.Neutral && !e.IsDead && e.Guid != WowInterface.ObjectManager.Player.Guid && e.Auras != null && e.Auras.Any(en => en.Name.Contains("Flag") || en.Name.Contains("flag"))).ToList();
            if (flagCarrierList.Count > 0)
                return flagCarrierList[0];
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

        private WowObject GetOwnFlagObject()
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

        private WowUnit GetTeamFlagCarrier()
        {
            List<WowUnit> flagCarrierList = WowInterface.ObjectManager.WowObjects.OfType<WowUnit>().Where(e => (WowInterface.HookManager.WowGetUnitReaction(WowInterface.ObjectManager.Player, e) == WowUnitReaction.Friendly || WowInterface.HookManager.WowGetUnitReaction(WowInterface.ObjectManager.Player, e) == WowUnitReaction.Neutral) && !e.IsDead && e.Guid != WowInterface.ObjectManager.Player.Guid && e.Auras != null && e.Auras.Any(en => en.Name.Contains("Flag") || en.Name.Contains("flag"))).ToList();
            if (flagCarrierList.Count > 0)
                return flagCarrierList[0];
            else
                return null;
        }

        private bool isAtPosition(Vector3 position)
        {
            return WowInterface.ObjectManager.Player.Position.GetDistance(position) < (WowInterface.ObjectManager.Player.CombatReach * 0.75f);
        }

        private bool isEnemyClose()
        {
            return WowInterface.ObjectManager.WowObjects.OfType<WowUnit>() != null && WowInterface.ObjectManager.WowObjects.OfType<WowUnit>().Any(e => WowInterface.ObjectManager.Player.Position.GetDistance(e.Position) < 49 && !e.IsDead && !(e.Health < 1) && WowInterface.HookManager.WowGetUnitReaction(WowInterface.ObjectManager.Player, e) != WowUnitReaction.Friendly && WowInterface.HookManager.WowGetUnitReaction(WowInterface.ObjectManager.Player, e) != WowUnitReaction.Neutral);
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

        private bool isInCombatReach(Vector3 position)
        {
            return WowInterface.ObjectManager.Player.Position.GetDistance(position) < 50;
        }

        private void OnFlagAlliance(long timestamp, List<string> args)
        {
            hasStateChanged = true;
        }
    }
}