using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Battleground.KamelBG.Enums;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Movement.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Battleground.KamelBG
{
    internal class ArathiBasin : IBattlegroundEngine
    {
        private bool IsCirclePath = true;

        public ArathiBasin(WowInterface wowInterface)
        {
            WowInterface = wowInterface;

            CaptureFlagEvent = new(TimeSpan.FromSeconds(1));
            CombatEvent = new(TimeSpan.FromSeconds(2));
            FlagsNodelist = new();
        }

        public string Author => "Lukas";

        public string Description => "Arathi Basin";

        public List<Flags> FlagsNodelist { get; set; }

        public string Name => "Arathi Basin";

        public List<Vector3> Path { get; } = new()
        {
            new(975, 1043, -44),    // Blacksmith
            new(803, 875, -55),     // Farm
            new(1144, 844, -110),   // GoldMine
            new(852, 1151, 11),     // LumberMill
            new(1166, 1203, -56)    // Stable
        };

        private TimegatedEvent CaptureFlagEvent { get; }

        private TimegatedEvent CombatEvent { get; }

        private int CurrentNodeCounter { get; set; }

        private bool faction { get; set; }

        private string FactionFlagState { get; set; }

        private WowInterface WowInterface { get; }

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
            Faction();
        }

        public void Execute()
        {
            Combat();

            WowGameobject FlagNode = WowInterface.Objects.WowObjects
            .OfType<WowGameobject>()
            .Where(x => !FlagsNodelist.Contains((Flags)x.DisplayId)
                    && Enum.IsDefined(typeof(Flags), x.DisplayId)
                    && x.Position.GetDistance(WowInterface.Player.Position) < 15)
            .OrderBy(x => x.Position.GetDistance(WowInterface.Player.Position))
            .FirstOrDefault();

            if (FlagNode != null)
            {
                WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, FlagNode.Position);

                if (WowInterface.Player.Position.GetDistance(FlagNode.Position) <= 4)
                {
                    WowInterface.MovementEngine.StopMovement();

                    if (CaptureFlagEvent.Run())
                    {
                        WowInterface.NewWowInterface.WowObjectRightClick(FlagNode.BaseAddress);
                    }
                }
                else
                {
                    WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, FlagNode.Position);
                }
            }
            else
            {
                if (WowInterface.NewWowInterface.WowExecuteLuaAndRead(BotUtils.ObfuscateLua("{v:0}=\"\" for i = 1, GetNumMapLandmarks(), 1 do base, status = GetMapLandmarkInfo(i) {v:0}= {v:0}..base..\":\"..status..\";\" end"), out string result))
                {
                    //AmeisenLogger.I.Log("KAMEL_DEBUG", $"time result: {result}");

                    string[] AllBaseList = result.Split(';');

                    Vector3 currentNode = Path[CurrentNodeCounter];

                    if (AllBaseList[CurrentNodeCounter].Contains("Uncontrolled")
                        || AllBaseList[CurrentNodeCounter].Contains("In Conflict")
                        || AllBaseList[CurrentNodeCounter].Contains(FactionFlagState))
                    {
                        WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, currentNode);
                    }

                    if (WowInterface.Player.Position.GetDistance(currentNode) < 10.0f)
                    {
                        ++CurrentNodeCounter;

                        if (CurrentNodeCounter >= Path.Count)
                        {
                            CurrentNodeCounter = 0;
                        }
                    }
                    else if (FactionFlagState != null && AllBaseList[CurrentNodeCounter].Contains(FactionFlagState))
                    {
                        ++CurrentNodeCounter;
                        if (CurrentNodeCounter >= Path.Count)
                        {
                            CurrentNodeCounter = 0;
                        }
                    }
                    else if (FlagNode != null)
                    {
                        IEnumerable<WowPlayer> enemiesNearFlag = WowInterface.Objects.GetNearEnemies<WowPlayer>(WowInterface.Db.GetReaction, FlagNode.Position, 40);
                        IEnumerable<WowPlayer> friendsNearFlag = WowInterface.Objects.GetNearFriends<WowPlayer>(WowInterface.Db.GetReaction, FlagNode.Position, 40);
                        IEnumerable<WowPlayer> friendsNearPlayer = WowInterface.Objects.GetNearFriends<WowPlayer>(WowInterface.Db.GetReaction, WowInterface.Player.Position, 20);

                        if (enemiesNearFlag != null)
                        {
                            if (enemiesNearFlag.Count() >= 2)
                            {
                                if (friendsNearFlag != null && (friendsNearFlag.Any() || friendsNearPlayer.Any()))
                                {
                                    WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, currentNode);
                                    return;
                                }
                            }
                            else
                            {
                                WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, currentNode);
                                return;
                            }
                        }
                    }
                    else
                    {
                        WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, currentNode);
                    }
                }
            }
        }

        public void Faction()
        {
            if (WowInterface.Player.IsHorde())
            {
                faction = true;
                FactionFlagState = "Alliance Controlled";
                FlagsNodelist.Add(Flags.HordFlags);
                FlagsNodelist.Add(Flags.HordFlagsAktivate);
            }
            else
            {
                faction = false;
                FactionFlagState = "Hord Controlled";
                FlagsNodelist.Add(Flags.AlliFlags);
                FlagsNodelist.Add(Flags.AlliFlagsAktivate);
            }
        }

        public void Leave()
        {
        }
    }
}