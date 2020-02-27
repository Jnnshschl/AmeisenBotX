using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Movement;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Pathfinding;
using AmeisenBotX.Pathfinding.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.StateMachine.States
{
    public class StateInsideAoeDamage : BasicState
    {
        public StateInsideAoeDamage(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, ObjectManager objectManager, CharacterManager characterManager, IPathfindingHandler pathfindingHandler, IMovementEngine movementEngine) : base(stateMachine)
        {
            Config = config;
            ObjectManager = objectManager;
            CharacterManager = characterManager;
            PathfindingHandler = pathfindingHandler;
            MovementEngine = movementEngine;
        }

        private CharacterManager CharacterManager { get; }

        private AmeisenBotConfig Config { get; }

        private List<int> FriendlySpells { get; } = new List<int>() {
            0
        };

        private IMovementEngine MovementEngine { get; }

        private ObjectManager ObjectManager { get; }

        private IPathfindingHandler PathfindingHandler { get; }

        public override void Enter()
        {
        }

        public override void Execute()
        {
            if (ObjectManager != null
                && ObjectManager.Player != null)
            {
                // TODO: exclude friendly spells like Circle of Healing
                WowDynobject aoeSpellObject = ObjectManager.WowObjects.OfType<WowDynobject>()
                    .FirstOrDefault(e => FriendlySpells.Contains(e.SpellId) && e.Position.GetDistance2D(ObjectManager.Player.Position) < e.Radius + 1);

                if (aoeSpellObject == null)
                {
                    AmeisenBotStateMachine.SetState(AmeisenBotStateMachine.LastState);
                    return;
                }

                Vector3 targetPosition = FindPositionOutsideOfAoeSpell(aoeSpellObject.Position, aoeSpellObject.Radius);

                MovementEngine.SetState(MovementEngineState.Moving, targetPosition);
                MovementEngine.Execute();
            }
        }

        public override void Exit()
        {
        }

        private Vector3 FindPositionOutsideOfAoeSpell(Vector3 aoePosition, float aoeRadius)
        {
            double angleX = ObjectManager.Player.Position.X - aoePosition.X;
            double angleY = ObjectManager.Player.Position.Y - aoePosition.Y;

            double angle = Math.Atan2(angleX, angleY);

            double distanceToMove = aoeRadius - ObjectManager.Player.Position.GetDistance2D(aoePosition) + 3;

            double x = ObjectManager.Player.Position.X + (Math.Cos(angle) * distanceToMove);
            double y = ObjectManager.Player.Position.Y + (Math.Sin(angle) * distanceToMove);

            return new Vector3()
            {
                X = Convert.ToSingle(x),
                Y = Convert.ToSingle(y),
                Z = ObjectManager.Player.Position.Z
            };
        }
    }
}