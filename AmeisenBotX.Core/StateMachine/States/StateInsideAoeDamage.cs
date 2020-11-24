using AmeisenBotX.Core.Data.Objects.WowObjects;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Statemachine.States
{
    public class StateInsideAoeDamage : BasicState
    {
        public StateInsideAoeDamage(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine, config, wowInterface)
        {
        }

        private List<int> Blacklisted { get; } = new List<int>()
        {
            0
        };

        public override void Enter()
        {
        }

        public override void Execute()
        {
            WowDynobject aoeSpellObject = WowInterface.ObjectManager.WowObjects.OfType<WowDynobject>()
                .FirstOrDefault(e => !Blacklisted.Contains(e.SpellId)
                    && e.Position.GetDistance(WowInterface.ObjectManager.Player.Position) < e.Radius + 5.0
                    && WowInterface.HookManager.WowGetUnitReaction(WowInterface.ObjectManager.Player, WowInterface.ObjectManager.GetWowObjectByGuid<WowUnit>(e.Caster)) != WowUnitReaction.Friendly);

            if (aoeSpellObject == null)
            {
                StateMachine.SetState(StateMachine.LastState);
                return;
            }

            Vector3 targetPosition = FindPositionOutsideOfAoeSpell(aoeSpellObject.Position, aoeSpellObject.Radius);
            WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, targetPosition);
        }

        public bool IsInsideAeoDamage()
        {
            return WowInterface.ObjectManager.WowObjects.OfType<WowDynobject>()
                   .Any(e => !Blacklisted.Contains(e.SpellId)
                          && e.Position.GetDistance(WowInterface.ObjectManager.Player.Position) < e.Radius + 5.0
                          && WowInterface.HookManager.WowGetUnitReaction(WowInterface.ObjectManager.Player, WowInterface.ObjectManager.GetWowObjectByGuid<WowUnit>(e.Caster)) != WowUnitReaction.Friendly);
        }

        public override void Leave()
        {
        }

        private Vector3 FindPositionOutsideOfAoeSpell(Vector3 aoePosition, float aoeRadius)
        {
            double angleX = WowInterface.ObjectManager.Player.Position.X - aoePosition.X;
            double angleY = WowInterface.ObjectManager.Player.Position.Y - aoePosition.Y;

            double angle = Math.Atan2(angleX, angleY);

            double distanceToMove = aoeRadius - WowInterface.ObjectManager.Player.Position.GetDistance(aoePosition) + 12.0;

            double x = WowInterface.ObjectManager.Player.Position.X + (Math.Cos(angle) * distanceToMove);
            double y = WowInterface.ObjectManager.Player.Position.Y + (Math.Sin(angle) * distanceToMove);

            Vector3 targetPos = new Vector3()
            {
                X = Convert.ToSingle(x),
                Y = Convert.ToSingle(y),
                Z = WowInterface.ObjectManager.Player.Position.Z
            };

            return WowInterface.I.PathfindingHandler.GetRandomPointAround((int)WowInterface.I.ObjectManager.MapId, targetPos, 5.0f);
        }
    }
}