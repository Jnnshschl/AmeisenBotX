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
    internal class EyeOfTheStorm : IBattlegroundEngine
    {
        public EyeOfTheStorm(WowInterface wowInterface)
        {
            WowInterface = wowInterface;

            CaptureFlagEvent = new(TimeSpan.FromSeconds(1));
            CombatEvent = new(TimeSpan.FromSeconds(2));
        }

        public string Author => "Lukas";

        public string Description => "Eye of the Storm";

        public string Name => "Eye of the Storm";

        public List<Vector3> PathBase { get; } = new List<Vector3>()
        {
            new Vector3(2284, 1731, 1189),//Mage Tower
            new Vector3(2286, 1402, 1197),//Draenei Ruins
            new Vector3(2048, 1393, 1194),//Blood Elf Tower
            new Vector3(2043, 1729, 1189)//Fel Reaver Ruins
        };

        public List<Vector3> PathFlag { get; } = new List<Vector3>()
        {
            new Vector3(2176, 1570, 1159)//Flag
        };

        private TimegatedEvent CaptureFlagEvent { get; }

        private TimegatedEvent CombatEvent { get; }

        private int CurrentNodeCounter { get; set; }

        private string factionFlagState { get; set; }

        private WowInterface WowInterface { get; }

        public void Combat()
        {
            WowPlayer weakestPlayer = WowInterface.Objects.GetNearEnemies<WowPlayer>(WowInterface.NewWowInterface, WowInterface.Player.Position, 30.0f).OrderBy(e => e.Health).FirstOrDefault();

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
                .Where(x => Enum.IsDefined(typeof(Flags), x.DisplayId)
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
                    Vector3 currentNode = PathBase[CurrentNodeCounter];
                    string[] AllBaseList = result.Split(';');

                    if (WowInterface.Player.HasBuffById(34976))
                    {
                        if (AllBaseList[CurrentNodeCounter].Contains(factionFlagState))
                        {
                            WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, currentNode);
                        }
                        else
                        {
                            ++CurrentNodeCounter;
                            if (CurrentNodeCounter >= PathBase.Count)
                            {
                                CurrentNodeCounter = 0;
                            }
                        }
                    }
                    else
                    {
                        if (AllBaseList[CurrentNodeCounter].Contains("Uncontrolled")
                            || AllBaseList[CurrentNodeCounter].Contains("In Conflict")
                            || AllBaseList[CurrentNodeCounter].Contains(factionFlagState))
                        {
                            WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, currentNode);
                        }

                        if (WowInterface.Player.Position.GetDistance(currentNode) < 10.0f)
                        {
                            ++CurrentNodeCounter;

                            if (CurrentNodeCounter >= PathBase.Count)
                            {
                                CurrentNodeCounter = 0;
                            }
                        }
                        else if (factionFlagState != null && AllBaseList[CurrentNodeCounter].Contains(factionFlagState))
                        {
                            ++CurrentNodeCounter;
                            if (CurrentNodeCounter >= PathBase.Count)
                            {
                                CurrentNodeCounter = 0;
                            }
                        }
                        else if (FlagNode != null)
                        {
                            IEnumerable<WowPlayer> enemiesNearFlag = WowInterface.Objects.GetNearEnemies<WowPlayer>(WowInterface.NewWowInterface, FlagNode.Position, 40);
                            IEnumerable<WowPlayer> friendsNearFlag = WowInterface.Objects.GetNearFriends<WowPlayer>(WowInterface.NewWowInterface, FlagNode.Position, 40);
                            IEnumerable<WowPlayer> friendsNearPlayer = WowInterface.Objects.GetNearFriends<WowPlayer>(WowInterface.NewWowInterface, WowInterface.Player.Position, 20);

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
        }

        public void Faction()
        {
            if (!WowInterface.Player.IsHorde())
            {
                factionFlagState = "Alliance Controlled";
            }
            else
            {
                factionFlagState = "Hord Controlled";
            }
        }

        public void Leave()
        {
        }
    }
}