using AmeisenBotX.Common.Engines.Battleground.Interfaces;
using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Wow.Objects;
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

        private IWowObject enemyFlag;

        private ulong EnemyFlagCarrierGuid;

        private bool enemyTeamHasFlag = false;

        private bool hasFlag = false;

        private bool hasStateChanged = true;

        private bool isHorde = false;

        private IWowObject ownFlag;

        private bool ownTeamHasFlag = false;

        private ulong TeamFlagCarrierGuid;

        public RunBoyRunEngine(AmeisenBotInterfaces bot)
        {
            Bot = bot;

            if (bot.Wow.Events != null)
            {
                bot.Wow.Events.Subscribe("CHAT_MSG_BG_SYSTEM_ALLIANCE", OnFlagAlliance);
                bot.Wow.Events.Subscribe("CHAT_MSG_BG_SYSTEM_HORDE", OnFlagAlliance);
                bot.Wow.Events.Subscribe("CHAT_MSG_BG_SYSTEM_NEUTRAL", OnFlagAlliance);
                bot.Wow.Events.Subscribe("UPDATE_BATTLEFIELD_SCORE", OnFlagAlliance);
            }
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
                Bot.CombatClass?.OutOfCombatExecute();
                return;
            }

            // --- set new state ---
            if (hasStateChanged)
            {
                hasStateChanged = false;
                hasFlag = Bot.Player.Auras != null && Bot.Player.Auras.Any(e => e.SpellId == 23333 || e.SpellId == 23335);
                IWowUnit teamFlagCarrier = GetTeamFlagCarrier();
                ownTeamHasFlag = teamFlagCarrier != null;
                if (ownTeamHasFlag)
                {
                    TeamFlagCarrierGuid = teamFlagCarrier.Guid;
                }

                IWowUnit enemyFlagCarrier = GetEnemyFlagCarrier();
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
                IWowObject tmpFlag = GetOwnFlagObject();
                ownFlag = tmpFlag ?? ownFlag;
                if (ownFlag != null)
                {
                    // own flag lies around
                    Bot.Movement.SetMovementAction(Movement.Enums.MovementAction.Move, ownFlag.Position);
                    if (IsAtPosition(ownFlag.Position))
                    {
                        // own flag reached, save it!
                        Bot.Wow.InteractWithObject(ownFlag);
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
                    IWowUnit enemyFlagCarrier = Bot.GetWowObjectByGuid<IWowUnit>(EnemyFlagCarrierGuid);
                    if (enemyFlagCarrier != null)
                    {
                        Bot.Movement.SetMovementAction(Movement.Enums.MovementAction.Move,
                            BotUtils.MoveAhead(enemyFlagCarrier.Position, enemyFlagCarrier.Rotation, ((float)Bot.Player.Position.GetDistance2D(enemyFlagCarrier.Position)) / 2f));
                        if (IsInCombatReach(enemyFlagCarrier.Position))
                        {
                            Bot.Wow.ChangeTarget(enemyFlagCarrier.Guid);
                        }

                        if (IsEnemyClose())
                        {
                            // StateMachine.Get<StateCombat>().Mode = CombatMode.Force;
                            return;
                        }
                    }
                    else
                    {
                        Bot.Movement.SetMovementAction(Movement.Enums.MovementAction.Move, isHorde ? baseAlly : baseHord);
                    }

                    Bot.CombatClass?.OutOfCombatExecute();
                }
                else
                {
                    // run to the own flag carrier
                    IWowUnit teamFlagCarrier = Bot.GetWowObjectByGuid<IWowUnit>(TeamFlagCarrierGuid);
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

                        if (IsEnemyClose())
                        {
                            // StateMachine.Get<StateCombat>().Mode = CombatMode.Force;
                            return;
                        }
                    }
                    else
                    {
                        // run to the enemy
                        IWowUnit enemyFlagCarrier = Bot.GetWowObjectByGuid<IWowUnit>(EnemyFlagCarrierGuid);
                        if (enemyFlagCarrier != null)
                        {
                            Bot.Movement.SetMovementAction(Movement.Enums.MovementAction.Move,
                                BotUtils.MoveAhead(enemyFlagCarrier.Position, enemyFlagCarrier.Rotation, ((float)Bot.Player.Position.GetDistance2D(enemyFlagCarrier.Position)) / 2f));
                            if (Bot.CombatClass.Role != WowRole.Heal && IsInCombatReach(enemyFlagCarrier.Position))
                            {
                                Bot.Wow.ChangeTarget(enemyFlagCarrier.Guid);
                            }

                            if (IsEnemyClose())
                            {
                                // StateMachine.Get<StateCombat>().Mode = CombatMode.Force;
                                return;
                            }
                        }
                        else
                        {
                            Bot.Movement.SetMovementAction(Movement.Enums.MovementAction.Move, isHorde ? baseHord : baseAlly);
                        }

                        Bot.CombatClass?.OutOfCombatExecute();
                    }
                }
            }
            else if (ownTeamHasFlag)
            {
                // a team mate got the flag
                IWowUnit teamFlagCarrier = Bot.GetWowObjectByGuid<IWowUnit>(TeamFlagCarrierGuid);
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

                    if (IsEnemyClose())
                    {
                        // StateMachine.Get<StateCombat>().Mode = CombatMode.Force;
                        return;
                    }
                }
                else
                {
                    Bot.Movement.SetMovementAction(Movement.Enums.MovementAction.Move, isHorde ? baseHord : baseAlly);
                }

                if (Bot.CombatClass.Role == WowRole.Dps)
                {
                    if (IsEnemyClose())
                    {
                        // StateMachine.Get<StateCombat>().Mode = CombatMode.Force;
                        return;
                    }
                }
                Bot.CombatClass?.OutOfCombatExecute();
            }
            else if (enemyTeamHasFlag)
            {
                // the enemy got the flag
                if (Bot.CombatClass.Role == WowRole.Tank)
                {
                    IWowObject tmpFlag = GetEnemyFlagObject();
                    enemyFlag = tmpFlag ?? enemyFlag;
                    if (enemyFlag != null)
                    {
                        // flag lies around
                        Bot.Movement.SetMovementAction(Movement.Enums.MovementAction.Move, enemyFlag.Position);
                        if (IsAtPosition(enemyFlag.Position))
                        {
                            // flag reached, save it!
                            hasStateChanged = true;
                            Bot.Wow.InteractWithObject(enemyFlag);
                        }
                    }
                    else
                    {
                        // go outside!
                        Bot.Movement.SetMovementAction(Movement.Enums.MovementAction.Move, isHorde ? baseAlly : baseHord);
                        Bot.CombatClass?.OutOfCombatExecute();
                    }
                }
                else
                {
                    IWowUnit enemyFlagCarrier = Bot.GetWowObjectByGuid<IWowUnit>(EnemyFlagCarrierGuid);
                    if (enemyFlagCarrier != null)
                    {
                        Bot.Movement.SetMovementAction(Movement.Enums.MovementAction.Move,
                            BotUtils.MoveAhead(enemyFlagCarrier.Position, enemyFlagCarrier.Rotation, ((float)Bot.Player.Position.GetDistance2D(enemyFlagCarrier.Position)) / 2f));
                        if (Bot.CombatClass.Role != WowRole.Heal && IsInCombatReach(enemyFlagCarrier.Position))
                        {
                            Bot.Wow.ChangeTarget(enemyFlagCarrier.Guid);
                        }

                        if (IsEnemyClose())
                        {
                            // StateMachine.Get<StateCombat>().Mode = CombatMode.Force;
                            return;
                        }
                    }
                    Bot.CombatClass?.OutOfCombatExecute();
                }
            }
            else
            {
                // go and get the enemy flag!!!
                IWowObject tmpFlag = GetEnemyFlagObject();
                enemyFlag = tmpFlag ?? enemyFlag;
                if (enemyFlag != null)
                {
                    // flag lies around
                    Bot.Movement.SetMovementAction(Movement.Enums.MovementAction.Move, enemyFlag.Position);
                    if (IsAtPosition(enemyFlag.Position))
                    {
                        // flag reached, save it!
                        hasStateChanged = true;
                        Bot.Wow.InteractWithObject(enemyFlag);
                    }
                }
                else
                {
                    // go outside!
                    Bot.Movement.SetMovementAction(Movement.Enums.MovementAction.Move, isHorde ? baseAlly : baseHord);
                    Bot.CombatClass?.OutOfCombatExecute();
                }
            }
            if (Bot.Movement.Status == Movement.Enums.MovementAction.None)
            {
                hasStateChanged = true;
                Bot.Movement.SetMovementAction(Movement.Enums.MovementAction.Move, isHorde ? baseAlly : baseHord);
                if (IsEnemyClose())
                {
                    // StateMachine.Get<StateCombat>().Mode = CombatMode.Force;
                    return;
                }
                Bot.CombatClass?.OutOfCombatExecute();
            }
        }

        public void Reset()
        {
        }

        private IWowUnit GetEnemyFlagCarrier()
        {
            List<IWowUnit> flagCarrierList = Bot.Objects.All.OfType<IWowUnit>().Where(e =>
            Bot.Db.GetReaction(Bot.Player, e) != WowUnitReaction.Friendly
            && Bot.Db.GetReaction(Bot.Player, e) != WowUnitReaction.Neutral
            && !e.IsDead && e.Guid != Bot.Wow.PlayerGuid
            && e.Auras != null && e.Auras.Any(en =>
            Bot.Db.GetSpellName(en.SpellId).Contains("Flag") || Bot.Db.GetSpellName(en.SpellId).Contains("flag")))
            .ToList();

            if (flagCarrierList.Count > 0)
            {
                return flagCarrierList[0];
            }
            else
            {
                return null;
            }
        }

        private IWowObject GetEnemyFlagObject()
        {
            WowGameObjectDisplayId targetFlag = Bot.Player.IsHorde()
                ? WowGameObjectDisplayId.WsgAllianceFlag : WowGameObjectDisplayId.WsgHordeFlag;

            List<IWowGameobject> flagObjectList = Bot.Objects.All
                .OfType<IWowGameobject>() // only WowGameobjects
                .Where(x => Enum.IsDefined(typeof(WowGameObjectDisplayId), x.DisplayId)
                         && targetFlag == (WowGameObjectDisplayId)x.DisplayId).ToList();

            if (flagObjectList.Count > 0)
            {
                return flagObjectList[0];
            }
            else
            {
                return null;
            }
        }

        private IWowObject GetOwnFlagObject()
        {
            WowGameObjectDisplayId targetFlag = Bot.Player.IsHorde()
                ? WowGameObjectDisplayId.WsgHordeFlag : WowGameObjectDisplayId.WsgAllianceFlag;

            List<IWowGameobject> flagObjectList = Bot.Objects.All
                .OfType<IWowGameobject>() // only WowGameobjects
                .Where(x => Enum.IsDefined(typeof(WowGameObjectDisplayId), x.DisplayId)
                         && targetFlag == (WowGameObjectDisplayId)x.DisplayId).ToList();

            if (flagObjectList.Count > 0)
            {
                return flagObjectList[0];
            }
            else
            {
                return null;
            }
        }

        private IWowUnit GetTeamFlagCarrier()
        {
            List<IWowUnit> flagCarrierList = Bot.Objects.All.OfType<IWowUnit>().Where(e => (Bot.Db.GetReaction(Bot.Player, e) == WowUnitReaction.Friendly || Bot.Db.GetReaction(Bot.Player, e) == WowUnitReaction.Neutral) && !e.IsDead && e.Guid != Bot.Wow.PlayerGuid && e.Auras != null && e.Auras.Any(en => Bot.Db.GetSpellName(en.SpellId).Contains("Flag") || Bot.Db.GetSpellName(en.SpellId).Contains("flag"))).ToList();
            if (flagCarrierList.Count > 0)
            {
                return flagCarrierList[0];
            }
            else
            {
                return null;
            }
        }

        private bool IsAtPosition(Vector3 position)
        {
            return Bot.Player.Position.GetDistance(position) < (Bot.Player.CombatReach * 0.75f);
        }

        private bool IsEnemyClose()
        {
            return Bot.Objects.All.OfType<IWowUnit>() != null && Bot.Objects.All.OfType<IWowUnit>().Any(e => Bot.Player.Position.GetDistance(e.Position) < 49 && !e.IsDead && !(e.Health < 1) && Bot.Db.GetReaction(Bot.Player, e) != WowUnitReaction.Friendly && Bot.Db.GetReaction(Bot.Player, e) != WowUnitReaction.Neutral);
        }

        private bool IsGateOpen()
        {
            if (Bot.Player.IsAlliance())
            {
                IWowGameobject obj = Bot.Objects.All.OfType<IWowGameobject>()
                                    .Where(e => e.GameObjectType == WowGameObjectType.Door && e.DisplayId == 411)
                                    .FirstOrDefault();

                return obj == null || obj.Bytes0 == 0;
            }
            else
            {
                IWowGameobject obj = Bot.Objects.All.OfType<IWowGameobject>()
                                    .Where(e => e.GameObjectType == WowGameObjectType.Door && e.DisplayId == 850)
                                    .FirstOrDefault();

                return obj == null || obj.Bytes0 == 0;
            }
        }

        private bool IsInCombatReach(Vector3 position)
        {
            return Bot.Player.Position.GetDistance(position) < 50;
        }

        private void OnFlagAlliance(long timestamp, List<string> args)
        {
            hasStateChanged = true;
        }
    }
}