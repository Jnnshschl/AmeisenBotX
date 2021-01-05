using AmeisenBotX.Core.Data.Objects.WowObjects;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Core.Battleground.KamelBG.Enums;
using System;
using System.Linq;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Common;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Battleground.KamelBG
{
    internal class ArathiBasin : IBattlegroundEngine
    {
        public string Author => "Lukas";

        public string Description => "Arathi Basin";

        public string Name => "Arathi Basin";

        private bool faction { get; set; }

        private int CurrentNodeCounter { get; set; }

        private Vector3 stable = new Vector3(1166, 1203, -56);
        private Vector3 Farm = new Vector3(803, 875, -55);
        private Vector3 LumberMill = new Vector3(852, 1151, 11);
        private Vector3 Blacksmith = new Vector3(975, 1043, -44);
        private Vector3 GoldMine = new Vector3(1144, 844, -110);

        public List<Vector3> Path { get; } = new List<Vector3>()
        {
            new Vector3(1166, 1203, -56),
            new Vector3(803, 875, -55),
            new Vector3(852, 1151, 11),
            new Vector3(975, 1043, -44),
            new Vector3(1144, 844, -110)
        };

        private TimegatedEvent CaptureFlagEvent { get; }

        private bool IsCirclePath = true;

        public List<Flags> FlagsNodelist { get; set; }

        private WowInterface WowInterface { get; }

        public ArathiBasin(WowInterface wowInterface)
        {
            WowInterface = wowInterface;

            CaptureFlagEvent = new TimegatedEvent(TimeSpan.FromSeconds(1));
            FlagsNodelist = new List<Flags>();
        }

        public void Enter()
        {
            Faction();
        }

        public void Execute()
        {
            WowGameobject FlagNode = WowInterface.ObjectManager.WowObjects
            .OfType<WowGameobject>()
            .Where(x => !FlagsNodelist.Contains((Flags)x.DisplayId) 
                    && Enum.IsDefined(typeof(Flags), x.DisplayId)
                    && x.Position.GetDistance(WowInterface.ObjectManager.Player.Position) < 160)
            .OrderBy(x => x.Position.GetDistance(WowInterface.ObjectManager.Player.Position))
            .FirstOrDefault();


            if (FlagNode != null)
            {
                WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, FlagNode.Position);

                if (WowInterface.ObjectManager.Player.Position.GetDistance(FlagNode.Position) <= 4)
                {
                    WowInterface.MovementEngine.StopMovement();

                    if (CaptureFlagEvent.Run())
                    {
                        WowInterface.HookManager.WowObjectRightClick(FlagNode);
                    }
                }
                else
                {
                    WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, FlagNode.Position);
                }
            }
            else
            {
                //Vector3 closestNode = Path.OrderBy(e => e.GetDistance(WowInterface.ObjectManager.Player.Position)).First();
                //CurrentNodeCounter = Path.IndexOf(closestNode) + 1;

                Vector3 currentNode = Path[CurrentNodeCounter];
                WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, currentNode);

                if (WowInterface.Player.Position.GetDistance(currentNode) < 3.0f)
                {
                    ++CurrentNodeCounter;

                    if (CurrentNodeCounter >= Path.Count)
                    {
                        if (!IsCirclePath)
                        {
                            Path.Reverse();
                        }

                        CurrentNodeCounter = 0;
                    }
                }
            }
        }

        public void Leave()
        {
            
        }

        public void Faction()
        {
            if (WowInterface.ObjectManager.Player.IsHorde())
            {
                faction = true;
                //FlagsNodelist.Add(Flags.NeutralFlags);
                FlagsNodelist.Add(Flags.HordFlags);
                FlagsNodelist.Add(Flags.HordFlagsAktivate);
            }
            else
            {
                faction = false;
                //FlagsNodelist.Add(Flags.NeutralFlags);
                FlagsNodelist.Add(Flags.AlliFlags);
                FlagsNodelist.Add(Flags.AlliFlagsAktivate);
            }
            
        }
    }
}
