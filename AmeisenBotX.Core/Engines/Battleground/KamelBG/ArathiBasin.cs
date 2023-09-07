using AmeisenBotX.Common.Engines.Battleground.Interfaces;
using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Engines.Battleground.KamelBG.Enums;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Wow.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Battleground.KamelBG
{
    internal class ArathiBasin : IBattlegroundEngine
    {
        public ArathiBasin(AmeisenBotInterfaces bot)
        {
            Bot = bot;

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

        private AmeisenBotInterfaces Bot { get; }

        private TimegatedEvent CaptureFlagEvent { get; }

        private TimegatedEvent CombatEvent { get; }

        private int CurrentNodeCounter { get; set; }

        private bool Faction { get; set; }

        private string FactionFlagState { get; set; }

        public void Combat()
        {
            IWowPlayer weakestPlayer = Bot.GetNearEnemies<IWowPlayer>(Bot.Player.Position, 30.0f).OrderBy(e => e.Health).FirstOrDefault();

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
                    // StateMachine.Get<StateCombat>().Mode = CombatMode.Force;
                    Bot.Wow.ChangeTarget(weakestPlayer.Guid);
                }
            }
            else
            {
            }
        }

        public void Enter()
        {
            GetFaction();
        }

        public void Execute()
        {
            Combat();

            IWowGameobject FlagNode = Bot.Objects.All
            .OfType<IWowGameobject>()
            .Where(x => !FlagsNodelist.Contains((Flags)x.DisplayId)
                    && Enum.IsDefined(typeof(Flags), x.DisplayId)
                    && x.Position.GetDistance(Bot.Player.Position) < 15)
            .OrderBy(x => x.Position.GetDistance(Bot.Player.Position))
            .FirstOrDefault();

            if (FlagNode != null)
            {
                Bot.Movement.SetMovementAction(MovementAction.Move, FlagNode.Position);

                if (Bot.Player.Position.GetDistance(FlagNode.Position) <= 4)
                {
                    Bot.Movement.StopMovement();

                    if (CaptureFlagEvent.Run())
                    {
                        Bot.Wow.InteractWithObject(FlagNode);
                    }
                }
                else
                {
                    Bot.Movement.SetMovementAction(MovementAction.Move, FlagNode.Position);
                }
            }
            else
            {
                if (Bot.Wow.ExecuteLuaAndRead(BotUtils.ObfuscateLua("{v:0}=\"\" for i = 1, GetNumMapLandmarks(), 1 do base, status = GetMapLandmarkInfo(i) {v:0}= {v:0}..base..\":\"..status..\";\" end"), out string result))
                {
                    //AmeisenLogger.I.Log("KAMEL_DEBUG", $"time result: {result}");

                    string[] AllBaseList = result.Split(';');

                    Vector3 currentNode = Path[CurrentNodeCounter];

                    if (AllBaseList[CurrentNodeCounter].Contains("Uncontrolled")
                        || AllBaseList[CurrentNodeCounter].Contains("In Conflict")
                        || AllBaseList[CurrentNodeCounter].Contains(FactionFlagState))
                    {
                        Bot.Movement.SetMovementAction(MovementAction.Move, currentNode);
                    }

                    if (Bot.Player.Position.GetDistance(currentNode) < 10.0f)
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
                        IEnumerable<IWowPlayer> enemiesNearFlag = Bot.GetNearEnemies<IWowPlayer>(FlagNode.Position, 40);
                        IEnumerable<IWowPlayer> friendsNearFlag = Bot.GetNearFriends<IWowPlayer>(FlagNode.Position, 40);
                        IEnumerable<IWowPlayer> friendsNearPlayer = Bot.GetNearFriends<IWowPlayer>(Bot.Player.Position, 20);

                        if (enemiesNearFlag != null)
                        {
                            if (enemiesNearFlag.Count() >= 2)
                            {
                                if (friendsNearFlag != null && (friendsNearFlag.Any() || friendsNearPlayer.Any()))
                                {
                                    Bot.Movement.SetMovementAction(MovementAction.Move, currentNode);
                                    return;
                                }
                            }
                            else
                            {
                                Bot.Movement.SetMovementAction(MovementAction.Move, currentNode);
                                return;
                            }
                        }
                    }
                    else
                    {
                        Bot.Movement.SetMovementAction(MovementAction.Move, currentNode);
                    }
                }
            }
        }

        public void GetFaction()
        {
            if (Bot.Player.IsHorde())
            {
                Faction = true;
                FactionFlagState = "Alliance Controlled";
                FlagsNodelist.Add(Flags.HordFlags);
                FlagsNodelist.Add(Flags.HordFlagsAktivate);
            }
            else
            {
                Faction = false;
                FactionFlagState = "Hord Controlled";
                FlagsNodelist.Add(Flags.AlliFlags);
                FlagsNodelist.Add(Flags.AlliFlagsAktivate);
            }
        }

        public void Reset()
        {
        }
    }
}