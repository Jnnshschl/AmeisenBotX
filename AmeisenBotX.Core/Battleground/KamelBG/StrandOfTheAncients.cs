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
        public StrandOfTheAncients(AmeisenBotInterfaces bot)
        {
            Bot = bot;

            CombatEvent = new(TimeSpan.FromSeconds(2));
        }

        public string Author => "Lukas";

        public string Description => "Strand of the Ancients";

        public string Name => "Strand of the Ancients";

        public List<Vector3> PathRight { get; } = new()
        {
            new(1403, 69, 30)
        };

        public AmeisenBotInterfaces Bot { get; }

        private TimegatedEvent CombatEvent { get; }

        public void Combat()
        {
            WowPlayer weakestPlayer = Bot.Objects.GetNearEnemies<WowPlayer>(Bot.Db.GetReaction, Bot.Player.Position, 30.0f).OrderBy(e => e.Health).FirstOrDefault();

            if (weakestPlayer != null)
            {
                double distance = weakestPlayer.Position.GetDistance(Bot.Player.Position);
                double threshold = Bot.CombatClass.IsMelee ? 3.0 : 28.0;

                if (distance > threshold)
                {
                    Bot.Movement.SetMovementAction(MovementAction.Move, weakestPlayer.Position);
                }
                else if (CombatEvent.Run())
                {
                    Bot.Globals.ForceCombat = true;
                    Bot.Wow.WowTargetGuid(weakestPlayer.Guid);
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

            if (Bot.Objects.Vehicle == null)
            {
                WowGameobject VehicleNode = Bot.Objects.WowObjects
                    .OfType<WowGameobject>()
                    .Where(x => Enum.IsDefined(typeof(Vehicle), x.DisplayId)
                            && x.Position.GetDistance(Bot.Player.Position) < 20)
                    .OrderBy(x => x.Position.GetDistance(Bot.Player.Position))
                    .FirstOrDefault();

                if (VehicleNode != null)
                {
                    Bot.Movement.SetMovementAction(MovementAction.Move, VehicleNode.Position);

                    if (Bot.Player.Position.GetDistance(VehicleNode.Position) <= 4)
                    {
                        Bot.Movement.StopMovement();

                        Bot.Wow.WowObjectRightClick(VehicleNode.BaseAddress);
                    }
                }
            }
            else
            {
                Vector3 currentNode = PathRight[0];
                Bot.Movement.SetMovementAction(MovementAction.Move, currentNode);
            }
        }

        public void Leave()
        {
        }
    }
}