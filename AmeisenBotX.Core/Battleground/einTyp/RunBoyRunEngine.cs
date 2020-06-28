using System;
using System.Collections.Generic;
using System.Linq;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;

namespace AmeisenBotX.Core.Battleground.einTyp
{
    public class RunBoyRunEngine : IBattlegroundEngine
    {
        private WowInterface WowInterface;
        private WowUnit EnemyFlagCarrier;
        private WowUnit TeamFlagCarrier;
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
        }

        private void onFlagAlliance(long timestamp, List<string> args)
        {
            hasStateChanged = true;
        }

        public void Execute()
        {
            // set new state
            if (hasStateChanged)
            {
                hasStateChanged = false;
                hasFlag = WowInterface.ObjectManager.Player.Auras != null && WowInterface.ObjectManager.Player.Auras.Any(e => e.SpellId == 23333 || e.SpellId == 23335);
                TeamFlagCarrier = getTeamFlagCarrier();
                ownTeamHasFlag = TeamFlagCarrier != null;
                EnemyFlagCarrier = getEnemyFlagCarrier();
                enemyTeamHasFlag = EnemyFlagCarrier != null;
            }
            
            // reaction
            if(hasFlag)
            {
                // you've got the flag!
                WowObject ownFlag = getOwnFlagObject();
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
                if(WowInterface.CombatClass.Role == Statemachine.Enums.CombatClassRole.Dps)
                {
                    EnemyFlagCarrier = getEnemyFlagCarrier();
                    WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Chasing, EnemyFlagCarrier.Position, EnemyFlagCarrier.Rotation);
                    WowInterface.HookManager.TargetGuid(EnemyFlagCarrier.Guid);
                    WowInterface.CombatClass.OutOfCombatExecute();
                }
                else
                {
                    TeamFlagCarrier = getTeamFlagCarrier();
                    WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Following, TeamFlagCarrier.Position);
                    if (WowInterface.CombatClass.Role == Statemachine.Enums.CombatClassRole.Dps)
                        WowInterface.CombatClass.OutOfCombatExecute();
                }
            }
            else if (ownTeamHasFlag)
            {
                TeamFlagCarrier = getTeamFlagCarrier();
                WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Following, TeamFlagCarrier.Position);
                if (WowInterface.CombatClass.Role == Statemachine.Enums.CombatClassRole.Dps)
                    WowInterface.CombatClass.OutOfCombatExecute();
            }
            else if (enemyTeamHasFlag)
            {
                if(WowInterface.CombatClass.Role == Statemachine.Enums.CombatClassRole.Tank)
                {
                    WowObject enemyFlag = getEnemyFlagObject();
                    if (enemyFlag != null)
                    {
                        // flag lies around
                        WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving, enemyFlag.Position);
                        if (WowInterface.MovementEngine.IsAtTargetPosition)
                        {
                            // flag reached, save it!
                            hasStateChanged = true;
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
                    EnemyFlagCarrier = getEnemyFlagCarrier();
                    WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Chasing, EnemyFlagCarrier.Position, EnemyFlagCarrier.Rotation);
                    if (WowInterface.CombatClass.Role != Statemachine.Enums.CombatClassRole.Heal)
                        WowInterface.HookManager.TargetGuid(EnemyFlagCarrier.Guid);
                    WowInterface.CombatClass.OutOfCombatExecute();
                }
            }
            else
            {
                WowObject enemyFlag = getEnemyFlagObject();
                if (enemyFlag != null)
                {
                    // flag lies around
                    WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving, enemyFlag.Position);
                    if (WowInterface.MovementEngine.IsAtTargetPosition)
                    {
                        // flag reached, save it!
                        hasStateChanged = true;
                    }
                }
                else
                {
                    // go outside!
                    WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving, new Vector3(1055, 1395, 340));
                    WowInterface.CombatClass.OutOfCombatExecute();
                }
            }
            if (WowInterface.MovementEngine.IsAtTargetPosition)
                hasStateChanged = true;
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