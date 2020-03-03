using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Pathfinding.Objects;
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

        private List<int> FriendlySpells { get; } = new List<int>()
        {
            0
        };

        public override void Enter()
        {
        }

        public override void Execute()
        {
            if (WowInterface.ObjectManager != null
                && WowInterface.ObjectManager.Player != null)
            {
                // TODO: exclude friendly spells like Circle of Healing
                WowDynobject aoeSpellObject = WowInterface.ObjectManager.WowObjects.OfType<WowDynobject>()
                    .FirstOrDefault(e => FriendlySpells.Contains(e.SpellId) && e.Position.GetDistance(WowInterface.ObjectManager.Player.Position) < e.Radius + 1);

                if (aoeSpellObject == null)
                {
                    StateMachine.SetState(StateMachine.LastState);
                    return;
                }

                Vector3 targetPosition = FindPositionOutsideOfAoeSpell(aoeSpellObject.Position, aoeSpellObject.Radius);

                WowInterface.MovementEngine.SetState(MovementEngineState.Moving, targetPosition);
                WowInterface.MovementEngine.Execute();
            }
        }

        public override void Exit()
        {
        }

        private Vector3 FindPositionOutsideOfAoeSpell(Vector3 aoePosition, float aoeRadius)
        {
            double angleX = WowInterface.ObjectManager.Player.Position.X - aoePosition.X;
            double angleY = WowInterface.ObjectManager.Player.Position.Y - aoePosition.Y;

            double angle = Math.Atan2(angleX, angleY);

            double distanceToMove = aoeRadius - WowInterface.ObjectManager.Player.Position.GetDistance(aoePosition) + 3;

            double x = WowInterface.ObjectManager.Player.Position.X + (Math.Cos(angle) * distanceToMove);
            double y = WowInterface.ObjectManager.Player.Position.Y + (Math.Sin(angle) * distanceToMove);

            return new Vector3()
            {
                X = Convert.ToSingle(x),
                Y = Convert.ToSingle(y),
                Z = WowInterface.ObjectManager.Player.Position.Z
            };
        }
    }
}