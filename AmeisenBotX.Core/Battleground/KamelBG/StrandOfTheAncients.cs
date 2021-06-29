﻿using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Battleground.KamelBG.Enums;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Movement.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Battleground.KamelBG
{
    internal class StrandOfTheAncients : IBattlegroundEngine
    {
        public StrandOfTheAncients(WowInterface wowInterface)
        {
            WowInterface = wowInterface;

            CombatEvent = new(TimeSpan.FromSeconds(2));
        }

        public string Author => "Lukas";

        public string Description => "Strand of the Ancients";

        public string Name => "Strand of the Ancients";

        public List<Vector3> PathRight { get; } = new()
        {
            new(1403, 69, 30)
        };

        public WowInterface WowInterface { get; }

        private TimegatedEvent CombatEvent { get; }

        public void Combat()
        {
            WowPlayer weakestPlayer = WowInterface.Objects.GetNearEnemies<WowPlayer>(WowInterface.Db.GetReaction, WowInterface.Player.Position, 30.0f).OrderBy(e => e.Health).FirstOrDefault();

            if (weakestPlayer != null)
            {
                double distance = weakestPlayer.Position.GetDistance(WowInterface.Player.Position);
                double threshold = WowInterface.CombatClass.IsMelee ? 3.0 : 28.0;

                if (distance > threshold)
                {
                    WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, weakestPlayer.Position);
                }
                else if (CombatEvent.Run())
                {
                    WowInterface.Globals.ForceCombat = true;
                    WowInterface.NewWowInterface.WowTargetGuid(weakestPlayer.Guid);
                }
            }
            else
            {
            }
        }

        public void Enter()
        {
        }

        public void Execute()
        {
            Combat();

            if (WowInterface.Objects.Vehicle == null)
            {
                WowGameobject VehicleNode = WowInterface.Objects.WowObjects
                    .OfType<WowGameobject>()
                    .Where(x => Enum.IsDefined(typeof(Vehicle), x.DisplayId)
                            && x.Position.GetDistance(WowInterface.Player.Position) < 20)
                    .OrderBy(x => x.Position.GetDistance(WowInterface.Player.Position))
                    .FirstOrDefault();

                if (VehicleNode != null)
                {
                    WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, VehicleNode.Position);

                    if (WowInterface.Player.Position.GetDistance(VehicleNode.Position) <= 4)
                    {
                        WowInterface.MovementEngine.StopMovement();

                        WowInterface.NewWowInterface.WowObjectRightClick(VehicleNode.BaseAddress);
                    }
                }
            }
            else
            {
                Vector3 currentNode = PathRight[0];
                WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, currentNode);
            }
        }

        public void Leave()
        {
        }
    }
}