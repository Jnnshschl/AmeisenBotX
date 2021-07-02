using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Battleground.einTyp
{
    public class RunBoyRunEngine : IBattlegroundEngine
    {
        private readonly AmeisenBotInterfaces Bot;
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

        public RunBoyRunEngine(AmeisenBotInterfaces bot)
        {
            Bot = bot;
            bot.Events.Subscribe("CHAT_MSG_BG_SYSTEM_ALLIANCE", OnFlagAlliance);
            bot.Events.Subscribe("CHAT_MSG_BG_SYSTEM_HORDE", OnFlagAlliance);
            bot.Events.Subscribe("CHAT_MSG_BG_SYSTEM_NEUTRAL", OnFlagAlliance);
            bot.Events.Subscribe("UPDATE_BATTLEFIELD_SCORE", OnFlagAlliance);
        }

        public string Author => "einTyp";

        public string Description => "...";

        public string Name => "RunBoyRunEngine";

        public void Enter()
        {
            isHorde = Bot.Player.IsHorde();
        }

        public void Execute()
        {
            if (!IsGateOpen())
            {
                Bot.CombatClass.OutOfCombatExecute();
                return;
            }

            // --- set new state ---
            if (hasStateChanged)
            {
                hasStateChanged = false;
                hasFlag = Bot.Player.Auras != null && Bot.Player.Auras.Any(e => e.SpellId == 23333 || e.SpellId == 23335);
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
                    Bot.Movement.SetMovementAction(Movement.Enums.MovementAction.Move, ownFlag.Position);
                    if (isAtPosition(ownFlag.Position))
                    {
                        // own flag reached, save it!
                        Bot.Wow.WowObjectRightClick(ownFlag.BaseAddress);
                        hasStateChanged = true;
                    }
                }
                else
                {
                    // bring it outside!
                    Bot.Movement.SetMovementAction(Movement.Enums.MovementAction.Move, isHorde ? baseHord : baseAlly);
                }
            }
            else if (ownTeamHasFlag && enemyTeamHasFlag)
            {
                // team mate and enemy got the flag
                if (Bot.CombatClass.Role == WowRole.Dps)
                {
                    // run to the enemy
                    WowUnit enemyFlagCarrier = Bot.GetWowObjectByGuid<WowUnit>(EnemyFlagCarrierGuid);
                    if (enemyFlagCarrier != null)
                    {
                        Bot.Movement.SetMovementAction(Movement.Enums.MovementAction.Move,
                            BotUtils.MoveAhead(enemyFlagCarrier.Position, enemyFlagCarrier.Rotation, ((float)Bot.Player.Position.GetDistance2D(enemyFlagCarrier.Position)) / 2f));
                        if (isInCombatReach(enemyFlagCarrier.Position))
                        {
                            Bot.Wow.WowTargetGuid(enemyFlagCarrier.Guid);
                        }

                        if (isEnemyClose())
                        {
                            Bot.Globals.ForceCombat = true;
                            return;
                        }
                    }
                    else
                    {
                        Bot.Movement.SetMovementAction(Movement.Enums.MovementAction.Move, isHorde ? baseAlly : baseHord);
                    }

                    Bot.CombatClass.OutOfCombatExecute();
                }
                else
                {
                    // run to the own flag carrier
                    WowUnit teamFlagCarrier = Bot.GetWowObjectByGuid<WowUnit>(TeamFlagCarrierGuid);
                    if (teamFlagCarrier != null)
                    {
                        if (Bot.CombatClass.Role == WowRole.Dps)
                        {
                            Bot.Movement.SetMovementAction(Movement.Enums.MovementAction.Move, BotMath.CalculatePositionBehind(teamFlagCarrier.Position, teamFlagCarrier.Rotation, 1f));
                        }
                        else if (Bot.CombatClass.Role == WowRole.Tank)
                        {
                            Bot.Movement.SetMovementAction(Movement.Enums.MovementAction.Move, BotUtils.MoveAhead(teamFlagCarrier.Position, teamFlagCarrier.Rotation, 2f));
                        }
                        else if (Bot.CombatClass.Role == WowRole.Heal)
                        {
                            Bot.Movement.SetMovementAction(Movement.Enums.MovementAction.Move, teamFlagCarrier.Position);
                        }

                        if (isEnemyClose())
                        {
                            Bot.Globals.ForceCombat = true;
                            return;
                        }
                    }
                    else
                    {
                        // run to the enemy
                        WowUnit enemyFlagCarrier = Bot.GetWowObjectByGuid<WowUnit>(EnemyFlagCarrierGuid);
                        if (enemyFlagCarrier != null)
                        {
                            Bot.Movement.SetMovementAction(Movement.Enums.MovementAction.Move,
                                BotUtils.MoveAhead(enemyFlagCarrier.Position, enemyFlagCarrier.Rotation, ((float)Bot.Player.Position.GetDistance2D(enemyFlagCarrier.Position)) / 2f));
                            if (Bot.CombatClass.Role != WowRole.Heal && isInCombatReach(enemyFlagCarrier.Position))
                            {
                                Bot.Wow.WowTargetGuid(enemyFlagCarrier.Guid);
                            }

                            if (isEnemyClose())
                            {
                                Bot.Globals.ForceCombat = true;
                                return;
                            }
                        }
                        else
                        {
                            Bot.Movement.SetMovementAction(Movement.Enums.MovementAction.Move, isHorde ? baseHord : baseAlly);
                        }

                        Bot.CombatClass.OutOfCombatExecute();
                    }
                }
            }
            else if (ownTeamHasFlag)
            {
                // a team mate got the flag
                WowUnit teamFlagCarrier = Bot.GetWowObjectByGuid<WowUnit>(TeamFlagCarrierGuid);
                if (teamFlagCarrier != null)
                {
                    if (Bot.CombatClass.Role == WowRole.Dps)
                    {
                        Bot.Movement.SetMovementAction(Movement.Enums.MovementAction.Move, BotMath.CalculatePositionBehind(teamFlagCarrier.Position, teamFlagCarrier.Rotation, 1f));
                    }
                    else if (Bot.CombatClass.Role == WowRole.Tank)
                    {
                        Bot.Movement.SetMovementAction(Movement.Enums.MovementAction.Move, BotUtils.MoveAhead(teamFlagCarrier.Position, teamFlagCarrier.Rotation, 2f));
                    }
                    else if (Bot.CombatClass.Role == WowRole.Heal)
                    {
                        Bot.Movement.SetMovementAction(Movement.Enums.MovementAction.Move, teamFlagCarrier.Position);
                    }

                    if (isEnemyClose())
                    {
                        Bot.Globals.ForceCombat = true;
                        return;
                    }
                }
                else
                {
                    Bot.Movement.SetMovementAction(Movement.Enums.MovementAction.Move, isHorde ? baseHord : baseAlly);
                }

                if (Bot.CombatClass.Role == WowRole.Dps)
                {
                    if (isEnemyClose())
                    {
                        Bot.Globals.ForceCombat = true;
                        return;
                    }
                }
                Bot.CombatClass.OutOfCombatExecute();
            }
            else if (enemyTeamHasFlag)
            {
                // the enemy got the flag
                if (Bot.CombatClass.Role == WowRole.Tank)
                {
                    WowObject tmpFlag = getEnemyFlagObject();
                    enemyFlag = tmpFlag == null ? enemyFlag : tmpFlag;
                    if (enemyFlag != null)
                    {
                        // flag lies around
                        Bot.Movement.SetMovementAction(Movement.Enums.MovementAction.Move, enemyFlag.Position);
                        if (isAtPosition(enemyFlag.Position))
                        {
                            // flag reached, save it!
                            hasStateChanged = true;
                            Bot.Wow.WowObjectRightClick(enemyFlag.BaseAddress);
                        }
                    }
                    else
                    {
                        // go outside!
                        Bot.Movement.SetMovementAction(Movement.Enums.MovementAction.Move, isHorde ? baseAlly : baseHord);
                        Bot.CombatClass.OutOfCombatExecute();
                    }
                }
                else
                {
                    WowUnit enemyFlagCarrier = Bot.GetWowObjectByGuid<WowUnit>(EnemyFlagCarrierGuid);
                    if (enemyFlagCarrier != null)
                    {
                        Bot.Movement.SetMovementAction(Movement.Enums.MovementAction.Move,
                            BotUtils.MoveAhead(enemyFlagCarrier.Position, enemyFlagCarrier.Rotation, ((float)Bot.Player.Position.GetDistance2D(enemyFlagCarrier.Position)) / 2f));
                        if (Bot.CombatClass.Role != WowRole.Heal && isInCombatReach(enemyFlagCarrier.Position))
                        {
                            Bot.Wow.WowTargetGuid(enemyFlagCarrier.Guid);
                        }

                        if (isEnemyClose())
                        {
                            Bot.Globals.ForceCombat = true;
                            return;
                        }
                    }
                    Bot.CombatClass.OutOfCombatExecute();
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
                    Bot.Movement.SetMovementAction(Movement.Enums.MovementAction.Move, enemyFlag.Position);
                    if (isAtPosition(enemyFlag.Position))
                    {
                        // flag reached, save it!
                        hasStateChanged = true;
                        Bot.Wow.WowObjectRightClick(enemyFlag.BaseAddress);
                    }
                }
                else
                {
                    // go outside!
                    Bot.Movement.SetMovementAction(Movement.Enums.MovementAction.Move, isHorde ? baseAlly : baseHord);
                    Bot.CombatClass.OutOfCombatExecute();
                }
            }
            if (Bot.Movement.Status == Movement.Enums.MovementAction.None)
            {
                hasStateChanged = true;
                Bot.Movement.SetMovementAction(Movement.Enums.MovementAction.Move, isHorde ? baseAlly : baseHord);
                if (isEnemyClose())
                {
                    Bot.Globals.ForceCombat = true;
                    return;
                }
                Bot.CombatClass.OutOfCombatExecute();
            }
        }

        public void Leave()
        {
        }

        private WowUnit GetEnemyFlagCarrier()
        {
            List<WowUnit> flagCarrierList = Bot.Objects.WowObjects.OfType<WowUnit>().Where(e => Bot.Db.GetReaction(Bot.Player, e) != WowUnitReaction.Friendly && Bot.Db.GetReaction(Bot.Player, e) != WowUnitReaction.Neutral && !e.IsDead && e.Guid != Bot.Wow.PlayerGuid && e.Auras != null && e.Auras.Any(en => Bot.Db.GetSpellName(en.SpellId).Contains("Flag") || Bot.Db.GetSpellName(en.SpellId).Contains("flag"))).ToList();

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
            WowGameobjectDisplayId targetFlag = Bot.Player.IsHorde() ? WowGameobjectDisplayId.WsgAllianceFlag : WowGameobjectDisplayId.WsgHordeFlag;
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

        private WowObject GetOwnFlagObject()
        {
            WowGameobjectDisplayId targetFlag = Bot.Player.IsHorde() ? WowGameobjectDisplayId.WsgHordeFlag : WowGameobjectDisplayId.WsgAllianceFlag;
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

        private WowUnit GetTeamFlagCarrier()
        {
            List<WowUnit> flagCarrierList = Bot.Objects.WowObjects.OfType<WowUnit>().Where(e => (Bot.Db.GetReaction(Bot.Player, e) == WowUnitReaction.Friendly || Bot.Db.GetReaction(Bot.Player, e) == WowUnitReaction.Neutral) && !e.IsDead && e.Guid != Bot.Wow.PlayerGuid && e.Auras != null && e.Auras.Any(en => Bot.Db.GetSpellName(en.SpellId).Contains("Flag") || Bot.Db.GetSpellName(en.SpellId).Contains("flag"))).ToList();
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
            return Bot.Player.Position.GetDistance(position) < (Bot.Player.CombatReach * 0.75f);
        }

        private bool isEnemyClose()
        {
            return Bot.Objects.WowObjects.OfType<WowUnit>() != null && Bot.Objects.WowObjects.OfType<WowUnit>().Any(e => Bot.Player.Position.GetDistance(e.Position) < 49 && !e.IsDead && !(e.Health < 1) && Bot.Db.GetReaction(Bot.Player, e) != WowUnitReaction.Friendly && Bot.Db.GetReaction(Bot.Player, e) != WowUnitReaction.Neutral);
        }

        private bool IsGateOpen()
        {
            if (Bot.Player.IsAlliance())
            {
                WowGameobject obj = Bot.Objects.WowObjects.OfType<WowGameobject>()
                                    .Where(e => e.GameobjectType == WowGameobjectType.Door && e.DisplayId == 411)
                                    .FirstOrDefault();

                return obj == null || obj.Bytes0 == 0;
            }
            else
            {
                WowGameobject obj = Bot.Objects.WowObjects.OfType<WowGameobject>()
                                    .Where(e => e.GameobjectType == WowGameobjectType.Door && e.DisplayId == 850)
                                    .FirstOrDefault();

                return obj == null || obj.Bytes0 == 0;
            }
        }

        private bool isInCombatReach(Vector3 position)
        {
            return Bot.Player.Position.GetDistance(position) < 50;
        }

        private void OnFlagAlliance(long timestamp, List<string> args)
        {
            hasStateChanged = true;
        }
    }
}