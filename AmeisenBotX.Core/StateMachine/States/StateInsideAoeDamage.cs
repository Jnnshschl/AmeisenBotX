using AmeisenBotX.Core.Data.Objects.WowObjects;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

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
                    && WowInterface.HookManager.GetUnitReaction(WowInterface.ObjectManager.Player, WowInterface.ObjectManager.GetWowObjectByGuid<WowUnit>(e.Caster)) != WowUnitReaction.Friendly);

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
                          && WowInterface.HookManager.GetUnitReaction(WowInterface.ObjectManager.Player, WowInterface.ObjectManager.GetWowObjectByGuid<WowUnit>(e.Caster)) != WowUnitReaction.Friendly);
        }

        public override void Leave()
        {
        }

        private Vector3 FindPositionOutsideOfAoeSpell(Vector3 aoePosition, float aoeRadius)
        {
            float angleX = WowInterface.ObjectManager.Player.Position.X - aoePosition.X;
            float angleY = WowInterface.ObjectManager.Player.Position.Y - aoePosition.Y;

            float angle = MathF.Atan2(angleX, angleY);

            float distanceToMove = aoeRadius - WowInterface.ObjectManager.Player.Position.GetDistance(aoePosition) + 6f;

            float x = WowInterface.ObjectManager.Player.Position.X + (MathF.Cos(angle) * distanceToMove);
            float y = WowInterface.ObjectManager.Player.Position.Y + (MathF.Sin(angle) * distanceToMove);

            return new Vector3(x, y, WowInterface.ObjectManager.Player.Position.Z);
        }
    }
}