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

        public List<Vector3> Path { get; } = new List<Vector3>()
        {
            new Vector3(1166, 1203, -56),//Stable
            new Vector3(803, 875, -55),//Farm
            new Vector3(852, 1151, 11),//LumberMill
            new Vector3(975, 1043, -44),//Blacksmith
            new Vector3(1144, 844, -110)//GoldMine
        };

        private TimegatedEvent CaptureFlagEvent { get; }
        private TimegatedEvent CombatEvent { get; }


        private bool IsCirclePath = true;

        public List<Flags> FlagsNodelist { get; set; }

        private WowInterface WowInterface { get; }

        public ArathiBasin(WowInterface wowInterface)
        {
            WowInterface = wowInterface;

            CaptureFlagEvent = new TimegatedEvent(TimeSpan.FromSeconds(1));
            CombatEvent = new TimegatedEvent(TimeSpan.FromSeconds(2));
            FlagsNodelist = new List<Flags>();
        }

        public void Enter()
        {
            Faction();
        }

        public void Execute()
        {
            Combat();

            WowGameobject FlagNode = WowInterface.ObjectManager.WowObjects
            .OfType<WowGameobject>()
            .Where(x => !FlagsNodelist.Contains((Flags)x.DisplayId)
                    && Enum.IsDefined(typeof(Flags), x.DisplayId)
                    && x.Position.GetDistance(WowInterface.ObjectManager.Player.Position) < 80)
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

        public void Combat()
        {
            WowPlayer weakestPlayer = WowInterface.ObjectManager.GetNearEnemies<WowPlayer>(WowInterface.ObjectManager.Player.Position, 30.0).OrderBy(e => e.Health).FirstOrDefault();

            if (weakestPlayer != null)
            {
                double distance = weakestPlayer.Position.GetDistance(WowInterface.ObjectManager.Player.Position);
                double threshold = WowInterface.CombatClass.IsMelee ? 3.0 : 28.0;

                if (distance > threshold)
                {
                    WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, weakestPlayer.Position);
                }
                else if (CombatEvent.Run())
                {
                    WowInterface.Globals.ForceCombat = true;
                    WowInterface.HookManager.WowTargetGuid(weakestPlayer.Guid);
                }
            }
            else
            {
                
            }
        }
    }
}
