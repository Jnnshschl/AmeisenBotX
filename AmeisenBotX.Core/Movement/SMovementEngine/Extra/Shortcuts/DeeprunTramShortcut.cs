using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System.Linq;

namespace AmeisenBotX.Core.Movement.SMovementEngine.Extra.Shortcuts
{
    public class DeeprunTramShortcut : IShortcut
    {
        public DeeprunTramShortcut(WowInterface wowInterface)
        {
            WowInterface = wowInterface;

            MinDistanceUntilWorth = EntryA.GetDistance(EntryB);
        }

        public bool Finished { get; private set; }

        public MapId MapToUseOn { get; } = MapId.EasternKingdoms;

        public double MinDistanceUntilWorth { get; }

        public bool UsedTram { get; set; }

        private (Vector3, Vector3) Entry { get; set; }

        private Vector3 EntryA { get; } = new Vector3(-8348, 518, 92);

        private Vector3 EntryB { get; } = new Vector3(-4840, -1325, 502);

        private (Vector3, Vector3) Exit { get; set; }

        private Vector3 ExitA { get; } = new Vector3(75, 2490, -4);

        private Vector3 ExitB { get; } = new Vector3(75, 10, -4);

        private (Vector3, Vector3) TramEntry { get; set; }

        private (Vector3, Vector3) TramExit { get; set; }

        private Vector3 TramExitA { get; } = new Vector3(-19, 2490, -4);

        private Vector3 TramExitB { get; } = new Vector3(-18, 9, -4);

        private WowInterface WowInterface { get; }

        public bool IsUseable(Vector3 position, Vector3 targetPosition)
        {
            if (WowInterface.ObjectManager.MapId != MapId.DeeprunTram)
            {
                Entry = EntryA.GetDistance(position) < EntryB.GetDistance(position) ? (EntryA, EntryB) : (EntryB, EntryA);
                Exit = EntryA.GetDistance(targetPosition) < EntryB.GetDistance(targetPosition) ? (ExitB, ExitA) : (ExitA, ExitB);

                TramEntry = EntryA.GetDistance(position) < EntryB.GetDistance(position) ? (TramExitA, TramExitB) : (TramExitB, TramExitA);
                TramExit = (TramEntry.Item2, TramEntry.Item1);

                return Entry.Item1 != Entry.Item2;
            }
            else
            {
                return false;
            }
        }

        public bool IsViable(Vector3 position, Vector3 targetPosition)
        {
            if (Entry == Exit)
            {
                return false;
            }
            else
            {
                return Entry.Item1.GetDistance(position) + Entry.Item2.GetDistance(targetPosition) < position.GetDistance(targetPosition);
            }
        }

        public bool UseShortcut(Vector3 position, Vector3 targetPosition, out Vector3 positionToGoTo, out bool usePathfinding)
        {
            usePathfinding = true;
            positionToGoTo = default;

            if (WowInterface.ObjectManager.MapId != MapId.DeeprunTram)
            {
                if (UsedTram)
                {
                    Finished = true;
                    return true;
                }

                double distanceToEntryA = position.GetDistance(EntryA);
                double distanceToEntryB = position.GetDistance(EntryB);

                Entry = distanceToEntryA < distanceToEntryB ? (EntryA, EntryB) : (EntryB, EntryA);
                Exit = distanceToEntryA < distanceToEntryB ? (ExitB, ExitA) : (ExitA, ExitB);

                TramEntry = distanceToEntryA < distanceToEntryB ? (TramExitA, TramExitB) : (TramExitB, TramExitA);
                TramExit = (TramEntry.Item2, TramEntry.Item1);
            }

            if (WowInterface.ObjectManager.MapId == MapId.DeeprunTram)
            {
                if (TramExit.Item1.GetDistance(position) < 10.0)
                {
                    // exit the tram
                    positionToGoTo = Exit.Item1;
                    usePathfinding = Exit.Item1.GetDistance(position) < 60.0;
                    UsedTram = true;
                }
                else
                {
                    WowGameobject tram = WowInterface.ObjectManager.WowObjects.OfType<WowGameobject>()
                        .Where(e => e.DisplayId == 3831 && e.Position.GetDistance(WowInterface.ObjectManager.Player.Position) < 48.0)
                        .OrderBy(e => e.Position.GetDistance(WowInterface.ObjectManager.Player.Position))
                        .FirstOrDefault();

                    if (tram == null)
                    {
                        if (TramEntry.Item1.GetDistance(position) > 10.0)
                        {
                            // wait for tram
                            positionToGoTo = TramEntry.Item1;
                            usePathfinding = true;
                        }
                        else
                        {
                            WowInterface.MovementEngine.StopMovement();
                        }
                    }
                    else
                    {
                        // enter the tram
                        Vector3 normalPos = new Vector3(tram.Position.X, tram.Position.Y, position.Z);

                        if (WowInterface.ObjectManager.Player.Position.GetDistance2D(normalPos) > 8.0)
                        {
                            positionToGoTo = normalPos;
                            usePathfinding = false;
                        }
                        else
                        {
                            WowInterface.MovementEngine.StopMovement();
                        }
                    }
                }

                return true;
            }
            else
            {
                // enter the deeprun tram
                positionToGoTo = Entry.Item1;
                usePathfinding = true;
                return true;
            }
        }
    }
}