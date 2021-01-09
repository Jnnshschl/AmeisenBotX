using AmeisenBotX.Core.Data.Objects.WowObjects;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Core.Battleground.KamelBG.Enums;
using System;
using System.Linq;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Common;
using System.Collections.Generic;
using AmeisenBotX.Logging;

namespace AmeisenBotX.Core.Battleground.KamelBG
{
    internal class ArathiBasin : IBattlegroundEngine
    {
        public string Author => "Lukas";

        public string Description => "Arathi Basin";

        public string Name => "Arathi Basin";

        private bool faction { get; set; }
        private string factionFlagState { get; set; }

        private int CurrentNodeCounter { get; set; }

        public List<Vector3> Path { get; } = new List<Vector3>()
        {
            new Vector3(975, 1043, -44),//Blacksmith
            new Vector3(803, 875, -55),//Farm
            new Vector3(1144, 844, -110),//GoldMine
            new Vector3(852, 1151, 11),//LumberMill
            new Vector3(1166, 1203, -56)//Stable
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
                    && x.Position.GetDistance(WowInterface.ObjectManager.Player.Position) < 15)
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

                if (WowInterface.HookManager.WowExecuteLuaAndRead(BotUtils.ObfuscateLua("{v:0}=\"\" for i = 1, GetNumMapLandmarks(), 1 do base, status = GetMapLandmarkInfo(i) {v:0}= {v:0}..base..\":\"..status..\";\" end"), out string result))
                {
                    //AmeisenLogger.I.Log("KAMEL_DEBUG", $"time result: {result}");

                    string[] AllBaseList = result.Split(';');

                    Vector3 currentNode = Path[CurrentNodeCounter];

                    if (AllBaseList[CurrentNodeCounter].Contains("Uncontrolled")
                        || AllBaseList[CurrentNodeCounter].Contains("In Conflict")
                        || AllBaseList[CurrentNodeCounter].Contains(factionFlagState))
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
                    else if (factionFlagState != null && AllBaseList[CurrentNodeCounter].Contains(factionFlagState))
                    {
                        ++CurrentNodeCounter;
                        if (CurrentNodeCounter >= Path.Count)
                        {
                            CurrentNodeCounter = 0;
                        }
                    }
                    else if (FlagNode != null)
                    {
                        IEnumerable<WowPlayer> enemiesNearFlag = WowInterface.ObjectManager.GetNearEnemies<WowPlayer>(FlagNode.Position, 40);
                        IEnumerable<WowPlayer> friendsNearFlag = WowInterface.ObjectManager.GetNearFriends<WowPlayer>(FlagNode.Position, 40);
                        IEnumerable<WowPlayer> friendsNearPlayer = WowInterface.ObjectManager.GetNearFriends<WowPlayer>(WowInterface.ObjectManager.Player.Position, 20);

                        if (enemiesNearFlag != null)
                        {
                            if (enemiesNearFlag.Count() >= 2)
                            {
                                if (friendsNearFlag != null && (friendsNearFlag.Count() >= 1 || friendsNearPlayer.Count() >= 1))
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

        public void Leave()
        {

        }

        public void Faction()
        {
            if (WowInterface.ObjectManager.Player.IsHorde())
            {
                faction = true;
                factionFlagState = "Alliance Controlled";
                FlagsNodelist.Add(Flags.HordFlags);
                FlagsNodelist.Add(Flags.HordFlagsAktivate);
            }
            else
            {
                faction = false;
                factionFlagState = "Hord Controlled";
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
